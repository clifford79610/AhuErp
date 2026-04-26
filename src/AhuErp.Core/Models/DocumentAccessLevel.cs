namespace AhuErp.Core.Models
{
    /// <summary>
    /// Гриф ограничения доступа к документу.
    /// Используется для фильтрации в журналах и контроля выдачи вложений.
    /// </summary>
    public enum DocumentAccessLevel
    {
        /// <summary>Без ограничения доступа.</summary>
        Public = 0,

        /// <summary>Для служебного пользования.</summary>
        Internal = 1,

        /// <summary>Конфиденциально / ограниченный доступ.</summary>
        Confidential = 2
    }
}
