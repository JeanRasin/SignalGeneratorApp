using SignalGeneratorApp.Models;
using System.Buffers;
using System.Collections.ObjectModel;

namespace SignalGeneratorApp.Services.Processing;

/// <summary>
/// Реализация сервиса обработки сигналов.
/// </summary>
public class SignalProcessingService : ISignalProcessingService
{
    const int CHECK_INTERVAL = 1000; // проверяем каждые 1000 точек

    /// <summary>
    /// Применяет медианный фильтр к указанному сигналу.
    /// </summary>
    /// <para><b>Теория метода:</b></para>
    /// <para>
    /// Медианный фильтр — это нелинейный цифровой фильтр, широко используемый для сглаживания сигналов
    /// и подавления импульсного шума (например, "соль и перец"), при этом сохраняя резкие переходы (края).
    /// </para>
    /// <para>
    /// Работа фильтра основана на замене значения каждой точки сигнала медианой (средним значением)
    /// подмножества соседних точек, определяемого скользящим окном заданного размера.
    /// </para>
    /// <list type="bullet">
    /// <item>Эффективен против импульсных выбросов и шума, состоящего из отдельных точек с аномальными значениями.</item>
    /// <item>Хорошо сохраняет резкие перепады (ступеньки) в сигнале, в отличие от линейных фильтров.</item>
    /// <item>Неэффективен против гауссовского (нормального) шума — для него лучше подойдут линейные фильтры.</item>
    /// <item>Нелинейность фильтра может привести к искажению формы сигнала при больших размерах окна.</item>
    /// </list>
    /// <para><b>Области применения:</b></para>
    /// <para>
    /// Медианный фильтр особенно полезен в обработке изображений (удаление шума "соль и перец"),
    /// анализе данных с датчиков (где возможны кратковременные выбросы), обработке биосигналов и т.д.
    /// </para>
    /// <para><b>Параметры:</b></para>
    /// <para>
    /// - <paramref name="signal"/>: Исходный сигнал, подлежащий фильтрации. Не должен быть null.
    /// - <paramref name="windowSize"/>: Размер скользящего окна (должен быть нечётным и больше 0). По умолчанию 3.
    /// - <paramref name="cancellationToken"/>: Токен для отмены асинхронной операции. По умолчанию не используется.
    /// </para>
    /// <returns>Новый сигнал с отфильтрованными значениями. Оригинальный сигнал не изменяется.</returns>
    /// <exception cref="System.ArgumentNullException">Если <paramref name="signal"/> равен null.</exception>
    /// <exception cref="System.ArgumentException">Если <paramref name="windowSize"/> меньше 1 или чётное.</exception>
    /// <exception cref="System.OperationCanceledException">Если операция была отменена через <paramref name="cancellationToken"/>.</exception>
    public Signal ApplyMedianFilter(Signal signal, int windowSize = 3, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(signal);

        if (windowSize < 1 || windowSize % 2 == 0)
            throw new ArgumentException("Размер окна должен быть нечётным и больше 0.", nameof(windowSize));

        // Быстрый выход для пустого сигнала
        if (signal.Points.Count == 0)
            return CloneSignal(signal);

        // Оптимизация: если окно = 1, сигнал не меняется
        if (windowSize == 1)
            return CloneSignal(signal);

        // Извлекаем значения сигнала в массив для эффективного доступа по индексу
        var values = signal.Points.Select(p => p.Value).ToArray();
        var filteredValues = new double[values.Length];

        // Выбираем стратегию в зависимости от размера окна:
        // Небольшие окна обрабатываются без аллокаций (stackalloc или константная логика),
        // что повышает производительность.
        if (windowSize == 3)
        {
            ApplyMedianFilter3(values, filteredValues, cancellationToken);
        }
        else if (windowSize == 5)
        {
            ApplyMedianFilter5(values, filteredValues, cancellationToken);
        }
        else if (windowSize == 7)
        {
            ApplyMedianFilter7(values, filteredValues, cancellationToken);
        }
        else
        {
            // Общий случай для любого нечётного windowSize (>7)
            ApplyMedianFilterGeneral(values, filteredValues, windowSize, cancellationToken);
        }

        // Создаём новый сигнал с отфильтрованными значениями
        var result = CloneSignal(signal);
        for (int i = 0; i < signal.Points.Count; i++)
        {
            result.Points[i] = new SignalPoint(Time: signal.Points[i].Time, Value: filteredValues[i]);
        }

        result.CreatedAt = DateTime.Now;
        return result;
    }

    /// <summary>
    /// Клонирует сигнал (глубокая копия метаданных и точек).
    /// </summary>
    private static Signal CloneSignal(Signal original)
    {
        var points = new List<SignalPoint>(original.Points.Count);

        var clone = new Signal
        {
            Id = original.Id,
            Type = original.Type,
            Amplitude = original.Amplitude,
            Frequency = original.Frequency,
            Phase = original.Phase,
            TimeInterval = original.TimeInterval,
            NoiseLevel = original.NoiseLevel,
            CreatedAt = original.CreatedAt,
            Points = new ObservableCollection<SignalPoint>(points)
        };

        // Копируем точки (они неизменяемы по сути, но создаём новые экземпляры)
        foreach (var point in original.Points)
        {
            clone.Points.Add(new SignalPoint(Time: point.Time, Value: point.Value));
        }

        return clone;
    }

