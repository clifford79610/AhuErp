using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Поручение по документу — основная единица контроля исполнительской дисциплины.
    /// Поддерживает иерархию (подпоручения через <see cref="ParentTaskId"/>),
    /// соисполнителей (<see cref="CoExecutors"/>) и контролёра.
    /// Завершение поручения может автоматически порождать хозяйственную операцию
    /// (см. <see cref="Services.IWorkflowService"/>).
    /// </summary>
    public class DocumentTask
    {
        public int Id { get; set; }

        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }

        public int? ResolutionId { get; set; }
        public virtual DocumentResolution Resolution { get; set; }

        public int? ParentTaskId { get; set; }
        public virtual DocumentTask ParentTask { get; set; }

        public virtual ICollection<DocumentTask> ChildTasks { get; set; }
            = new HashSet<DocumentTask>();

        /// <summary>Автор поручения.</summary>
        public int AuthorId { get; set; }
        public virtual Employee Author { get; set; }

        /// <summary>Основной исполнитель.</summary>
        public int ExecutorId { get; set; }
        public virtual Employee Executor { get; set; }

        /// <summary>Контролёр исполнения (опционален).</summary>
        public int? ControllerId { get; set; }
        public virtual Employee Controller { get; set; }

        /// <summary>Соисполнители (ФИО через ;) — лёгкая денормализация для отчётов.</summary>
        [StringLength(1024)]
        public string CoExecutors { get; set; }

        [Required]
        [StringLength(2048)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime Deadline { get; set; }

        public DocumentTaskStatus Status { get; set; } = DocumentTaskStatus.New;

        public DateTime? CompletedAt { get; set; }

        [StringLength(2048)]
        public string ReportText { get; set; }

        /// <summary>Признак критичности (например, поручения мэра).</summary>
        public bool IsCritical { get; set; }

        /// <summary>Поручение просрочено, если срок прошёл и статус не финальный.</summary>
        public bool IsOverdue(DateTime now)
        {
            return Deadline < now
                && Status != DocumentTaskStatus.Completed
                && Status != DocumentTaskStatus.Cancelled;
        }
    }
}
