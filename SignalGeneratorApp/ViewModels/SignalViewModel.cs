using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SignalGeneratorApp.Models;
using SignalGeneratorApp.Repositories;
using SignalGeneratorApp.Visualizers;
using System.Collections.ObjectModel;
using System.Windows;

namespace SignalGeneratorApp.ViewModels;

/// <summary>
/// ViewModel для отображения сигнала на графике.
/// </summary>
public partial class SignalViewModel(ISignalVisualizer visualizer, ISignalRepository signalRepository) : ObservableObject
{
    private readonly ISignalVisualizer _visualizer = visualizer;

    public const int MaxDisplayPoints = 10_000; // Максимум точек для отображения на графике

    /// <summary>
    /// Серии данных для отображения на графике.
    /// </summary>
    public ObservableCollection<ISeries> ChartSeries { get; set; } = [];

    /// <summary>
    /// Текущий отображаемый сигнал.
    /// </summary>
    public Signal? CurrentSignal { get; set; }

    /// <summary>
    /// Оси X для графика (привязываются к CartesianChart.XAxes).
    /// </summary>
    public Axis[] XAxis { get; set; } =
    [
        new()
        {
            Name = "Время",
            MinLimit = 0,
            MaxLimit = 1,
            ShowSeparatorLines = true
        }
    ];

    /// <summary>
    /// Оси Y для графика (привязываются к CartesianChart.YAxes).
    /// </summary>
    public Axis[] YAxis { get; set; } =
    [
        new()
        {
            Name = "Амплитуда",
            MinLimit = -10,
            MaxLimit = 10,
            ShowSeparatorLines = true
        }
    ];

    /// <summary>
    /// Загружает сигнал и обновляет график с ограничением количества точек.
    /// </summary>
    /// <param name="signal">Сигнал для загрузки и отображения.</param>
    /// <param name="token">Токен отмены операции.</param>
    public async Task LoadSignalAsync(Signal signal, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(signal);

        // Загрузка точек из репозитория, если сигнал сохранён в БД
        if (signal.Id > 0)
        {
            var loadPoints = await signalRepository.LoadSignalPointsAsync(signal.Id, token);
            signal.Points = new ObservableCollection<SignalPoint>(loadPoints);
        }

        CurrentSignal = signal;

        // Пропуск, если нет точек
        if (signal.Points == null || signal.Points.Count == 0)
        {
            await InvokeOnUIThreadAsync(ChartSeries.Clear);
            return;
        }

        // Применяем даунсэмплинг при необходимости
        var displaySignal = signal.Points.Count > MaxDisplayPoints
            ? DownsampleSignal(signal, MaxDisplayPoints)
            : signal;

        // Повторная проверка после даунсэмплинга
        if (displaySignal.Points is null || displaySignal.Points.Count == 0)
        {
            await InvokeOnUIThreadAsync(ChartSeries.Clear);
            return;
        }

        var newSeries = _visualizer.Visualize(displaySignal);
        if (newSeries.Length == 0)
        {
            await InvokeOnUIThreadAsync(ChartSeries.Clear);
            return;
        }

        // Обновляем UI в основном потоке
        await InvokeOnUIThreadAsync(() =>
        {
            ChartSeries.Clear();
            foreach (var series in newSeries)
            {
                ChartSeries.Add(series);
            }

            var points = displaySignal.Points;
            XAxis[0].MinLimit = points[0].Time;
            XAxis[0].MaxLimit = points[^1].Time;

            var values = points.Select(static p => p.Value).ToArray();
            var minY = values.Min();
            var maxY = values.Max();
            var rangeY = maxY - minY;
            var padding = rangeY > 0 ? rangeY * 0.1 : 1.0;

            YAxis[0].MinLimit = minY - padding;
            YAxis[0].MaxLimit = maxY + padding;
        });
    }

    /// <summary>
    /// Выполняет указанное действие в UI-потоке WPF.
    /// </summary>
    private async Task InvokeOnUIThreadAsync(Action action)
    {
        if (Application.Current?.Dispatcher != null)
        {
            await Application.Current.Dispatcher.InvokeAsync(action);
        }
        else
        {
            // Если Dispatcher недоступен (например, в тестах), выполняем напрямую.
            // ⚠️ Это предполагает, что мы уже в UI-потоке.
            action();
        }
    }

    /// <summary>
    /// Создаёт упрощённую копию сигнала с ограниченным числом точек.
    /// Гарантирует включение первой и последней точки.
    /// </summary>
    /// <param name="original">Исходный сигнал.</param>
    /// <param name="maxPoints">Максимальное количество точек в результате.</param>
    /// <returns>Упрощённый сигнал.</returns>
    private static Signal DownsampleSignal(Signal original, int maxPoints)
    {
        var originalPoints = original.Points;
        var count = originalPoints.Count;

        if (count <= maxPoints || maxPoints < 2)
            return original;

        var step = (int)Math.Ceiling((double)count / (maxPoints - 1)); // -1, чтобы оставить место для последней точки
        var downsampled = new List<SignalPoint>(maxPoints);

        // Добавляем точки с равномерным шагом
        for (int i = 0; i < count - 1 && downsampled.Count < maxPoints - 1; i += step)
        {
            downsampled.Add(originalPoints[i]);
        }

        // Обязательно добавляем последнюю точку
        if (downsampled.Count == 0 || downsampled[^1] != originalPoints[^1])
        {
            downsampled.Add(originalPoints[^1]);
        }

        return new Signal
        {
            Id = original.Id,
            Points = new ObservableCollection<SignalPoint>(downsampled)
        };
    }

    /// <summary>
    /// Сбрасывает масштаб осей к полному диапазону сигнала.
    /// </summary>
    [RelayCommand]
    private void ResetZoom()
    {
        if (CurrentSignal is { } signal)
        {
            // Используем новый токен, так как это пользовательское действие
            _ = LoadSignalAsync(signal);
        }
    }
}