namespace SignalGeneratorApp.Services.NoiseGenerations;

/// <summary>
/// Определяет контракт для стратегии генерации шума.
/// </summary>
public interface INoiseGenerationStrategy
{
    /// <summary>
    /// Вычисляет амплитуду шума на основе амплитуды сигнала и уровня шума.
    /// </summary>
    /// <param name="amplitude">Амплитуда исходного сигнала.</param>
    /// <param name="noiseLevel">Уровень шума в процентах (0–100).</param>
    /// <returns>Амплитуда шума в абсолютных единицах.</returns>
    double GetNoiseAmplitude(double amplitude, int noiseLevel);

    /// <summary>
    /// Генерирует одно значение шума на основе заданной амплитуды шума и генератора случайных чисел.
    /// </summary>
    /// <param name="noiseAmplitude">Максимальная амплитуда шума (масштаб).</param>
    /// <param name="random">Экземпляр генератора случайных чисел.</param>
    /// <returns>Сгенерированное значение шума.</returns>
    double Generate(double noiseAmplitude, Random random);
}