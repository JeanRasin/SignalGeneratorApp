using System.Globalization;
using System.Windows.Data;
using SignalGeneratorApp.Extensions;

namespace SignalGeneratorApp.Converters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Enum enumValue ? enumValue.GetDescription() : value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Обратное преобразование не реализовано для преобразователя EnumToDescriptionConverter.");
    }
}