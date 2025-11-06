using SignalGeneratorApp.Models;
using SignalGeneratorApp.Models.Enums;
using SignalGeneratorApp.Services.Generators.Strategies;
using SignalGeneratorApp.Services.NoiseGenerations;

namespace SignalGeneratorApp.Services.Generators;

/// <summary>
/// Реализация генератора сигнала с использованием стратегий и возможности добавления шума.
/// Поддерживает параллельную генерацию при большом количестве точек для повышения производительности.
/// </summary>
public class SignalGeneratorWithStrategy(INoiseGenerationStrategy noiseGenerationStrategy) : ISignalGenerator
{
    /// <summary>
    /// Потокобезопасный экземпляр генератора случайных чисел.
    /// Используется <see cref="ThreadLocal{T}"/> для избежания contention при высокой параллельности.
    /// </summary>
    private static readonly ThreadLocal<Random> ThreadRandom = new(() => new Random());

    /// <summary>
    /// Минимальное количество точек, при котором включается параллельная генерация.
    /// </summary>
    private const int ParallelizationThreshold = 10_000;

    /// <summary>
    /// Выполняет основную логику генерации сигнала: вычисляет временные точки, применяет стратегию генерации и добавляет шум.
    /// </summary>
    /// <param name="strategy">Стратегия генерации сигнала (синус, косинус, пила и т.д.).</param>
    /// <param name="type">Тип сигнала (например, синусоидальный, прямоугольный).</param>
    /// <param name="amplitude">Амплитуда сигнала.</param>
    /// <param name="frequency">Частота сигнала в герцах.</param>
    /// <param name="phase">Начальная фаза сигнала в радианах.</param>
    /// <param name="pointCount">Количество точек в генерируемом сигнале.</param>
    /// <param name="timeInterval">Общая длительность сигнала в секундах.</param>
    /// <param name="noiseLevel">Уровень шума (0–100%).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Сгенерированный сигнал в виде объекта <see cref="Signal"/>.</returns>
    private Signal GenerateSignalCore(
        ISignalGenerationStrategy strategy,
        SignalTypeEnum type,
        double amplitude,
        double frequency,
        double phase,
        int pointCount,
        double timeInterval,
        int noiseLevel,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Шаг по времени между соседними точками
        double dt = timeInterval / Math.Max(1, pointCount - 1);

        // Вычисление амплитуды шума на основе исходной амплитуды и уровня шума
        var noiseAmplitude = noiseGenerationStrategy.GetNoiseAmplitude(amplitude, noiseLevel);

        var pointsArray = new SignalPoint[pointCount];

        if (pointCount >= ParallelizationThreshold)
        {
            var parallelOptions = new ParallelOptions
            {
                // Использовать все ядра, кроме одного — чтобы избежать "подвисания" пользовательского интерфейса
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
                CancellationToken = cancellationToken
            };

            Parallel.For(0, pointCount, parallelOptions, i =>
            {
                pointsArray[i] = CreateSignalPoint(strategy, amplitude, frequency, phase, i, dt, noiseAmplitude);
            });
        }
        else
        {
            var localRandom = ThreadRandom.Value;
            for (int i = 0; i < pointCount; i++)
            {
                pointsArray[i] = CreateSignalPoint(strategy, amplitude, frequency, phase, i, dt, noiseAmplitude);
            }
        }

        return new Signal
        {
            Type = type,
            Amplitude = amplitude,
            Frequency = frequency,
            Phase = phase,
            TimeInterval = timeInterval,
            NoiseLevel = noiseLevel,
            Points = new(pointsArray)
        };
    }

    /// <summary>
    /// Создаёт одну точку сигнала, включая добавление шума.
    /// </summary>
    /// <param name="strategy">Стратегия генерации сигнала.</param>
    /// <param name="amplitude">Амплитуда сигнала.</param>
    /// <param name="frequency">Частота сигнала.</param>
    /// <param name="phase">Начальная фаза.</param>
    /// <param name="i">Индекс текущей точки.</param>
    /// <param name="dt">Шаг по времени между точками.</param>
    /// <param name="noiseAmplitude">Амплитуда шума, добавляемого к сигналу.</param>
    /// <returns>Сгенерированная точка сигнала.</returns>
    private SignalPoint CreateSignalPoint(
        ISignalGenerationStrategy strategy,
        double amplitude,
        double frequency,
        double phase,
        int i,
        double dt,
        double noiseAmplitude)
    {
        double t = i * dt; // Текущее время
        double value = strategy.CalculateValue(amplitude, frequency, t, phase); // "Чистое" значение сигнала
        double noise = noiseGenerationStrategy.Generate(noiseAmplitude, ThreadRandom.Value); // Генерация шума
        return new(Time: t, Value: value + noise); // Сложение сигнала и шума
    }

    /// <summary>
    /// Асинхронно генерирует сигнал указанного типа с заданными параметрами и уровнем шума.
    /// </summary>
    /// <param name="type">Тип сигнала (например, <see cref="SignalTypeEnum.Sine"/>).</param>
    /// <param name="amplitude">Амплитуда сигнала.</param>
    /// <param name="frequency">Частота сигнала в герцах.</param>
    /// <param name="phase">Начальная фаза в радианах (по умолчанию 0).</param>
    /// <param name="pointCount">Количество точек в сигнале (по умолчанию 100).</param>
    /// <param name="timeInterval">Общая длительность сигнала в секундах (по умолчанию 1).</param>
    /// <param name="noiseLevel">Уровень шума в процентах от амплитуды (0–100, по умолчанию 0).</param>
    /// <param name="token">Токен отмены операции.</param>
    /// <returns>Задача, возвращающая сгенерированный <see cref="Signal"/>.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="pointCount"/> меньше или равен нулю.</exception>
    public async Task<Signal> GenerateSignalAsync(
        SignalTypeEnum type,
        double amplitude,
        double frequency,
        double phase = SignalDefaults.Phase,
        int pointCount = SignalDefaults.PointCount,
        double timeInterval = SignalDefaults.TimeInterval,
        int noiseLevel = SignalDefaults.NoiseLevel,
        CancellationToken token = default)
    {
        if (pointCount <= 0)
            throw new ArgumentException("Число точек должно быть больше нуля.", nameof(pointCount));

        // Выполняем тяжёлую вычислительную работу в фоновом потоке, чтобы не блокировать UI
        var result = await Task.Run(() => GenerateSignalCore(
            SignalGenerationStrategyFactory.CreateStrategy(type),
            type,
            amplitude,
            frequency,
            phase,
            pointCount,
            timeInterval,
            noiseLevel,
            token), token);

        return result;
    }
}