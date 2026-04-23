using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AhuErp.UI.Converters
{
    /// <summary>
    /// Преобразует <c>bool</c> в <see cref="Visibility"/>. Если
    /// <c>ConverterParameter=="Invert"</c> — логика инвертируется.
    /// Визуально скрывает пункты меню, недоступные текущему пользователю (RBAC).
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = value is bool b && b;
            if (IsInvert(parameter)) flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = value is Visibility v && v == Visibility.Visible;
            if (IsInvert(parameter)) visible = !visible;
            return visible;
        }

        private static bool IsInvert(object parameter) =>
            parameter is string s &&
            string.Equals(s, "Invert", StringComparison.OrdinalIgnoreCase);
    }
}
