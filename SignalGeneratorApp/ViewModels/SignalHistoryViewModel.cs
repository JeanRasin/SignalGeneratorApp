using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SignalGeneratorApp.Models;
using SignalGeneratorApp.Services;
using System.Collections.ObjectModel;

namespace SignalGeneratorApp.ViewModels;

public partial class SignalHistoryViewModel(
    ISignalLibraryService library,
    Func<Signal, CancellationToken, Task> onSignalLoadRequestedAsync) : ObservableObject
{
    private readonly ISignalLibraryService _library = library ?? throw new ArgumentNullException(nameof(library));
    private readonly Func<Signal, CancellationToken, Task> _onSignalLoadRequestedAsync = onSignalLoadRequestedAsync ?? throw new ArgumentNullException(nameof(onSignalLoadRequestedAsync));

    public ObservableCollection<Signal> SavedSignals => _library.Signals;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanLoadSelectedSignal), nameof(CanDeleteSelectedSignal))]
    private Signal? _selectedSignal;

    public bool CanLoadSelectedSignal => SelectedSignal is not null;
    public bool CanDeleteSelectedSignal => SelectedSignal is not null;

    /// <summary>
    /// Асинхронно загружает историю сигналов через сервис библиотеки.
    /// </summary>
    /// <param name="cancellationToken">Токен для отмены операции.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Вызываем LoadAsync у сервиса ---
            await _library.LoadAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Игнорируем отмену
        }
        catch (Exception ex) // Можно добавить обработку других ошибок, например, логгирование
        {
            // _logger?.LogError(ex, "");
            throw; // Или обработка ошибки через UI-сервис
        }
    }

    /// <summary>
    /// Команда для загрузки выбранного сигнала.
    /// </summary>
    [RelayCommand]
    private async Task LoadSelectedSignal(CancellationToken cancellationToken = default)
    {
        if (_selectedSignal is { } signal)
        {
            await _onSignalLoadRequestedAsync(signal, cancellationToken);
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task DeleteSelectedSignal(CancellationToken cancellationToken = default)
    {
        if (SelectedSignal?.Id is long id)
        {
            try
            {
                await _library.DeleteAsync(id, cancellationToken);
                // _library сам удаляет сигнал из своей коллекции _library.Signals
                // Поэтому SavedSignals (который проксирует _library.Signals) автоматически обновится
                SelectedSignal = null; // Сбрасываем выбор
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену
            }
            catch (Exception ex) // Обработка ошибок удаления
            {
                // _logger?.LogError(ex, "", id);
                // Уведомить пользователя через UI-сервис
                throw;
            }
        }
    }

    public async Task SaveSignalAsync(Signal signal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(signal);

        try
        {
            await _library.SaveAsync(signal, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Игнорируем отмену
        }
        catch (Exception ex) // Обработка ошибок добавления
        {
            // _logger?.LogError(ex, "", id);
            // Уведомить пользователя через UI-сервис
            throw;
        }

    }
}