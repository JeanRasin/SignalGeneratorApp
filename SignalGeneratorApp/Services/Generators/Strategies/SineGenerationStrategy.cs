using SignalGeneratorApp.Services.Generators.Strategies;

/// <summary>
/// Стратегия генерации синусоидального сигнала.
/// Математически, значение сигнала в момент времени t вычисляется по формуле:
/// A * sin(2π * f * t + φ), где:
/// - A — амплитуда (peak amplitude),
/// - f — частота в герцах (Hz),
/// - t — время в секундах,
/// - φ — начальная фаза в радианах.
/// </summary>
public class SineGenerationStrategy : ISignalGenerationStrategy
{
    /// <inheritdoc/>
    public double CalculateValue(double amplitude, double frequency, double time, double phase = 0)
    {
        // Вычисляем значение синуса с учётом частоты, времени и начальной фазы.
        // Угловая частота ω = 2πf, общая фаза = ωt + φ.
        return amplitude * Math.Sin(2 * Math.PI * frequency * time + phase);
    }
}