using System.ComponentModel;

namespace SignalGeneratorApp.Models.Enums;

public enum SignalTypeEnum
{
    [Description("Синусоида")]
    Sine,
    [Description("Меандр")]
    Square,
    [Description("Треугольник")]
    Triangle,
    [Description("Пилообразный")]
    Sawtooth
}