using System.ComponentModel;

namespace SignalGeneratorApp.Extensions;

/// <summary>
/// Статический вспомогательный класс, предоставляющий методы расширения для работы с перечислениями (<see cref="Enum"/>).
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Возвращает описание перечислимого значения, заданное с помощью атрибута <see cref="DescriptionAttribute"/>.
    /// Если атрибут не указан, возвращается строковое представление значения перечисления.
    /// </summary>
    /// <param name="value">Значение перечисления, для которого требуется получить описание.</param>
    /// <returns>Описание из атрибута <see cref="DescriptionAttribute"/> или имя значения перечисления, если атрибут отсутствует.</returns>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null) return value.ToString();

        var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(
            field, typeof(DescriptionAttribute));

        return attribute?.Description ?? value.ToString();
    }
}