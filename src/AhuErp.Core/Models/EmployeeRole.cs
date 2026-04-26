namespace AhuErp.Core.Models
{
    /// <summary>
    /// Роль сотрудника, используется для контроля доступа к подсистемам ERP/EDMS.
    /// </summary>
    public enum EmployeeRole
    {
        /// <summary>Системный администратор — полный доступ.</summary>
        Admin = 0,

        /// <summary>Руководитель учреждения или службы — полный функциональный доступ.</summary>
        Manager = 1,

        /// <summary>Сотрудник архивного отдела — работает с архивными запросами.</summary>
        Archivist = 2,

        /// <summary>Специалист службы по информационно-техническому обеспечению.</summary>
        TechSupport = 3,

        /// <summary>Ответственный за ТМЦ, транспортные заявки и хозяйственные операции.</summary>
        WarehouseManager = 4
    }
}
