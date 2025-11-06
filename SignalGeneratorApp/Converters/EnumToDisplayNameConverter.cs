using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace SignalGeneratorApp.Converters;

public class EnumToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            if (fieldInfo != null)
            {
                var displayNameAttribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();
                if (displayNameAttribute != null)
                {
                    return displayNameAttribute.DisplayName;
                }
            }
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Обратное преобразование не реализовано для преобразователя EnumToDisplayNameConverter.");
    }
}