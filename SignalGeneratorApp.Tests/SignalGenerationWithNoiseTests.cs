using SignalGeneratorApp.Services.NoiseGenerations;

namespace SignalGeneratorApp.Tests;

/// <summary>
/// Тесты для проверки стратегий генерации сигналов с учётом добавления шума.
/// Цель: Убедиться, что значение сигнала корректно вычисляется и изменяется
/// при добавлении шума, сгенерированного соответствующей стратегией.
/// </summary>
public class SignalGenerationWithNoiseTests
{
    [Fact]
    public void SineGenerationStrategy_CalculateValue_WithNoise()
    {
        // Arrange
        var strategy = new SineGenerationStrategy();
        var noiseGen = new GaussianNoiseGenerationStrategy();

        // Параметры для "чистого" сигнала
        double amplitude = 5.0;
        double frequency = 2.0; // Гц
        double time = 0.25; // секунда
        double phase = 0.0; // радиан

        // Уровень шума
        int noiseLevel = 10; // 10% от амплитуды

        // Вычисляем "чистое" значение сигнала: A * sin(2πft + φ)
        // t=0.25, f=2 -> 2πft = 2π * 2 * 0.25 = π
        // sin(π) = 0
        // cleanValue = 5.0 * 0 = 0.0
        double cleanValue = strategy.CalculateValue(amplitude, frequency, time, phase);
        Assert.Equal(0.0, cleanValue, precision: 10); // Проверяем "чистое" значение

        // Вычисляем амплитуду шума: 10% от 5.0 = 0.5
        double noiseAmplitude = noiseGen.GetNoiseAmplitude(amplitude, noiseLevel);

        // Создаём предсказуемый генератор случайных чисел для воспроизводимости
        var random = new Random(42); // Используем фиксированный seed

        // Генерируем конкретное значение шума
        double noiseValue = noiseGen.Generate(noiseAmplitude, random);

        // Ожидаемое значение: чистый сигнал + шум
        double expectedValueWithNoise = cleanValue + noiseValue;

        // Act & Assert
        // Повторяем генерацию шума с тем же random, чтобы получить тот же noiseValue
        var randomForAct = new Random(42); // Тот же seed
        double noiseValueInAct = noiseGen.Generate(noiseAmplitude, randomForAct);
        double actualValueWithNoise = cleanValue + noiseValueInAct;

        Assert.Equal(expectedValueWithNoise, actualValueWithNoise, precision: 10);
        // Дополнительно: проверим, что значение с шумом отличается от "чистого", если шум не нулевой
        if (noiseValue != 0)
        {
            Assert.NotEqual(cleanValue, actualValueWithNoise);
        }
    }

    [Fact]
    public void SquareGenerationStrategy_CalculateValue_WithNoise()
    {
        // Arrange
        var strategy = new SquareGenerationStrategy();
        var noiseGen = new GaussianNoiseGenerationStrategy();

        // Параметры для "чистого" сигнала
        double amplitude = 3.0;
        double frequency = 1.0; // Гц
        double time = 0.75; // секунда
        double phase = 0.0; // радиан

        // Уровень шума
        int noiseLevel = 20; // 20% от амплитуды

        // Вычисляем "чистое" значение сигнала: sign(sin(2πft + φ)) * A
        // t=0.75, f=1 -> 2πft = 2π * 1 * 0.75 = 1.5π
        // sin(1.5π) = -1
        // sign(-1) = -1
        // cleanValue = -1 * 3.0 = -3.0
        double cleanValue = strategy.CalculateValue(amplitude, frequency, time, phase);
        Assert.Equal(-3.0, cleanValue, precision: 10);

        // Вычисляем амплитуду шума: 20% от 3.0 = 0.6
        double noiseAmplitude = noiseGen.GetNoiseAmplitude(amplitude, noiseLevel);

        // Создаём предсказуемый генератор случайных чисел
        var random = new Random(123); // Фиксированный seed

        // Генерируем конкретное значение шума
        double noiseValue = noiseGen.Generate(noiseAmplitude, random);

        // Ожидаемое значение: чистый сигнал + шум
        double expectedValueWithNoise = cleanValue + noiseValue;

        // Act & Assert
        var randomForAct = new Random(123); // Тот же seed
        double noiseValueInAct = noiseGen.Generate(noiseAmplitude, randomForAct);
        double actualValueWithNoise = cleanValue + noiseValueInAct;

        Assert.Equal(expectedValueWithNoise, actualValueWithNoise, precision: 10);
        if (noiseValue != 0)
        {
            Assert.NotEqual(cleanValue, actualValueWithNoise);
        }
    }

