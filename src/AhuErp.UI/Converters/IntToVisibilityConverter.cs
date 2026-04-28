using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AhuErp.UI.Converters
{
    /// <summary>
    /// Скрывает элемент, если число равно нулю; показывает при ненулевом
    /// (для бейджа уведомлений: «0 непрочитанных» — индикатор не светится).
    /// </summary>
    public sealed class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;
            try
            {
                var asInt = System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
                return asInt != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
