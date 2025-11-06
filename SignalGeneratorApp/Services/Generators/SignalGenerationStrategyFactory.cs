using SignalGeneratorApp.Models.Enums;
using SignalGeneratorApp.Services.Generators.Strategies;

namespace SignalGeneratorApp.Services.Generators;

public static class SignalGenerationStrategyFactory
{
    public static ISignalGenerationStrategy CreateStrategy(SignalTypeEnum type)
    {
        return type switch
        {
            SignalTypeEnum.Sine => new SineGenerationStrategy(),
            SignalTypeEnum.Square => new SquareGenerationStrategy(),
            SignalTypeEnum.Triangle => new TriangleGenerationStrategy(),
            SignalTypeEnum.Sawtooth => new SawtoothGenerationStrategy(),
            _ => throw new ArgumentException($"Неизвестный тип сигнала: {type}", nameof(type))
        };
    }
}