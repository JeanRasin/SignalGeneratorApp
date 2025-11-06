using SignalGeneratorApp.Services.Generators.Strategies;

/// <summary>
/// Стратегия генерации треугольного сигнала.
/// Сигнал линейно возрастает от <c>-amplitude</c> до <c>+amplitude</c>,
/// затем линейно убывает обратно до <c>-amplitude</c>, и цикл повторяется.
/// Математически, это модифицированный пилообразный сигнал, к которому применена функция модуля
/// и соответствующее масштабирование, чтобы получить симметричную треугольную форму.
/// </summary>
public class TriangleGenerationStrategy : ISignalGenerationStrategy
{
    /// <inheritdoc/>
    public double CalculateValue(double amplitude, double frequency, double time, double phase = 0)
    {
        // Рассчитываем продолжительность одного периода T = 1 / f
        double period = 1.0 / frequency;

        // Преобразуем фазу (в радианах) во временной сдвиг.
        // Формула: φ = 2πf·Δt → Δt = φ / (2πf)
        double timeShift = phase / (2 * Math.PI * frequency);

        // Приводим время к интервалу одного периода [0, T) с учётом сдвига фазы.
        double t = (time + timeShift) % period;
        if (t < 0) t += period; // Нормализуем отрицательный остаток, если применимо.

        // Преобразуем линейно растущее значение (пилообразный сигнал) в треугольный.
        // Формула: y = (4 * amplitude / T) * |(t % T) - T/2| - amplitude
        // Однако, для более прямолинейного подхода:
        // Если t < T/2 (первая половина периода), сигнал растёт от -amplitude к +amplitude.
        // Если t >= T/2 (вторая половина), сигнал падает от +amplitude к -amplitude.
        double halfPeriod = period / 2.0;

        // Сдвигаем и масштабируем время для линейного роста/падения
        // В первой половине: 0 -> 1 (от -amplitude к +amplitude)
        // Во второй половине: 0 -> -1 (от +amplitude к -amplitude)
        if (t < halfPeriod)
        {
            // Рост: от -amplitude до +amplitude за halfPeriod
            // Формула: y = (2 * amplitude / halfPeriod) * t - amplitude
            // Упрощаем: (2 * amplitude / (T/2)) * t - amplitude = (4 * amplitude / T) * t - amplitude
            return (4 * amplitude / period) * t - amplitude;
        }
        else
        {
            // Падение: от +amplitude до -amplitude за halfPeriod
            // t' = t - halfPeriod (относительное время во второй половине)
            // y = - (2 * amplitude / halfPeriod) * t' + amplitude
            // Упрощаем: - (4 * amplitude / T) * t' + amplitude
            double t_prime = t - halfPeriod;
            return -(4 * amplitude / period) * t_prime + amplitude;
        }
    }
}