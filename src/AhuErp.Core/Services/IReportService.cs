namespace AhuErp.Core.Services
{
    /// <summary>
    /// Сервис выгрузки табличных отчётов и формальных документов. Абстрагирует
    /// ViewModel от ClosedXML / OpenXML, чтобы UI оставался свободным от
    /// зависимостей на форматы файлов.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Формирует XLSX-файл со списком всех позиций ТМЦ (ID, Наименование,
        /// Категория, Остаток) с отформатированной шапкой и автошириной колонок.
        /// </summary>
        /// <param name="filePath">Целевой путь для записи файла.</param>
        void ExportInventoryToExcel(string filePath);

        /// <summary>
        /// Генерирует Word-справку (DOCX) по архивному запросу: подставляет
        /// номер, дату, тему, статусы сканов и формальный текст ответа.
        /// </summary>
        /// <param name="archiveRequestId">Идентификатор <see cref="Models.ArchiveRequest"/>.</param>
        /// <param name="filePath">Целевой путь для записи файла.</param>
        void GenerateArchiveCertificate(int archiveRequestId, string filePath);
    }
}
