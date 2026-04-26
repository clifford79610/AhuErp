using System;
using System.IO;
using System.Linq;
using AhuErp.Core.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Реализация <see cref="IReportService"/>. Использует ClosedXML для XLSX
    /// и DocumentFormat.OpenXml для DOCX — оба работают на чистом .NET без
    /// установленного MS Office.
    /// </summary>
    public sealed class ReportService : IReportService
    {
        private readonly IInventoryRepository _inventory;
        private readonly IDocumentRepository _documents;

        public ReportService(IInventoryRepository inventory, IDocumentRepository documents)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _documents = documents ?? throw new ArgumentNullException(nameof(documents));
        }

        public void ExportInventoryToExcel(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу обязателен.", nameof(filePath));

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Склад ТМЦ");

                sheet.Cell(1, 1).Value = "№";
                sheet.Cell(1, 2).Value = "Наименование";
                sheet.Cell(1, 3).Value = "Категория";
                sheet.Cell(1, 4).Value = "Остаток";

                var header = sheet.Range(1, 1, 1, 4);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

                var row = 2;
                foreach (var item in _inventory.ListItems().OrderBy(i => i.Name))
                {
                    sheet.Cell(row, 1).Value = item.Id;
                    sheet.Cell(row, 2).Value = item.Name;
                    sheet.Cell(row, 3).Value = FormatCategory(item.Category);
                    sheet.Cell(row, 4).Value = item.TotalQuantity;
                    row++;
                }

                sheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

        public void GenerateArchiveCertificate(int archiveRequestId, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу обязателен.", nameof(filePath));

            var document = _documents.GetById(archiveRequestId) as ArchiveRequest
                ?? throw new InvalidOperationException(
                    $"Архивный запрос #{archiveRequestId} не найден.");

            using (var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                var main = doc.AddMainDocumentPart();
                main.Document = new W.Document();
                var body = main.Document.AppendChild(new W.Body());

                body.AppendChild(Paragraph(OrganizationProfile.FullName));
                body.AppendChild(Paragraph(OrganizationProfile.ArchiveDepartmentName));
                body.AppendChild(Paragraph(OrganizationProfile.ArchiveAddress));
                body.AppendChild(Paragraph($"Телефон: {OrganizationProfile.ArchivePhone}; e-mail: {OrganizationProfile.ArchiveEmail}"));
                body.AppendChild(Paragraph(string.Empty));
                body.AppendChild(Heading("АРХИВНАЯ СПРАВКА"));
                body.AppendChild(Paragraph($"по архивному запросу №{document.Id} от {document.CreationDate:dd.MM.yyyy}"));
                body.AppendChild(Paragraph(string.Empty));
                body.AppendChild(Paragraph($"Вид запроса: {FormatArchiveRequestKind(document.RequestKind)}"));
                body.AppendChild(Paragraph($"Тема запроса: {document.Title}"));
                body.AppendChild(Paragraph($"Срок исполнения: {document.Deadline:dd.MM.yyyy}"));
                body.AppendChild(Paragraph(string.Empty));

                var passport = document.HasPassportScan ? "приложен" : "не приложен";
                var workBook = document.HasWorkBookScan ? "приложена" : "не приложена";
                body.AppendChild(Paragraph($"Скан паспорта: {passport}."));
                body.AppendChild(Paragraph($"Скан трудовой книжки: {workBook}."));
                body.AppendChild(Paragraph(string.Empty));

                if (document.HasPassportScan && document.HasWorkBookScan)
                {
                    body.AppendChild(Paragraph(
                        "Настоящим подтверждается, что документы представлены в полном объёме. " +
                        "Архивная справка, выписка или копия подготовлена для выдачи заявителю."));
                }
                else
                {
                    body.AppendChild(Paragraph(
                        "Для выдачи архивной справки необходимо дополнительно представить " +
                        "отсутствующие документы, после чего запрос будет обработан повторно."));
                }

                body.AppendChild(Paragraph(string.Empty));
                body.AppendChild(Paragraph($"Начальник архивного отдела _________________________ {OrganizationProfile.ArchiveHeadShortName}"));
                body.AppendChild(Paragraph($"Дата оформления: {DateTime.Now:dd.MM.yyyy}"));

                main.Document.Save();
            }
        }

        private static string FormatArchiveRequestKind(ArchiveRequestKind kind)
        {
            switch (kind)
            {
                case ArchiveRequestKind.SocialLegal:
                    return "социально-правовой запрос";
                case ArchiveRequestKind.Thematic:
                    return "тематический запрос";
                case ArchiveRequestKind.MunicipalLegalActCopy:
                    return "копия муниципального правового акта";
                case ArchiveRequestKind.PaidThematic:
                    return "платный тематический запрос";
                default:
                    return kind.ToString();
            }
        }

        private static string FormatCategory(InventoryCategory category)
        {
            switch (category)
            {
                case InventoryCategory.Stationery:
                    return "Канцелярские товары и бланки";
                case InventoryCategory.IT_Equipment:
                    return "Оргтехника, расходные материалы и связь";
                case InventoryCategory.Cleaning_Supplies:
                    return "Хозяйственные и эксплуатационные материалы";
                default:
                    return category.ToString();
            }
        }

        private static W.Paragraph Paragraph(string text)
        {
            var p = new W.Paragraph();
            var run = p.AppendChild(new W.Run());
            run.AppendChild(new W.Text(text) { Space = SpaceProcessingModeValues.Preserve });
            return p;
        }

        private static W.Paragraph Heading(string text)
        {
            var p = new W.Paragraph();
            var props = p.AppendChild(new W.ParagraphProperties());
            props.AppendChild(new W.Justification { Val = W.JustificationValues.Center });
            var run = p.AppendChild(new W.Run());
            var runProps = run.AppendChild(new W.RunProperties());
            runProps.AppendChild(new W.Bold());
            runProps.AppendChild(new W.FontSize { Val = "32" });
            run.AppendChild(new W.Text(text) { Space = SpaceProcessingModeValues.Preserve });
            return p;
        }
    }
}
