using SignalGeneratorApp.Models;

namespace SignalGeneratorApp.Services.Formatting;

/// <summary>
/// Форматирует информацию о сигнале для отображения в пользовательском интерфейсе.
/// </summary>
public interface ISignalInfoFormatter
{
    /// <summary>
    /// Форматирует информацию о сигнале.
    /// </summary>
    /// <param name="signal">Сигнал для форматирования. Может быть <c>null</c>.</param>
    /// <returns>Готовая к отображению строка.</returns>
    string Format(Signal? signal);
}