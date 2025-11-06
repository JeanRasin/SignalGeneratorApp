using Moq;
using SignalGeneratorApp.Models.Enums;
using SignalGeneratorApp.Services.Generators;
using SignalGeneratorApp.Services.NoiseGenerations;

namespace SignalGeneratorApp.Tests;

/// <summary>
/// Тесты для основного генератора
/// </summary>
public class SignalGeneratorTests
{
    private readonly Mock<INoiseGenerationStrategy> _mockNoiseStrategy;
    private readonly ISignalGenerator _generator;

    public SignalGeneratorTests()
    {
        _mockNoiseStrategy = new Mock<INoiseGenerationStrategy>();
        _mockNoiseStrategy.Setup(ns => ns.GetNoiseAmplitude(It.IsAny<double>(), It.IsAny<int>()))
                          .Returns((double amp, int level) => amp * (level / 100.0)); // Простая реализация для теста
        _mockNoiseStrategy.Setup(ns => ns.Generate(It.IsAny<double>(), It.IsAny<Random>()))
                          .Returns((double amp, Random r) => 0.0); // Для простоты теста шум = 0

        _generator = new SignalGeneratorWithStrategy(_mockNoiseStrategy.Object);
    }

    [Fact]
    public async Task GenerateSignalAsync_ThrowsArgumentException_IfPointCountIsZeroOrNegative()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _generator.GenerateSignalAsync(SignalTypeEnum.Sine, pointCount: 0));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _generator.GenerateSignalAsync(SignalTypeEnum.Sine, pointCount: -5));
    }

    [Fact]
    public async Task GenerateSignalAsync_ReturnsSignalWithCorrectMetadata()
    {
        var type = SignalTypeEnum.Square;
        var amplitude = 2.5;
        var frequency = 2.0;
        var phase = Math.PI / 4;
        var pointCount = 50;
        var timeInterval = 2.0;
        var noiseLevel = 5;

        var signal = await _generator.GenerateSignalAsync(
            type, amplitude, frequency, phase, pointCount, timeInterval, noiseLevel);

        Assert.Equal(type, signal.Type);
        Assert.Equal(amplitude, signal.Amplitude);
        Assert.Equal(frequency, signal.Frequency);
        Assert.Equal(phase, signal.Phase);
        Assert.Equal(timeInterval, signal.TimeInterval);
        Assert.Equal(noiseLevel, signal.NoiseLevel);
    }

    [Fact]
    public async Task GenerateSignalAsync_ReturnsSignalWithCorrectPointCount()
    {
        var pointCount = 100;

        var signal = await _generator.GenerateSignalAsync(SignalTypeEnum.Sine, pointCount: pointCount);

        Assert.Equal(pointCount, signal.Points.Count);
    }

    [Fact]
    public async Task GenerateSignalAsync_ReturnsSignalWithCorrectTimeStamps()
    {
        var pointCount = 5;
        var timeInterval = 1.0;
        var expectedDt = timeInterval / (pointCount - 1); // (1.0 - 0) / (5-1) = 0.25

        var signal = await _generator.GenerateSignalAsync(
            SignalTypeEnum.Sine, pointCount: pointCount, timeInterval: timeInterval);

        Assert.Equal(0.0, signal.Points[0].Time);
        Assert.Equal(expectedDt, signal.Points[1].Time);
        Assert.Equal(2 * expectedDt, signal.Points[2].Time);
        Assert.Equal(3 * expectedDt, signal.Points[3].Time);
        Assert.Equal(timeInterval, signal.Points[4].Time); // Последняя точка = timeInterval
    }

    [Fact]
    public async Task GenerateSignalAsync_UsesNoiseStrategyCorrectly()
    {
        var amplitude = 10.0;
        var noiseLevel = 20; // Ожидаемый noiseAmplitude = 2.0
        var expectedNoiseAmplitude = amplitude * (noiseLevel / 100.0);

        var signal = await _generator.GenerateSignalAsync(
            SignalTypeEnum.Sine, amplitude: amplitude, noiseLevel: noiseLevel, pointCount: 10);

        // Проверяем, что стратегия шума была вызвана с правильными параметрами
        _mockNoiseStrategy.Verify(
            ns => ns.GetNoiseAmplitude(amplitude, noiseLevel),
            Times.Once // Ожидаем, что GetNoiseAmplitude вызывается один раз в начале GenerateSignalCore
        );
        // Для каждого шага генерации вызывается Generate. pointCount = 10, значит 10 вызовов.
        _mockNoiseStrategy.Verify(
            ns => ns.Generate(expectedNoiseAmplitude, It.IsAny<Random>()),
            Times.Exactly(10)
        );
    }

    [Fact]
    public async Task GenerateSignalAsync_CancelsCorrectly()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = _generator.GenerateSignalAsync(SignalTypeEnum.Sine, pointCount: 10000, token: cts.Token);

        // Ожидаем TaskCanceledException, так как это то, что выбрасывает Task.Run при отмене.
        await Assert.ThrowsAsync<TaskCanceledException>(() => task);
    }
}
