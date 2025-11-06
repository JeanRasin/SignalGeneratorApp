
using SignalGeneratorApp.Models;
using System.Collections.ObjectModel;

namespace SignalGeneratorApp.Services;

/// <summary>
/// Предоставляет доступ к библиотеке сохранённых сигналов и управляет их жизненным циклом.
/// </summary>
public interface ISignalLibraryService
{
    /// <summary>
    /// Коллекция сохранённых сигналов. 
    /// </summary>
    ObservableCollection<Signal> Signals { get; }

    /// <summary>
    /// Асинхронно загружает все сигналы из постоянного хранилища.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Асинхронно сохраняет сигнал в постоянное хранилище и добавляет его в коллекцию.
    /// </summary>
    /// <param name="signal">Сигнал для сохранения.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task SaveAsync(Signal signal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Асинхронно удаляет сигнал из постоянного хранилища и коллекции.
    /// </summary>
    /// <param name="id">Идентификатор сигнала для удаления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}