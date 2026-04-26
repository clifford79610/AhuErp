using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Шаблон маршрута согласования. Состоит из этапов
    /// (<see cref="ApprovalStage"/>) — параллельных или последовательных.
    /// При запуске согласования по документу шаблон «инстанцируется»
    /// в набор <see cref="DocumentApproval"/>.
    /// </summary>
    public class ApprovalRouteTemplate
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Опциональная привязка шаблона к виду документа (например, маршрут
        /// «Согласование акта списания» применим к видам «Акт списания»).
        /// </summary>
        public int? DocumentTypeRefId { get; set; }
        public virtual DocumentTypeRef DocumentTypeRef { get; set; }

        public virtual ICollection<ApprovalStage> Stages { get; set; }
            = new HashSet<ApprovalStage>();
    }
}
