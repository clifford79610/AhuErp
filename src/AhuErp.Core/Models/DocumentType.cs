namespace AhuErp.Core.Models
{
    /// <summary>
    /// Тип документа, используется для фильтрации по подсистемам.
    /// Значения сохраняются как int — добавление новых членов в конец
    /// не требует миграции EF6.
    /// </summary>
    public enum DocumentType
    {
        General = 0,
        Office = 1,
        Archive = 2,
        It = 3,
        Fleet = 4,

        /// <summary>Входящий документ (канцелярия).</summary>
        Incoming = 5,

        /// <summary>Внутренний документ (канцелярия).</summary>
        Internal = 6,

        /// <summary>Архивный запрос (используется <see cref="ArchiveRequest"/>).</summary>
        ArchiveRequest = 7
    }
}
