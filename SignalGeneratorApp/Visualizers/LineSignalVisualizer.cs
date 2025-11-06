using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SignalGeneratorApp.Models;
using SkiaSharp;

namespace SignalGeneratorApp.Visualizers;

/// <summary>
/// Визуализирует сигнал в виде линейного графика с использованием библиотеки LiveCharts.
/// Поддерживает настройку цвета линии, толщины и отображения маркеров в точках данных.
/// </summary>
/// <remarks>
/// Инициализирует новый экземпляр класса <see cref="LineSignalVisualizer"/>.
/// </remarks>
/// <param name="strokeColor">
/// Цвет линии графика. По умолчанию — синий (<see cref="SKColors.Blue"/>).
/// </param>
/// <param name="strokeThickness">
/// Толщина линии в пикселях. Значение по умолчанию — 2.
/// </param>
/// <param name="showMarkers">
/// Указывает, следует ли отображать маркеры (точки) в каждой точке данных.
/// Если <see langword="true"/>, размер маркеров фиксирован (5 пикселей).
/// По умолчанию — <see langword="true"/>.
/// </param>
/// <param name="markerFillColor">
/// Цвет заливки маркеров (внутренняя часть точек). Если <see langword="null"/> — заливка не применяется,
/// и маркеры будут прозрачными или наследовать цвет от линии (в зависимости от реализации LiveCharts).
/// По умолчанию — <see langword="null"/>.
/// </param>
/// <param name="markerStrokeColor">
/// Цвет контура (обводки) маркеров. Если <see langword="null"/> — обводка не отображается.
/// По умолчанию — <see langword="null"/>.
/// </param>
public class LineSignalVisualizer(
    SKColor strokeColor = default,
    float strokeThickness = 2f,
    bool showMarkers = true,
    SKColor? markerFillColor = null,
    SKColor? markerStrokeColor = null) : ISignalVisualizer
{
    private readonly SKColor _strokeColor = strokeColor == default ? SKColors.Blue : strokeColor;
    private readonly float _strokeThickness = strokeThickness;
    private readonly double _geometrySize = showMarkers ? 5 : 0;
    private readonly SolidColorPaint? _geometryFill = markerFillColor is null ? null : new SolidColorPaint(markerFillColor.Value);
    private readonly SolidColorPaint? _geometryStroke = markerStrokeColor is null ? null : new SolidColorPaint(markerStrokeColor.Value);

    /// <inheritdoc/>
    public ISeries[] Visualize(Signal signal)
    {
        if (signal?.Points is not { Count: > 0 })
            return [];

        return [new LineSeries<(double Time, double Value)>
    {
        Values = signal.Points.Select(p => (p.Time, p.Value)),
        Mapping = (point, index) => new Coordinate(point.Time, point.Value),
        Fill = null,
        Stroke = new SolidColorPaint(_strokeColor) { StrokeThickness = _strokeThickness },
        GeometryFill = _geometryFill,
        GeometryStroke = _geometryStroke,
        GeometrySize = _geometrySize,
        YToolTipLabelFormatter= point => $"Время: {point.Model.Time:N2}",// Форматирование ToolTip точки
        XToolTipLabelFormatter= point => $"Значение: {point.Model.Value:N2}"
    }];
    }
}