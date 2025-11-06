using SignalGeneratorApp.Models;
using SignalGeneratorApp.Models.Enums;

namespace SignalGeneratorApp.Services.Generators;

/// <summary>
/// Интерфейс для генерации сигналов с шумом.
/// </summary>
public interface ISignalGenerator
{
    /// <summary>
    /// Асинхронно генерирует сигнал заданного типа с указанными параметрами и уровнем шума.
    /// </summary>
    /// <param name="type">Тип генерируемого сигнала (например, синусоида, меандр и т.д.).</param>
    /// <param name="amplitude">Амплитуда сигнала в условных единицах. По умолчанию — <see cref="SignalDefaults.Amplitude"/>.</param>
    /// <param name="frequency">Частота сигнала в герцах (Гц). По умолчанию — <see cref="SignalDefaults.Frequency"/>.</param>
    /// <param name="phase">Начальная фаза сигнала в радианах. Используется в основном для синусоиды. По умолчанию — <see cref="SignalDefaults.Phase"/>.</param>
    /// <param name="pointCount">Количество точек дискретизации. По умолчанию — <see cref="SignalDefaults.PointCount"/>.</param>
    /// <param name="timeInterval">Общая длительность сигнала в секундах. По умолчанию — <see cref="SignalDefaults.TimeInterval"/>.</param>
    /// <param name="noiseLevel">Уровень шума в процентах от амплитуды (0–100). По умолчанию — <see cref="SignalDefaults.NoiseLevel"/>.</param>
    /// <param name="token">Токен отмены операции. По умолчанию — <see cref="CancellationToken.None"/>.</param>
    /// <returns>Сгенерированный сигнал в виде объекта <see cref="Signal"/>.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если переданы недопустимые параметры (например, отрицательная частота).</exception>
    /// <exception cref="OperationCanceledException">Выбрасывается, если операция была отменена через <paramref name="token"/>.</exception>
    Task<Signal> GenerateSignalAsync(
        SignalTypeEnum type,
        double amplitude = SignalDefaults.Amplitude,
        double frequency = SignalDefaults.Frequency,
        double phase = SignalDefaults.Phase,
        int pointCount = SignalDefaults.PointCount,
        double timeInterval = SignalDefaults.TimeInterval,
        int noiseLevel = SignalDefaults.NoiseLevel,
        CancellationToken token = default);
}