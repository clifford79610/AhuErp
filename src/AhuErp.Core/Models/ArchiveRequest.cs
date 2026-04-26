using System;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Архивный запрос на выдачу справки, выписки или копии документов.
    /// </summary>
    public class ArchiveRequest : Document
    {
        /// <summary>
        /// Регламентный срок выдачи архивных справок и выписок (в рабочих днях).
        /// </summary>
        public const int DefaultDeadlineDays = 30;

        /// <summary>
        /// Регламентный срок выдачи копий муниципальных правовых актов (в рабочих днях).
        /// </summary>
        public const int MunicipalLegalActCopyDeadlineDays = 15;

        /// <summary>
        /// Срок перенаправления непрофильного заявления в другой архив или организацию.
        /// </summary>
        public const int RedirectDeadlineDays = 7;

        public bool HasPassportScan { get; set; }

        public bool HasWorkBookScan { get; set; }

        public ArchiveRequestKind RequestKind { get; set; } = ArchiveRequestKind.SocialLegal;

        public ArchiveRequest()
        {
            Type = DocumentType.Archive;
        }

        public void InitializeDeadline(DateTime creationDate)
        {
            CreationDate = creationDate;
            Deadline = creationDate.AddDays(GetDeadlineDays(RequestKind));
        }

        /// <summary>
        /// Запрос может быть закрыт только при полном комплекте скан-копий.
        /// </summary>
        public bool CanCompleteRequest()
        {
            if (RequestKind == ArchiveRequestKind.SocialLegal)
            {
                return HasPassportScan && HasWorkBookScan;
            }

            return true;
        }

        private static int GetDeadlineDays(ArchiveRequestKind kind)
        {
            return kind == ArchiveRequestKind.MunicipalLegalActCopy
                ? MunicipalLegalActCopyDeadlineDays
                : DefaultDeadlineDays;
        }
    }
}
