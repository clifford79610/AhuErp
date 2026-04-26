namespace AhuErp.Core.Models
{
    /// <summary>
    /// Тип вложения документа: проект, скан-копия подписанного оригинала и т.д.
    /// </summary>
    public enum AttachmentKind
    {
        /// <summary>Проект документа (черновик).</summary>
        Draft = 0,

        /// <summary>Скан-копия (оригинал/подписанный экземпляр).</summary>
        Scan = 1,

        /// <summary>Электронная подпись / подписанный электронным образом.</summary>
        Signed = 2,

        /// <summary>Прочие приложения.</summary>
        Other = 3
    }
}