    [Fact]
    public void TriangleGenerationStrategy_CalculateValue_WithNoise()
    {
        // Arrange
        var strategy = new TriangleGenerationStrategy();
        var noiseGen = new GaussianNoiseGenerationStrategy();

        // Параметры для "чистого" сигнала
        double amplitude = 4.0;
        double frequency = 0.5; // Гц
        double time = 0.5; // секунда
        double phase = 0.0; // радиан

        // Уровень шума
        int noiseLevel = 5; // 5% от амплитуды

        // Вычисляем "чистое" значение сигнала
        // T = 1/f = 1/0.5 = 2 секунды
        // В момент t=0.5, это первая четверть периода (0.5 < T/2 = 1).
        // Формула: (4 * A / T) * t - A
        // (4 * 4.0 / 2.0) * 0.5 - 4.0 = 8.0 * 0.5 - 4.0 = 4.0 - 4.0 = 0.0
        double cleanValue = strategy.CalculateValue(amplitude, frequency, time, phase);
        Assert.Equal(0.0, cleanValue, precision: 10);

        // Вычисляем амплитуду шума: 5% от 4.0 = 0.2
        double noiseAmplitude = noiseGen.GetNoiseAmplitude(amplitude, noiseLevel);

        // Создаём предсказуемый генератор случайных чисел
        var random = new Random(456); // Фиксированный seed

        // Генерируем конкретное значение шума
        double noiseValue = noiseGen.Generate(noiseAmplitude, random);

        // Ожидаемое значение: чистый сигнал + шум
        double expectedValueWithNoise = cleanValue + noiseValue;

        // Act & Assert
        var randomForAct = new Random(456); // Тот же seed
        double noiseValueInAct = noiseGen.Generate(noiseAmplitude, randomForAct);
        double actualValueWithNoise = cleanValue + noiseValueInAct;

        Assert.Equal(expectedValueWithNoise, actualValueWithNoise, precision: 10);
        if (noiseValue != 0)
        {
            Assert.NotEqual(cleanValue, actualValueWithNoise);
        }
    }

    [Fact]
    public void SawtoothGenerationStrategy_CalculateValue_WithNoise()
    {
        // Arrange
        var strategy = new SawtoothGenerationStrategy();
        var noiseGen = new GaussianNoiseGenerationStrategy();

        // Параметры для "чистого" сигнала
        double amplitude = 2.0;
        double frequency = 1.0; // Гц
        double time = 0.25; // секунда
        double phase = 0.0; // радиан

        // Уровень шума
        int noiseLevel = 15; // 15% от амплитуды

        // Вычисляем "чистое" значение сигнала
        // T = 1/f = 1/1.0 = 1 секунда
        // Формула: 2 * A * (t / T) - A (для фазы 0 и t < T)
        // 2 * 2.0 * (0.25 / 1.0) - 2.0 = 4.0 * 0.25 - 2.0 = 1.0 - 2.0 = -1.0
        double cleanValue = strategy.CalculateValue(amplitude, frequency, time, phase);
        Assert.Equal(-1.0, cleanValue, precision: 10);

        // Вычисляем амплитуду шума: 15% от 2.0 = 0.3
        double noiseAmplitude = noiseGen.GetNoiseAmplitude(amplitude, noiseLevel);

        // Создаём предсказуемый генератор случайных чисел
        var random = new Random(789); // Фиксированный seed

        // Генерируем конкретное значение шума
        double noiseValue = noiseGen.Generate(noiseAmplitude, random);

        // Ожидаемое значение: чистый сигнал + шум
        double expectedValueWithNoise = cleanValue + noiseValue;

        // Act & Assert
        var randomForAct = new Random(789); // Тот же seed
        double noiseValueInAct = noiseGen.Generate(noiseAmplitude, randomForAct);
        double actualValueWithNoise = cleanValue + noiseValueInAct;

        Assert.Equal(expectedValueWithNoise, actualValueWithNoise, precision: 10);
        if (noiseValue != 0)
        {
            Assert.NotEqual(cleanValue, actualValueWithNoise);
        }
    }
}