    /// <summary>
    /// Применяет медианный фильтр с окном размера 3.
    /// Использует оптимизированную логику без сортировки — только сравнения.
    /// </summary>
    private static void ApplyMedianFilter3(ReadOnlySpan<double> input, Span<double> output, CancellationToken cancellationToken = default)
    {
        int n = input.Length;
        for (int i = 0; i < n; i++)
        {
            if (i % CHECK_INTERVAL == 0)
                cancellationToken.ThrowIfCancellationRequested();

            // Используем Math.Max/Min для обработки границ: первая и последняя точки
            // дублируются (т.е. применяется "зеркальное" поведение без выхода за пределы)
            double a = input[Math.Max(0, i - 1)];
            double b = input[i];
            double c = input[Math.Min(n - 1, i + 1)];
            output[i] = Median3(a, b, c);
        }
    }

    /// <summary>
    /// Возвращает медиану из трёх значений с минимальным количеством сравнений (3 сравнения).
    /// </summary>
    private static double Median3(double a, double b, double c)
    {
        // Алгоритм: последовательное упорядочивание пар
        if (a > b) (a, b) = (b, a); // a <= b
        if (b > c) (b, c) = (c, b);// b <= c → теперь a <= b <= c или a <= c <= b
        if (a > b) (a, b) = (b, a); // окончательная сортировка
        return b;// b — медиана
    }

    /// <summary>
    /// Применяет медианный фильтр с окном размера 5.
    /// Использует stackalloc для временного окна (аллокация в стеке, не в куче).
    /// </summary>
    private static void ApplyMedianFilter5(ReadOnlySpan<double> input, Span<double> output, CancellationToken cancellationToken = default)
    {
        int n = input.Length;

        Span<double> window = stackalloc double[5];// окно размещается в стеке

        for (int i = 0; i < n; i++)
        {
            int count = 0;
            // Заполняем окно с учётом границ: [-2, -1, 0, +1, +2]
            for (int j = -2; j <= 2; j++)
            {
                if (i % CHECK_INTERVAL == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                int idx = Math.Clamp(i + j, 0, n - 1);
                window[count++] = input[idx];
            }
            output[i] = Median5(window);
        }
    }

    /// <summary>
    /// Возвращает медиану из 5 элементов с помощью пузырьковой сортировки.
    /// Для 5 элементов это эффективно (максимум 10 сравнений).
    /// </summary>
    private static double Median5(Span<double> arr)
    {
        // Простая сортировка пузырьком — приемлема для 5 элементов
        for (int i = 0; i < 4; i++)
        {
            for (int j = i + 1; j < 5; j++)
            {
                if (arr[i] > arr[j])
                    (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
        return arr[2]; // индекс 2 — это медиана в нечётном массиве из 5 элементов
    }

    /// <summary>
    /// Применяет медианный фильтр с окном размера 7.
    /// Использует stackalloc + сортировку вставками (эффективна для малых массивов).
    /// </summary>
    private static void ApplyMedianFilter7(ReadOnlySpan<double> input, Span<double> output, CancellationToken cancellationToken = default)
    {
        int n = input.Length;

        Span<double> window = stackalloc double[7];

        for (int i = 0; i < n; i++)
        {
            if (i % CHECK_INTERVAL == 0)
                cancellationToken.ThrowIfCancellationRequested();

            int count = 0;
            // Окно: [-3, -2, -1, 0, +1, +2, +3]
            for (int j = -3; j <= 3; j++)
            {
                int idx = Math.Clamp(i + j, 0, n - 1);
                window[count++] = input[idx];
            }
            // Сортировка вставками — лучше пузырька для 7 элементов в среднем случае
            InsertionSort(window);
            output[i] = window[3]; // индекс 3 — медиана для 7 элементов
        }
    }

    /// <summary>
    /// Сортировка вставками для небольших массивов (до ~10 элементов).
    /// Более эффективна, чем пузырёк, и стабильна.
    /// </summary>
    private static void InsertionSort(Span<double> arr)
    {
        for (int i = 1; i < arr.Length; i++)
        {
            double key = arr[i];
            int j = i - 1;
            // Сдвигаем элементы, большие key, вправо
            while (j >= 0 && arr[j] > key)
            {
                arr[j + 1] = arr[j];
                j--;
            }
            arr[j + 1] = key;
        }
    }

    /// <summary>
    /// Применяет медианный фильтр для произвольного нечётного размера окна.
    /// Использует ArrayPool для повторного использования буфера и снижения GC-давления.
    /// </summary>
    private static void ApplyMedianFilterGeneral(ReadOnlySpan<double> input, Span<double> output, int windowSize, CancellationToken cancellationToken)
    {
        int n = input.Length;
        int radius = windowSize / 2;// например, при windowSize=9 → radius=4

        // Используем ArrayPool, чтобы избежать аллокаций при каждом окне
        var pool = ArrayPool<double>.Shared;
        var window = pool.Rent(windowSize);// запрашиваем буфер нужного размера

        try
        {
            for (int i = 0; i < n; i++)
            {
                // Периодическая проверка отмены
                if (i % CHECK_INTERVAL == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                // Заполняем окно, корректно обрабатывая границы сигнала
                int start = Math.Max(0, i - radius);
                int end = Math.Min(n - 1, i + radius);

                int count = 0;
                for (int j = start; j <= end; j++)
                {
                    window[count++] = input[j];
                }

                // Если окно меньше заявленного (на краях), используем реальный размер
                int actualSize = count;

                // Сортируем ТОЛЬКО заполненную часть окна
                Array.Sort(window, 0, actualSize);

                // Медиана — центральный элемент в отсортированном сегменте
                output[i] = window[actualSize / 2];
            }
        }
        finally
        {
            // ВАЖНО: всегда возвращать в пул, даже при исключении
            pool.Return(window);
        }
    }
}