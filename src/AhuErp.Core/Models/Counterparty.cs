using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Контрагент — внешняя организация / физическое лицо, фигурирующее
    /// в документообороте (отправитель/получатель писем, заявок и т.п.).
    /// Введён в Foundation extras как самостоятельный справочник.
    /// Связывание <see cref="Document.Correspondent"/> (свободная строка) с
    /// FK на эту сущность выполнит следующий PR — сейчас справочник лишь
    /// заполняется и используется в UI как источник подсказок.
    /// </summary>
    public class Counterparty
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string ShortName { get; set; }

        [StringLength(512)]
        public string FullName { get; set; }

        /// <summary>ИНН (10 для юр. лиц, 12 для ИП/физлиц).</summary>
        [StringLength(12)]
        public string Inn { get; set; }

        /// <summary>КПП (только у юр. лиц).</summary>
        [StringLength(9)]
        public string Kpp { get; set; }

        /// <summary>
        /// ОГРН — 13 цифр для юр.лиц; для ИП — ОГРНИП (15 цифр), поэтому
        /// длина поля задаётся 15 символов, чтобы покрыть оба формата.
        /// </summary>
        [StringLength(15)]
        public string Ogrn { get; set; }

        [StringLength(512)]
        public string Address { get; set; }

        [StringLength(64)]
        public string Phone { get; set; }

        [StringLength(256)]
        public string Email { get; set; }

        /// <summary>Тип контрагента: организация / ИП / физическое лицо.</summary>
        public CounterpartyKind Kind { get; set; } = CounterpartyKind.Organization;

        public bool IsActive { get; set; } = true;
    }

    public enum CounterpartyKind
    {
        Organization,
        SoleProprietor,
        Individual,
        GovernmentBody,
    }
}
