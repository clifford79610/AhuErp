using System;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Вложение документа с поддержкой версионирования. Несколько записей
    /// относятся к одному «логическому» вложению через общий <see cref="AttachmentGroupId"/>;
    /// текущая (актуальная) версия помечается флагом <see cref="IsCurrentVersion"/>.
    /// Файлы хранятся через абстракцию <see cref="Services.IFileStorageService"/>;
    /// поле <see cref="StoragePath"/> — относительный ключ внутри хранилища
    /// (например, <c>Documents/2026/АХУ-01-2026-00037/v1/scan.pdf</c>).
    /// </summary>
    public class DocumentAttachment
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }

        /// <summary>
        /// Идентификатор «логического» вложения (объединяет версии одного файла).
        /// Для первой версии заполняется тем же значением, что <see cref="Id"/>,
        /// либо отдельным GUID — здесь используется int-токен, выдаваемый сервисом.
        /// </summary>
        public int AttachmentGroupId { get; set; }

        [Required]
        [StringLength(512)]
        public string FileName { get; set; }

        [Required]
        [StringLength(1024)]
        public string StoragePath { get; set; }

        public int VersionNumber { get; set; } = 1;

        public bool IsCurrentVersion { get; set; } = true;

        public DateTime UploadedAt { get; set; }

        public int UploadedById { get; set; }
        public virtual Employee UploadedBy { get; set; }

        [StringLength(1024)]
        public string Comment { get; set; }

        /// <summary>SHA-256 хэш содержимого (hex), пригодный для контроля целостности.</summary>
        [StringLength(128)]
        public string Hash { get; set; }

        public AttachmentKind FileType { get; set; } = AttachmentKind.Draft;

        public long SizeBytes { get; set; }
    }
}
