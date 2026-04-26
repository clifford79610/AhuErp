using System;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Связь документа с делом номенклатуры (M:N). У документа может быть
    /// «основное» дело (см. <see cref="Document.NomenclatureCaseId"/>) и
    /// дополнительные дела (например, тематические подшивки, контрольные дела).
    /// </summary>
    public class DocumentCaseLink
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }

        public int NomenclatureCaseId { get; set; }
        public virtual NomenclatureCase NomenclatureCase { get; set; }

        public DateTime LinkedAt { get; set; }

        public int? LinkedById { get; set; }
        public virtual Employee LinkedBy { get; set; }

        /// <summary>Признак основного дела.</summary>
        public bool IsPrimary { get; set; }
    }
}
