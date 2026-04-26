using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Этап шаблонного маршрута согласования.
    /// Этап определяется ролью или конкретным сотрудником и порядком исполнения;
    /// несколько этапов с одинаковым <see cref="Order"/> = параллельная группа.
    /// </summary>
    public class ApprovalStage
    {
        public int Id { get; set; }

        public int RouteTemplateId { get; set; }
        public virtual ApprovalRouteTemplate RouteTemplate { get; set; }

        /// <summary>Порядковый номер этапа.</summary>
        public int Order { get; set; }

        /// <summary>Признак параллельного исполнения (внутри своего <see cref="Order"/>).</summary>
        public bool IsParallel { get; set; }

        /// <summary>Конкретный согласующий, если он закреплён в шаблоне.</summary>
        public int? ApproverEmployeeId { get; set; }
        public virtual Employee ApproverEmployee { get; set; }

        /// <summary>Альтернатива: согласующий определяется ролью.</summary>
        public EmployeeRole? ApproverRole { get; set; }

        [StringLength(512)]
        public string Description { get; set; }
    }
}
