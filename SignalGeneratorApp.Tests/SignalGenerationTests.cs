namespace SignalGeneratorApp.Tests;

/// <summary>
/// Тесты для стратегий генерации сигналов
/// </summary>
public class SignalStrategyTests
{
    private readonly double _testAmplitude = 5.0;
    private readonly double _testFrequency = 1.0; // 1 Гц для простоты

    [Fact]
    public void SineStrategy_CalculateValue_ReturnsCorrectValue()
    {
        var strategy = new SineGenerationStrategy();
        // sin(0) = 0
        double valueAtZero = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.0, 0.0);
        Assert.Equal(0.0, valueAtZero, precision: 10); // Используем precision для учета погрешности double

        // sin(π/2) = 1 -> при t=0.25, фазе 0: sin(2π*1*0.25 + 0) = sin(π/2) = 1
        double valueQuarterPeriod = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.25, 0.0);
        Assert.Equal(_testAmplitude, valueQuarterPeriod, precision: 10);

        // sin(π) = 0 -> при фазе π: sin(2π*1*0 + π) = sin(π) = 0
        double valueWithPhasePi = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.0, Math.PI);
        Assert.Equal(0.0, valueWithPhasePi, precision: 10);

        // sin(π/2 + π) = sin(3π/2) = -1 -> при t=0.25, фазе π: sin(2π*1*0.25 + π) = sin(π/2 + π) = -1
        double valueWithPhasePiQuarterT = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.25, Math.PI);
        Assert.Equal(-_testAmplitude, valueWithPhasePiQuarterT, precision: 10);
    }

    [Fact]
    public void SquareStrategy_CalculateValue_ReturnsCorrectValue()
    {
        var strategy = new SquareGenerationStrategy();
        // sin(0) = 0 -> Math.Sign(0) = 0. Проверим точки слева и справа от 0.
        // sin(-0.1*2*π) = sin(-0.2π) < 0 -> Math.Sign = -1
        double valueNeg = strategy.CalculateValue(_testAmplitude, _testFrequency, -0.1, 0.0);
        Assert.Equal(-_testAmplitude, valueNeg);

        // sin(0.1*2*π) = sin(0.2π) > 0 -> Math.Sign = 1
        double valuePos = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.1, 0.0);
        Assert.Equal(_testAmplitude, valuePos);

        // sin(0.5*2*π) = sin(π) = 0 -> Math.Sign = 0. Поведение зависит от реализации Math.Sign.
        // В C# Math.Sign(0.0) = 0, но Math.Sin может вернуть значение близкое к 0, но не 0.
        // Для устойчивости проверим точки, где sin определенно > 0 или < 0.
        // sin(0.25*2*π) = sin(π/2) = 1 -> Math.Sign = 1
        double valueAtQuarter = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.25, 0.0);
        Assert.Equal(_testAmplitude, valueAtQuarter);

        // sin(0.75*2*π) = sin(3π/2) = -1 -> Math.Sign = -1
        double valueAtThreeQuarter = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.75, 0.0);
        Assert.Equal(-_testAmplitude, valueAtThreeQuarter);
    }

    [Fact]
    public void TriangleStrategy_CalculateValue_ReturnsCorrectValue()
    {
        var strategy = new TriangleGenerationStrategy();
        // t=0: начало роста, y = -amplitude
        double valueAtZero = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.0, 0.0);
        Assert.Equal(-_testAmplitude, valueAtZero, precision: 10);

        // t=0.25 (четверть периода 1Гц): середина роста, y = 0
        double valueQuarterRise = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.25, 0.0);
        Assert.Equal(0.0, valueQuarterRise, precision: 10);

        // t=0.5 (половина периода): пик, y = +amplitude
        double valueAtPeak = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.5, 0.0);
        Assert.Equal(_testAmplitude, valueAtPeak, precision: 10);

        // t=0.75: середина падения, y = 0
        double valueQuarterFall = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.75, 0.0);
        Assert.Equal(0.0, valueQuarterFall, precision: 10);

        // t=1.0: конец периода, возвращается к -amplitude
        double valueAtEnd = strategy.CalculateValue(_testAmplitude, _testFrequency, 1.0, 0.0);
        Assert.Equal(-_testAmplitude, valueAtEnd, precision: 10);
    }

    [Fact]
    public void SawtoothStrategy_CalculateValue_ReturnsCorrectValue()
    {
        var strategy = new SawtoothGenerationStrategy();
        // t=0: начало роста, y = -amplitude
        double valueAtZero = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.0, 0.0);
        Assert.Equal(-_testAmplitude, valueAtZero, precision: 10);

        // t=0.5 (половина периода 1Гц): середина, y = 0
        double valueAtHalf = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.5, 0.0);
        Assert.Equal(0.0, valueAtHalf, precision: 10);

        // t=0.999: почти на пике, y ~ +amplitude
        double valueNearPeak = strategy.CalculateValue(_testAmplitude, _testFrequency, 0.999, 0.0);
        Assert.InRange(valueNearPeak, _testAmplitude * 0.99, _testAmplitude * 1.01); // Допуск на погрешность

        // t=1.0: сброс, y = -amplitude
        double valueAtReset = strategy.CalculateValue(_testAmplitude, _testFrequency, 1.0, 0.0);
        Assert.Equal(-_testAmplitude, valueAtReset, precision: 10);
    }
}
