using SignalGeneratorApp.Models;
using SignalGeneratorApp.Repositories;
using System.Collections.ObjectModel;

namespace SignalGeneratorApp.Services;

/// <summary>
/// Реализация сервиса управления библиотекой сигналов.
/// </summary>
/// <remarks>
/// Создаёт экземпляр <see cref="SignalLibraryService"/>.
/// </remarks>
/// <param name="repository">Репозиторий для доступа к данным.</param>
/// <exception cref="ArgumentNullException">Если <paramref name="repository"/> равен null.</exception>
public class SignalLibraryService(ISignalRepository repository) : ISignalLibraryService
{
    private readonly ISignalRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <inheritdoc/>
    public ObservableCollection<Signal> Signals { get; } = [];

    /// <inheritdoc/>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var signals = await _repository.LoadSignalsAsync(cancellationToken);

            Signals.Clear();
            foreach (var signal in signals)
            {
                Signals.Add(signal);
            }
        }
        catch (OperationCanceledException)
        {
            // Операция отменена — ничего не делаем
            throw;
        }
        catch (Exception ex)
        {
            // Логирование ошибки (если есть ILogger)
            // Например: _logger?.LogError(ex, "");
            throw; // или обрабатывайте по-другому
        }
    }

    /// <inheritdoc/>
    public async Task SaveAsync(Signal signal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(signal);

        try
        {
            await _repository.SaveSignalAsync(signal, cancellationToken);

            signal.PointCount = signal.Points.Count;
            signal.Points.Clear();

            Signals.Insert(0, signal);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // _logger?.LogError(ex, "");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.DeleteSignalAsync(id, cancellationToken);

            // Удаляем из коллекции
            var signalToRemove = Signals.FirstOrDefault(s => s.Id == id);
            if (signalToRemove is not null)
            {
                Signals.Remove(signalToRemove);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // _logger?.LogError(ex, "");
            throw;
        }
    }
}