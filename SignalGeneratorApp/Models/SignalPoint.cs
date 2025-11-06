namespace SignalGeneratorApp.Models;

/// <summary>
/// Представляет точку сигнала, содержащую момент времени и соответствующее значение.
/// </summary>
/// <param name="Time">Момент времени.</param>
/// <param name="Value">Значение сигнала.</param>
public record SignalPoint(double Time, double Value);