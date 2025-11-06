using SignalGeneratorApp.Models;

namespace SignalGeneratorApp.Services.Processing;

/// <summary>
/// Сервис для обработки сигналов (например, сглаживание).
/// </summary>
public interface ISignalProcessingService
{
    /// <summary>
    /// Применяет медианный фильтр к указанному сигналу.
    /// </summary>
    /// <param name="signal">Сигнал, к которому применяется фильтр. Не должен быть null.</param>
    /// <param name="windowSize">Размер окна фильтра. Должен быть нечетным числом больше 0. По умолчанию 3.</param>
    /// <param name="cancellationToken">Токен для отмены операции. По умолчанию используется токен отсутствия отмены.</param>
    /// <returns>Новый сигнал с отфильтрованными значениями.</returns>
    Signal ApplyMedianFilter(Signal signal, int windowSize = 3, CancellationToken cancellationToken = default);
}