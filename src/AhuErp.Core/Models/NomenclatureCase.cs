using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Дело номенклатуры (заголовок дела по форме номенклатуры МКУ АХУ БМР).
    /// Соответствует ГОСТ Р 7.0.8-2013 и Типовой инструкции по делопроизводству:
    /// каждое дело имеет индекс, заголовок, отдел-владельца, статью по перечню
    /// и срок хранения.
    /// </summary>
    public class NomenclatureCase
    {
        public int Id { get; set; }

        /// <summary>Индекс дела по номенклатуре, например «01-07».</summary>
        [Required]
        [StringLength(32)]
        public string Index { get; set; }

        [Required]
        [StringLength(512)]
        public string Title { get; set; }

        public int? DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        /// <summary>Срок хранения (лет). 0 — постоянно.</summary>
        public int RetentionPeriodYears { get; set; }

        /// <summary>Статья по перечню типовых документов.</summary>
        [StringLength(64)]
        public string Article { get; set; }

        /// <summary>Год номенклатуры (например, 2026).</summary>
        public int Year { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<DocumentCaseLink> DocumentLinks { get; set; }
            = new HashSet<DocumentCaseLink>();
    }
}
