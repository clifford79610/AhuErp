using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using ClosedXML.Excel;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// Тесты сервиса отчётов. Используем временные файлы в <c>Path.GetTempPath()</c>,
    /// а не реальный MS Office — ClosedXML и DocumentFormat.OpenXml работают
    /// на чистом .NET без Excel/Word на машине.
    /// </summary>
    public class ReportServiceTests : IDisposable
    {
        private readonly InMemoryInventoryRepository _inventory = new InMemoryInventoryRepository();
        private readonly InMemoryDocumentRepository _documents = new InMemoryDocumentRepository();
        private readonly ReportService _service;
        private readonly string _workdir;

        public ReportServiceTests()
        {
            _service = new ReportService(_inventory, _documents);
            _workdir = Path.Combine(Path.GetTempPath(), "AhuErpTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_workdir);

            _inventory.AddItem(new InventoryItem
            {
                Name = "Бумага А4 500 л.",
                Category = InventoryCategory.Stationery,
                TotalQuantity = 42
            });
            _inventory.AddItem(new InventoryItem
            {
                Name = "Картридж HP 59A",
                Category = InventoryCategory.IT_Equipment,
                TotalQuantity = 6
            });
        }

        public void Dispose()
        {
            try { Directory.Delete(_workdir, recursive: true); } catch { /* best-effort */ }
        }

        [Fact]
        public void ExportInventoryToExcel_writes_header_and_rows()
        {
            var path = Path.Combine(_workdir, "inv.xlsx");

            _service.ExportInventoryToExcel(path);

            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 0);

            using (var wb = new XLWorkbook(path))
            {
                var sheet = wb.Worksheet(1);
                Assert.Equal("Склад ТМЦ", sheet.Name);

                // Шапка.
                Assert.Equal("№", sheet.Cell(1, 1).GetString());
                Assert.Equal("Наименование", sheet.Cell(1, 2).GetString());
                Assert.Equal("Категория", sheet.Cell(1, 3).GetString());
                Assert.Equal("Остаток", sheet.Cell(1, 4).GetString());
                Assert.True(sheet.Cell(1, 1).Style.Font.Bold);

                // Две строки данных в алфавитном порядке: Бумага А4 → Картридж HP.
                Assert.Equal("Бумага А4 500 л.", sheet.Cell(2, 2).GetString());
                Assert.Equal(42, (int)sheet.Cell(2, 4).GetDouble());
                Assert.Equal("Картридж HP 59A", sheet.Cell(3, 2).GetString());
                Assert.Equal(6, (int)sheet.Cell(3, 4).GetDouble());
            }
        }

        [Fact]
        public void GenerateArchiveCertificate_produces_valid_docx_with_request_data()
        {
            var request = new ArchiveRequest
            {
                Title = "Запрос о стаже (Петров П.П.)",
                CreationDate = new DateTime(2026, 5, 1),
                Deadline = new DateTime(2026, 5, 31),
                HasPassportScan = true,
                HasWorkBookScan = true,
                Status = DocumentStatus.Completed
            };
            _documents.Add(request);
            var path = Path.Combine(_workdir, "cert.docx");

            _service.GenerateArchiveCertificate(request.Id, path);

            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 0);

            // Пакет OPC валиден: открывается без исключений и содержит word/document.xml с данными.
            using (var pkg = Package.Open(path, FileMode.Open, FileAccess.Read))
            {
                var docPart = pkg.GetParts()
                    .First(p => p.Uri.OriginalString.EndsWith("/word/document.xml", StringComparison.Ordinal));
                using (var sr = new StreamReader(docPart.GetStream()))
                {
                    var xml = sr.ReadToEnd();
                    Assert.Contains("СПРАВКА о стаже", xml);
                    Assert.Contains($"№{request.Id}", xml);
                    Assert.Contains("01.05.2026", xml);
                    Assert.Contains("Запрос о стаже", xml);
                    Assert.Contains("приложен", xml);
                    Assert.Contains("приложена", xml);
                    Assert.Contains("в полном объёме", xml);
                }
            }
        }

        [Fact]
        public void GenerateArchiveCertificate_missing_scans_renders_followup_text()
        {
            var request = new ArchiveRequest
            {
                Title = "Неполный запрос",
                CreationDate = new DateTime(2026, 4, 1),
                Deadline = new DateTime(2026, 5, 1),
                HasPassportScan = false,
                HasWorkBookScan = false
            };
            _documents.Add(request);
            var path = Path.Combine(_workdir, "cert-missing.docx");

            _service.GenerateArchiveCertificate(request.Id, path);

            using (var pkg = Package.Open(path, FileMode.Open, FileAccess.Read))
            {
                var docPart = pkg.GetParts()
                    .First(p => p.Uri.OriginalString.EndsWith("/word/document.xml", StringComparison.Ordinal));
                using (var sr = new StreamReader(docPart.GetStream()))
                {
                    var xml = sr.ReadToEnd();
                    Assert.Contains("не приложен", xml);
                    Assert.Contains("не приложена", xml);
                    Assert.Contains("отсутствующие документы", xml);
                    Assert.DoesNotContain("в полном объёме", xml);
                }
            }
        }

        [Fact]
        public void GenerateArchiveCertificate_throws_for_missing_request()
        {
            var path = Path.Combine(_workdir, "nope.docx");
            Assert.Throws<InvalidOperationException>(() =>
                _service.GenerateArchiveCertificate(9999, path));
        }
    }
}
