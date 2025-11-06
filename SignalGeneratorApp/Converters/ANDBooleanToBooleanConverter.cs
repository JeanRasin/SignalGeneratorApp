using System.Globalization;
using System.Windows.Data;

namespace SignalGeneratorApp.Converters;

public class ANDBooleanToBooleanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // Проверяем, что массив значений не null и не пуст
        if (values == null || values.Length == 0)
            return false;

        // Проходим по всем значениям
        foreach (var value in values)
        {
            // Проверяем, является ли значение bool
            if (value is bool b)
            {
                // Если хотя бы одно значение false, результат всего выражения false
                if (!b)
                    return false;
            }
            else
            {
                // Если одно из значений не bool, результат false
                return false;
            }
        }
        // Если все значения были true, результат true
        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        // ConvertBack не нужен для IsEnabled
        throw new NotImplementedException("Обратное преобразование не реализовано для преобразователя ANDBooleanToBooleanConverter.");
    }
}