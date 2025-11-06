using Moq;
using SignalGeneratorApp.Services.NoiseGenerations;

namespace SignalGeneratorApp.Tests;

/// <summary>
/// Тесты для стратегии генерации шума
/// </summary>
public class NoiseStrategyTests
{
    private readonly double _testAmplitude = 10.0;

    [Fact]
    public void GaussianNoise_GetNoiseAmplitude_ReturnsZeroForNegativeOrZeroLevel()
    {
        var strategy = new GaussianNoiseGenerationStrategy();

        Assert.Equal(0.0, strategy.GetNoiseAmplitude(_testAmplitude, -1));
        Assert.Equal(0.0, strategy.GetNoiseAmplitude(_testAmplitude, 0));
    }

    [Fact]
    public void GaussianNoise_GetNoiseAmplitude_CalculatesCorrectly()
    {
        var strategy = new GaussianNoiseGenerationStrategy();

        Assert.Equal(_testAmplitude * 0.1, strategy.GetNoiseAmplitude(_testAmplitude, 10));
        Assert.Equal(_testAmplitude, strategy.GetNoiseAmplitude(_testAmplitude, 100));
        Assert.Equal(_testAmplitude * 1.5, strategy.GetNoiseAmplitude(_testAmplitude, 150));
    }

    [Fact]
    public void GaussianNoise_Generate_DoesNotReturnNaNOrInfinity()
    {
        var strategy = new GaussianNoiseGenerationStrategy();
        var mockRandom = new Mock<Random>();

        // Устанавливаем фиксированные значения для двух вызовов NextDouble() внутри Generate
        // Это гарантирует предсказуемый результат.
        // Первый вызов NextDouble() вернёт 0.5
        // Второй вызов NextDouble() вернёт 0.25
        mockRandom.SetupSequence(r => r.NextDouble())
                  .Returns(0.5) // Это даст u1 = 1.0 - 0.5 = 0.5
                  .Returns(0.25); // Это даст u2 = 1.0 - 0.25 = 0.75

        double noiseAmplitude = 2.0;

        // Вычисляем ожидаемый результат вручную, как это делает метод Generate:
        // u1 = 1.0 - 0.5 = 0.5
        // u2 = 1.0 - 0.25 = 0.75
        // randStdNormal = sqrt(-2 * ln(u1)) * sin(2 * Math.PI * u2)
        // randStdNormal = sqrt(-2 * ln(0.5)) * sin(2 * Math.PI * 0.75)
        // sin(2 * Math.PI * 0.75) = sin(1.5 * Math.PI) = -1
        // randStdNormal = sqrt(-2 * ln(0.5)) * (-1) = -sqrt(-2 * ln(0.5))
        double sqrt_part = Math.Sqrt(-2.0 * Math.Log(0.5));
        double sin_part = Math.Sin(2.0 * Math.PI * 0.75); // sin(1.5π) = -1
        double randStdNormal = sqrt_part * sin_part; // sqrt_part * (-1)
        double expected_result = randStdNormal * noiseAmplitude; // (sqrt_part * -1) * 2.0

        double result = strategy.Generate(noiseAmplitude, mockRandom.Object);

        // Проверяем, что результат не является специальным значением
        Assert.False(double.IsNaN(result));
        Assert.False(double.IsInfinity(result));

        // Проверяем, что результат соответствует ожидаемому значению, вычисленному по формуле
        // Учитывая расчет выше: результат должен быть отрицательным.
        Assert.Equal(expected_result, result, precision: 10);
    }
}
