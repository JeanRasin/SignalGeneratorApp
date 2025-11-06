using SignalGeneratorApp.Services.Generators.Strategies;

/// <summary>
/// Стратегия генерации прямоугольного (меандрового) сигнала.
/// Сигнал переключается между значениями <c>+amplitude</c> и <c>-amplitude</c>
/// с заданной частотой, сохраняя соотношение длительности импульса и паузы 1:1 (скважность 2).
/// Математически, значение определяется знаком синусоиды в заданный момент времени:
/// если sin(угол) > 0, то +amplitude, если sin(угол) < 0, то -amplitude.
/// </summary>
public class SquareGenerationStrategy : ISignalGenerationStrategy
{
    /// <inheritdoc/>
    public double CalculateValue(double amplitude, double frequency, double time, double phase = 0)
    {
        // Вычисляем общий угол (в радианах) включая начальную фазу.
        double angle = 2 * Math.PI * frequency * time + phase;

        // Используем Math.Sin для определения текущего "состояния" синусоиды.
        // Math.Sign возвращает -1 для отрицательных, 0 для нуля и +1 для положительных значений.
        // Умножаем результат на амплитуду, чтобы получить +amplitude или -amplitude.
        // Нулевое значение sin (теоретически возможное при идеальных условиях) будет давать 0,
        // что может быть корректным или требовать специальной обработки в зависимости от спецификации.
        // В большинстве случаев, оно будет отнесено к положительному или отрицательному уровню.
        return Math.Sign(Math.Sin(angle)) * amplitude;
    }
}