using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Справочник видов документов: «Приказ», «Служебная записка», «Акт списания»,
    /// «Путевой лист», «IT-заявка» и т.п. Сущность отделена от enum-а
    /// <see cref="DocumentType"/> (тот используется для модульной фильтрации в UI):
    /// здесь хранится профессиональная классификация делопроизводства,
    /// привязанная к номенклатуре дел и шаблонам регистрационных номеров.
    /// </summary>
    public class DocumentTypeRef
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        /// <summary>Краткий код вида (например, «ПР» для приказа).</summary>
        [StringLength(16)]
        public string ShortCode { get; set; }

        public DocumentDirection DefaultDirection { get; set; } = DocumentDirection.Internal;

        /// <summary>Срок хранения по умолчанию (лет), используется при привязке к делу.</summary>
        public int DefaultRetentionYears { get; set; } = 5;

        /// <summary>Шаблон регистрационного номера (см. <see cref="Services.INomenclatureService"/>).</summary>
        [StringLength(128)]
        public string RegistrationNumberTemplate { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Document> Documents { get; set; } = new HashSet<Document>();
    }
}
