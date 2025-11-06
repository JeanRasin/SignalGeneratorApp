using SignalGeneratorApp.Services.Generators.Strategies;

/// <summary>
/// Стратегия генерации пилообразного сигнала.
/// Сигнал линейно возрастает от <c>-amplitude</c> до <c>+amplitude</c> за один период (1/frequency),
/// после чего мгновенно сбрасывается обратно к <c>-amplitude</c> и цикл повторяется.
/// Математически: значение сигнала в момент времени t определяется как линейная интерполяция
/// внутри одного периода, с учётом начальной фазы <c>phase</c>.
/// </summary>
public class SawtoothGenerationStrategy : ISignalGenerationStrategy
{
    /// <inheritdoc/>
    public double CalculateValue(double amplitude, double frequency, double time, double phase = 0)
    {
        // Рассчитываем продолжительность одного периода T = 1 / f
        double period = 1.0 / frequency;

        // Преобразуем фазу (в радианах) во временной сдвиг.
        // Формула: φ = 2πf·Δt → Δt = φ / (2πf)
        // Это смещение времени, соответствующее заданной фазе.
        double timeShift = phase / (2 * Math.PI * frequency);

        // Приводим время к интервалу одного периода [0, T) с учётом сдвига фазы.
        // % - оператор остатка от деления, может давать отрицательный результат,
        // но в данном случае, учитывая физический смысл, результат будет в нужном диапазоне
        // или его можно нормализовать, если это вызывает сомнения.
        double t = (time + timeShift) % period;

        // Линейная интерполяция от -amplitude до +amplitude за время одного периода.
        // Формула: y = 2 * amplitude * (t / period) - amplitude
        // При t=0: y = -amplitude
        // При t=period: y = 2*amplitude - amplitude = +amplitude
        return 2 * amplitude * (t / period) - amplitude;
    }
}