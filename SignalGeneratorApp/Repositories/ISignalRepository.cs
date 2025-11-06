using SignalGeneratorApp.Models;

namespace SignalGeneratorApp.Repositories;

/// <summary>
/// Предоставляет методы для работы с сигналами в хранилище.
/// </summary>
public interface ISignalRepository
{
    /// <summary>
    /// Сохраняет сигнал и его точки в базу данных.
    /// </summary>
    /// <param name="signal">Сигнал для сохранения.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    Task SaveSignalAsync(Signal signal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Загружает все сигналы из базы данных.
    /// </summary>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Список загруженных сигналов.</returns>
    Task<List<Signal>> LoadSignalsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Загружает все точки из базы данных для сигнала.
    /// </summary>
    /// <param name="id">Идентификатор сигнала.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    /// <returns>Список загруженных точек сигнала.</returns>
    Task<List<SignalPoint>> LoadSignalPointsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет сигнал по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор сигнала.</param>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    Task DeleteSignalAsync(long id, CancellationToken cancellationToken = default);
}