using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SignalGeneratorApp.Converters;

public class StringToNullableDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s))
            return null; 

        if (double.TryParse(s, NumberStyles.Float, culture, out var result))
            return result;

        return DependencyProperty.UnsetValue; // вызовет ошибку привязки
    }
}