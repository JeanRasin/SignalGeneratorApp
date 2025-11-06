using SignalGeneratorApp.Extensions;
using SignalGeneratorApp.Models;
using SignalGeneratorApp.ViewModels;
using System.Globalization;

namespace SignalGeneratorApp.Services.Formatting;

/// <summary>
/// Реализация форматирования информации о сигнале.
/// </summary>
public class SignalInfoFormatter : ISignalInfoFormatter
{
    private const string EmptySignalText = "Нет текущего сигнала.";

    /// <inheritdoc/>
    public string Format(Signal? signal)
    {
        if (signal is null)
            return EmptySignalText;

        // Форматирование даты: "2 ноября 2025 [14:30]"
        // Используем культуру по умолчанию (например, ru-RU), но можно параметризовать
        var createdAtFormatted = signal.CreatedAt.ToString("d MMMM yyyy [HH:mm]", CultureInfo.CurrentCulture);

        var pointCountText = FormatPointCount(signal.Points.Count);

        return
            $"Тип: {signal.Type.GetDescription()} · " +
            $"Амплитуда: {signal.Amplitude} В · " +
            $"Частота: {signal.Frequency} Гц · " +
            $"Фаза: {signal.Phase} рад\n" +
            $"{pointCountText} · " +
            $"Длительность: {signal.TimeInterval} с · " +
            $"Дата: {createdAtFormatted}";
    }

    /// <summary>
    /// Форматирует количество точек с учётом ограничения на отображение.
    /// </summary>
    /// <param name="pointCount">Фактическое количество точек.</param>
    /// <returns>Форматированная строка.</returns>
    private static string FormatPointCount(int pointCount)
    {
        const int MaxDisplayPoints = SignalViewModel.MaxDisplayPoints;

        return pointCount >= MaxDisplayPoints
            ? $"Точек: {pointCount:N0} (на графике: {MaxDisplayPoints:N0})"
            : $"Точек: {pointCount:N0}";
    }
}