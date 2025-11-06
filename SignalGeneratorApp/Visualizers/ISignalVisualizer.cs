using LiveChartsCore;
using SignalGeneratorApp.Models;

namespace SignalGeneratorApp.Visualizers;

/// <summary>
/// Представляет стратегию визуализации сигнала в виде графической серии (или набора серий.
/// </summary>
public interface ISignalVisualizer
{
    /// <summary>
    /// Преобразует заданный сигнал в массив серий данных для отображения на графике.
    /// </summary>
    /// <param name="signal">
    /// Сигнал, подлежащий визуализации. Должен содержать набор точек с временем и значением.
    /// Может быть <see langword="null"/>, в этом случае возвращается пустой массив.
    /// </param>
    /// <returns>
    /// Массив объектов <see cref="ISeries"/>, готовых к отображению в компоненте LiveCharts.
    /// Возвращает пустой массив, если входной сигнал некорректен или не содержит точек.
    /// </returns>
    ISeries[] Visualize(Signal signal);
}