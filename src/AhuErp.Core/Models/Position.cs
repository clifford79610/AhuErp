using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Должность сотрудника. Введена в Foundation extras чтобы заменить
    /// свободную строку <c>Employee.Position</c> на справочный FK
    /// (нормализация наименований, отчёты по штатке).
    /// На переходный период строка остаётся для совместимости.
    /// </summary>
    public class Position
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        /// <summary>Категория (руководитель / специалист / рабочий и т.п.).</summary>
        [StringLength(64)]
        public string Category { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
