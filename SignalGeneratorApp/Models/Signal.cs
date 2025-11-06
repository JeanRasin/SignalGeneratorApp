using SignalGeneratorApp.Extensions;
using SignalGeneratorApp.Models.Enums;
using System.Collections.ObjectModel;

namespace SignalGeneratorApp.Models;

/// <summary>
/// Модель сигнала: содержит метаданные и точки.
/// </summary>
public class Signal
{
    /// <summary>
    /// Уникальный идентификатор сигнала.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Тип сигнала (например, синусоидальный, прямоугольный).
    /// </summary>
    public SignalTypeEnum Type { get; set; }

    /// <summary>
    /// Амплитуда сигнала.
    /// </summary>
    public double Amplitude { get; set; }

    /// <summary>
    /// Частота сигнала.
    /// </summary>
    public double Frequency { get; set; }

    /// <summary>
    /// Начальная фаза сигнала.
    /// </summary>
    public double Phase { get; set; }

    /// <summary>
    /// Общая длительность сигнала в секундах.
    /// </summary>
    public double TimeInterval { get; set; }

    /// <summary>
    /// Уровень шума в процентах.
    /// </summary>
    public int NoiseLevel { get; set; } = SignalDefaults.NoiseLevel;

    /// <summary>
    /// Дата и время создания сигнала.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Количество точек в сигнале. Автоматически синхронизируется с коллекцией <see cref="Points"/>.
    /// </summary>
    public int PointCount { get; set; }

    // Коллекция точек сигнала
    private ObservableCollection<SignalPoint> _points = [];

    /// <summary>
    /// Коллекция точек сигнала, где каждая точка содержит время и значение.
    /// При установке значения свойства <see cref="PointCount"/> автоматически обновляется.
    /// </summary>
    public ObservableCollection<SignalPoint> Points
    {
        get => _points;
        set
        {
            _points = value ?? [];
            PointCount = _points.Count; 
        }
    }

    /// <summary>
    /// Возвращает отображаемое имя сигнала с параметрами.
    /// </summary>
    public string DisplayName
    {
        get
        {
            return $"{Type.GetDescription()} (А={Amplitude}, Ч={Frequency}, Ф={Phase}, T={TimeInterval})";
        }
    }
}