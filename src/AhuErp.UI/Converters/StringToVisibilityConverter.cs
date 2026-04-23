using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AhuErp.UI.Converters
{
    /// <summary>
    /// Возвращает <see cref="Visibility.Visible"/>, если строка непуста,
    /// иначе — <see cref="Visibility.Collapsed"/>.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Visibility))]
    public sealed class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            return string.IsNullOrWhiteSpace(s) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
