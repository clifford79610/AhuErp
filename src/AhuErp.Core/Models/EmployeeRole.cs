namespace AhuErp.Core.Models
{
    /// <summary>
    /// Роль сотрудника, используется для контроля доступа к подсистемам ERP/EDMS.
    /// </summary>
    public enum EmployeeRole
    {
        /// <summary>Системный администратор — полный доступ.</summary>
        Admin = 0,

        /// <summary>Руководитель АХУ — полный функциональный доступ.</summary>
        Manager = 1,

        /// <summary>Архивариус — работает только с архивными запросами.</summary>
        Archivist = 2,

        /// <summary>Специалист IT-службы — обрабатывает заявки хелпдеска.</summary>
        TechSupport = 3,

        /// <summary>Заведующий складом — распоряжается ТМЦ, автопарком, канцелярией.</summary>
        WarehouseManager = 4
    }
}
