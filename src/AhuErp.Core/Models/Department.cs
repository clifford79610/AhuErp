using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Структурное подразделение учреждения. Используется в номенклатуре дел
    /// (привязка дела к отделу) и в отчётах по исполнительской дисциплине.
    /// </summary>
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        /// <summary>Краткий код отдела для регистрационных индексов (например, «АХУ»).</summary>
        [StringLength(16)]
        public string ShortCode { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<NomenclatureCase> NomenclatureCases { get; set; }
            = new HashSet<NomenclatureCase>();
    }
}
