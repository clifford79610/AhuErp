using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using AhuErp.Core.Models;

namespace AhuErp.UI.Converters
{
    /// <summary>
    /// Двусторонний конвертер enum ↔ русская подпись. Ровно одно место,
    /// где описаны переводы, чтобы не плодить дубликаты по XAML/VM. Реальные
    /// значения в БД и в коде остаются английскими (привязаны к маппингу EF6
    /// и к тестам), пользователь видит локализованные строки.
    /// </summary>
    public sealed class EnumDisplayConverter : IValueConverter
    {
        private static readonly IReadOnlyDictionary<Enum, string> Map = new Dictionary<Enum, string>
        {
            [VehicleStatus.Available] = "Доступен",
            [VehicleStatus.OnMission] = "В рейсе",
            [VehicleStatus.Maintenance] = "На обслуживании",

            [DocumentStatus.New] = "Новый",
            [DocumentStatus.InProgress] = "В работе",
            [DocumentStatus.OnHold] = "Приостановлен",
            [DocumentStatus.Completed] = "Завершён",
            [DocumentStatus.Cancelled] = "Отменён",

            [DocumentType.General] = "Общий",
            [DocumentType.Office] = "Документационное обеспечение",
            [DocumentType.Archive] = "Архивный отдел",
            [DocumentType.It] = "ИТО",
            [DocumentType.Fleet] = "Транспорт",
            [DocumentType.Incoming] = "Входящий",
            [DocumentType.Internal] = "Внутренний",
            [DocumentType.ArchiveRequest] = "Архивный запрос",

            [ArchiveRequestKind.SocialLegal] = "Социально-правовой запрос",
            [ArchiveRequestKind.Thematic] = "Тематический запрос",
            [ArchiveRequestKind.MunicipalLegalActCopy] = "Копия муниципального правового акта",
            [ArchiveRequestKind.PaidThematic] = "Платный тематический запрос",

            [InventoryCategory.Stationery] = "Канцелярские товары и бланки",
            [InventoryCategory.IT_Equipment] = "Оргтехника, расходные материалы и связь",
            [InventoryCategory.Cleaning_Supplies] = "Хозяйственные и эксплуатационные материалы",

            [EmployeeRole.Admin] = "Администратор",
            [EmployeeRole.Manager] = "Руководитель / начальник службы",
            [EmployeeRole.Archivist] = "Сотрудник архивного отдела",
            [EmployeeRole.TechSupport] = "Специалист ИТО",
            [EmployeeRole.WarehouseManager] = "Ответственный за ТМЦ",
        };

        public static string Translate(Enum value) =>
            value != null && Map.TryGetValue(value, out var label) ? label : value?.ToString();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum e) return Translate(e);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Обратное преобразование не используется (ComboBox привязывает SelectedItem
            // к самому enum, а не к строке) — оставляем заглушку.
            throw new NotSupportedException();
        }
    }
}
