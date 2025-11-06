namespace SignalGeneratorApp.Services.Generators.Strategies;

/// <summary>
/// Определяет контракт для стратегии генерации сигнала.
/// Реализации этого интерфейса предоставляют логику вычисления мгновенного значения сигнала
/// (например, синусоидального, прямоугольного, треугольного и т.д.) в заданный момент времени.
/// </summary>
public interface ISignalGenerationStrategy
{
    /// <summary>
    /// Вычисляет мгновенное значение сигнала на основе заданных параметров.
    /// </summary>
    /// <param name="amplitude">Амплитуда сигнала — пиковое отклонение от нулевого уровня.</param>
    /// <param name="frequency">Частота сигнала в герцах (Hz) — количество полных циклов в секунду.</param>
    /// <param name="time">Момент времени (в секундах), для которого вычисляется значение сигнала.</param>
    /// <param name="phase">Начальная фаза сигнала в радианах (опционально, по умолчанию 0).</param>
    /// <returns>Значение сигнала в указанный момент времени, как правило в диапазоне от <c>-amplitude</c> до <c>+amplitude</c>.</returns>
    double CalculateValue(double amplitude, double frequency, double time, double phase = 0);
}