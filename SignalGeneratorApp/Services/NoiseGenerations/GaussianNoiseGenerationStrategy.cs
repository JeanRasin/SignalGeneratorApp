namespace SignalGeneratorApp.Services.NoiseGenerations;

/// <summary>
/// Реализация стратегии генерации шума, создающая нормально распределённый (гауссовский) шум.
/// </summary>
public class GaussianNoiseGenerationStrategy : INoiseGenerationStrategy
{
    // Константа 2 * π, используемая в формуле Бокса-Мюллера.
    private static double PI2 => 2.0 * Math.PI;

    /// <inheritdoc/>
    /// <summary>
    /// Вычисляет амплитуду шума на основе амплитуды сигнала и уровня шума в процентах.
    /// </summary>
    /// <param name="amplitude">Амплитуда основного сигнала.</param>
    /// <param name="noiseLevel">Уровень шума в процентах (0-100). Если 0 или отрицательный, возвращается 0.</param>
    /// <returns>Абсолютное значение амплитуды шума.</returns>
    public double GetNoiseAmplitude(double amplitude, int noiseLevel)
    {
        // Если уровень шума нулевой или отрицательный, шум не генерируется.
        if (noiseLevel <= 0)
            return 0.0;

        // Вычисляем амплитуду шума как процент от амплитуды сигнала.
        double noiseAmplitude = amplitude * (noiseLevel / 100.0);

        return noiseAmplitude;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Генерирует одно значение гауссовского шума с нулевым средним и стандартным отклонением,
    /// равным <paramref name="noiseAmplitude"/>.
    /// </summary>
    /// <param name="noiseAmplitude">Стандартное отклонение (масштаб) генерируемого шума.</param>
    /// <param name="random">Экземпляр генератора случайных чисел.</param>
    /// <returns>Сгенерированное значение шума.</returns>
    public double Generate(double noiseAmplitude, Random random)
    {
        // Генерация двух независимых равномерно распределённых значений [0, 1),
        // затем преобразование в нормальное распределение с помощью формулы Бокса-Мюллера.
        // Вычитание из 1.0 гарантирует, что значения u1 и u2 не будут равны 0.0,
        // что важно для корректной работы функции логарифма.
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();

        // Формула Бокса-Мюллера: Z0 = sqrt(-2 * ln(U1)) * cos(2 * π * U2)
        // В данном случае используется синус для получения одного значения из пары.
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(PI2 * u2);

        // Масштабирование стандартного нормального значения к требуемой амплитуде шума.
        double result = randStdNormal * noiseAmplitude;

        return result;
    }
}