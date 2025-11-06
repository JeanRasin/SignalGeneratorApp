namespace SignalGeneratorApp.Models;

/// <summary>
/// Параметры генерации сигнала.
/// Содержит все необходимые числовые настройки для создания сигнала.
/// </summary>
public class SignalParameter
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="SignalParameter"/> со значениями по умолчанию.
    /// </summary>
    public SignalParameter()
    {
        Amplitude = SignalDefaults.Amplitude;
        Frequency = SignalDefaults.Frequency;
        Phase = SignalDefaults.Phase;
        PointCount = SignalDefaults.PointCount;
        TimeInterval = SignalDefaults.TimeInterval;
        NoiseLevel = SignalDefaults.NoiseLevel;
    }

    /// <summary>
    /// Амплитуда сигнала (в условных единицах или вольтах).
    /// Должна быть неотрицательной.
    /// </summary>
    public double? Amplitude { get; set; }

    /// <summary>
    /// Частота сигнала в герцах (Гц).
    /// Определяет, сколько полных колебаний происходит за секунду.
    /// </summary>
    public double? Frequency { get; set; }

    /// <summary>
    /// Начальная фаза сигнала в радианах.
    /// Используется только для синусоидального сигнала.
    /// </summary>
    public double? Phase { get; set; }

    /// <summary>
    /// Количество точек дискретизации сигнала.
    /// Определяет разрешение временной сетки.
    /// </summary>
    public int? PointCount { get; set; }

    /// <summary>
    /// Общая длительность генерируемого сигнала в секундах.
    /// Должна быть положительной.
    /// </summary>
    public double? TimeInterval { get; set; }

    /// <summary>
    /// Уровень шума в процентах от амплитуды (0.0 = без шума, 1.0 = 100%).
    /// Внутренне используется как коэффициент (0.0–1.0).
    /// </summary>
    public int NoiseLevel { get; set; }
}