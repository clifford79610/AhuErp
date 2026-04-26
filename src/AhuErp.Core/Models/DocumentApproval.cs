using System;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Конкретный шаг согласования, привязанный к документу.
    /// Создаётся <see cref="Services.IApprovalService"/> при запуске
    /// маршрута и наполняется по мере прохождения.
    /// </summary>
    public class DocumentApproval
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }

        /// <summary>Связанный этап шаблона (для трассировки и журнала).</summary>
        public int? StageId { get; set; }
        public virtual ApprovalStage Stage { get; set; }

        public int Order { get; set; }

        public bool IsParallel { get; set; }

        public int ApproverId { get; set; }
        public virtual Employee Approver { get; set; }

        public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;

        [StringLength(2048)]
        public string Comment { get; set; }

        public DateTime? DecisionDate { get; set; }
    }
}
