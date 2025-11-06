using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SignalGeneratorApp.Models;
using SignalGeneratorApp.Models.Enums;
using SignalGeneratorApp.Services;
using SignalGeneratorApp.Services.Formatting;
using SignalGeneratorApp.Services.Generators;
using SignalGeneratorApp.Services.Processing;
using SignalGeneratorApp.Validators;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SignalGeneratorApp.ViewModels;

/// <summary>
/// Основная ViewModel приложения, управляющая параметрами сигнала, его генерацией,
/// сохранением, загрузкой и обработкой.
/// Реализует валидацию через <see cref="INotifyDataErrorInfo"/>.
/// </summary>
public partial class MainViewModel : ObservableObject, INotifyDataErrorInfo, IDisposable
{
    private SignalParameter _parameters = new();

    private readonly ISignalGenerator _signalGenerator;
    private readonly ISignalProcessingService _processingService;
    private readonly SignalParametersValidator _parametersValidator;
    private readonly ISignalInfoFormatter _signalInfoFormatter;
    private readonly Dictionary<string, List<string>> _errors = [];

    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private SignalTypeEnum _selectedSignalType = SignalTypeEnum.Sine;

    /// <summary>
    /// ViewModel для отображения сигнала на графике.
    /// </summary>
    public required SignalViewModel SignalVM { get; init; }

    /// <summary>
    /// Список доступных типов сигналов для выбора в UI.
    /// </summary>
    public ObservableCollection<SignalTypeEnum> SignalTypes { get; } =
        new(Enum.GetValues<SignalTypeEnum>());

    public SignalHistoryViewModel SignalHistoryVM { get; set; }

    /// <summary>
    /// Указывает, являются ли текущие параметры валидными.
    /// </summary>
    public bool AreParametersValid => !HasErrors;

    /// <summary>
    /// Амплитуда сигнала в вольтах.
    /// </summary>
    public double? Amplitude
    {
        get => _parameters.Amplitude;
        set
        {
            _parameters.Amplitude = value;
            OnPropertyChanged();
            ValidateParameters();
        }
    }

    /// <summary>
    /// Частота сигнала в герцах.
    /// </summary>
    public double? Frequency
    {
        get => _parameters.Frequency;
        set
        {
            _parameters.Frequency = value;
            OnPropertyChanged();
            ValidateParameters();
        }
    }

    /// <summary>
    /// Начальная фаза сигнала в радианах.
    /// </summary>
    public double? Phase
    {
        get => _parameters.Phase;
        set
        {
            _parameters.Phase = value;
            OnPropertyChanged();
            ValidateParameters();
        }
    }

    /// <summary>
    /// Количество точек в генерируемом сигнале.
    /// </summary>
    public int? PointCount
    {
        get => _parameters.PointCount;
        set
        {
            _parameters.PointCount = value;
            OnPropertyChanged();
            ValidateParameters();
        }
    }

    /// <summary>
    /// Длительность сигнала в секундах.
    /// </summary>
    public double? TimeInterval
    {
        get => _parameters.TimeInterval;
        set
        {
            _parameters.TimeInterval = value;
            OnPropertyChanged();
            ValidateParameters();
        }
    }

    /// <summary>
    /// Уровень шума (0–100).
    /// </summary>
    public int NoiseLevel
    {
        get => _parameters.NoiseLevel;
        set
        {
            _parameters.NoiseLevel = value;
            OnPropertyChanged();
            ValidateParameters();
        }
    }

    /// <summary>
    /// Указывает, загружен ли сигнал для отображения.
    /// </summary>
    public bool HasSignalData => SignalVM.CurrentSignal?.Points?.Count > 0;

    /// <summary>
    /// Информация о текущем сигнале для отображения в UI.
    /// </summary>
    public string CurrentSignalInfo => _signalInfoFormatter.Format(SignalVM.CurrentSignal);

    /// <summary>
    /// Указывает, есть ли ошибки валидации.
    /// </summary>
    public bool HasErrors => _errors.Count != 0;

    /// <inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Создаёт экземпляр <see cref="MainViewModel"/> с внедрёнными зависимостями.
    /// Асинхронная инициализация выполняется в <see cref="CreateAsync"/>.
    /// </summary>
    private MainViewModel(
        ISignalGenerator signalGenerator,
        ISignalProcessingService processingService,
        SignalParametersValidator parametersValidator,
        ISignalInfoFormatter signalInfoFormatter)
    {
        _signalGenerator = signalGenerator;
        _processingService = processingService;
        _parametersValidator = parametersValidator;
        _signalInfoFormatter = signalInfoFormatter;

        // Подписка на изменения ошибок для обновления AreParametersValid
        ErrorsChanged += (_, _) => OnPropertyChanged(nameof(AreParametersValid));
    }

    /// <summary>
    /// Асинхронно создаёт и инициализирует экземпляр <see cref="MainViewModel"/>.
    /// </summary>
    /// <param name="signalGenerator">Сервис генерации сигнала.</param>
    /// <param name="signalVM">ViewModel для отображения сигнала.</param>
    /// <param name="processingService">Сервис обработки сигнала.</param>
    /// <param name="signalInfoFormatter">Сервис информации о сигнале.</param>
    /// <param name="parametersValidator">Валидатор параметров сигнала.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Инициализированный экземпляр <see cref="MainViewModel"/>.</returns>
    public static async Task<MainViewModel> CreateAsync(
        ISignalGenerator signalGenerator,
        SignalViewModel signalVM,
        ISignalProcessingService processingService,
        ISignalInfoFormatter signalInfoFormatter,
        SignalParametersValidator parametersValidator,
        ISignalLibraryService signalLibraryService,
        CancellationToken cancellationToken = default)
    {
        var vm = new MainViewModel(signalGenerator, processingService, parametersValidator, signalInfoFormatter)
        {
            SignalVM = signalVM
        };

        vm.SignalHistoryVM = new SignalHistoryViewModel(
               signalLibraryService,
               vm.LoadSignalIntoMainViewAsync
         );

        await vm.SignalHistoryVM.InitializeAsync(cancellationToken);

        return vm;
    }

    /// <summary>
    /// Загружает указанный сигнал в основную панель: отображает на графике и заполняет форму параметров.
    /// </summary>
    /// <param name="signal">Сигнал для загрузки. Не должен быть null.</param>
    /// <exception cref="ArgumentNullException">Если <paramref name="signal"/> равен null.</exception>
    public async Task LoadSignalIntoMainViewAsync(Signal signal, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(signal);

        // Загружаем сигнал в график (асинхронно, но без ожидания — UI обновится через привязку)
        await SignalVM.LoadSignalAsync(signal, cancellationToken);

        // Синхронно обновляем параметры формы
        SelectedSignalType = signal.Type;
        Amplitude = signal.Amplitude;
        Frequency = signal.Frequency;
        Phase = signal.Phase;
        PointCount = signal.Points.Count; // Обратите внимание: это Count, а не TimeInterval!
        TimeInterval = signal.TimeInterval;
        NoiseLevel = signal.NoiseLevel;

        // Уведомляем UI об изменениях вычисляемых свойств
        OnPropertyChanged(nameof(CurrentSignalInfo));
        OnPropertyChanged(nameof(HasSignalData));
    }

    /// <summary>
    /// Выполняет валидацию текущих параметров сигнала и обновляет состояние ошибок.
    /// </summary>
    private void ValidateParameters()
    {
        var result = _parametersValidator.Validate(_parameters);

        var oldErrorProps = new HashSet<string>(_errors.Keys);
        _errors.Clear();

        var newErrorProps = new HashSet<string>();

        foreach (var error in result.Errors)
        {
            _errors.TryAdd(error.PropertyName, []);
            _errors[error.PropertyName].Add(error.ErrorMessage);
            newErrorProps.Add(error.PropertyName);
        }

        var allProps = new[]
        {
            nameof(Amplitude),
            nameof(Frequency),
            nameof(Phase),
            nameof(PointCount),
            nameof(TimeInterval),
            nameof(NoiseLevel)
        };

        foreach (var prop in allProps)
        {
            bool wasInError = oldErrorProps.Contains(prop);
            bool isInError = newErrorProps.Contains(prop);
            if (wasInError != isInError)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
            }
        }

        OnPropertyChanged(nameof(AreParametersValid));
    }

    /// <inheritdoc/>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.Values.SelectMany(e => e);

        return _errors.GetValueOrDefault(propertyName, []);
    }

    /// <summary>
    /// Применяет медианный фильтр к текущему сигналу.
    /// </summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task ApplyMedianFilter()
    {
        if (SignalVM.CurrentSignal is null) return;

        var cts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _cancellationTokenSource, cts);

        // Заменяем CTS, отменяем старый (если не null) и освобождаем ресурсы, игнорируя возможные исключения
        SafeCancelAndDispose(oldCts);

        try
        {
            var filteredSignal = await Task.Run(() => _processingService.ApplyMedianFilter(SignalVM.CurrentSignal!, 5, cts.Token), cts.Token);
            await SignalVM.LoadSignalAsync(filteredSignal, cts.Token);

            OnPropertyChanged(nameof(CurrentSignalInfo));
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            // Операция отменена — игнорируем
        }
        finally
        {
            cts.Dispose();
        }
    }

    /// <summary>
    /// Генерирует новый сигнал указанного типа на основе текущих параметров.
    /// </summary>
    /// <param name="type">Тип генерируемого сигнала.</param>
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task GenerateSignal(SignalTypeEnum type)
    {
        ValidateParameters();
        if (HasErrors) return;

        // Убедимся, что все значения не null (валидация прошла)
        if (!Amplitude.HasValue || !Frequency.HasValue || !Phase.HasValue ||
            !PointCount.HasValue || !TimeInterval.HasValue) return;

        var cts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _cancellationTokenSource, cts);

        // Заменяем CTS, отменяем старый (если не null) и освобождаем ресурсы, игнорируя возможные исключения
        SafeCancelAndDispose(oldCts);

        try
        {
            var signal = await _signalGenerator.GenerateSignalAsync(
                type, Amplitude.Value, Frequency.Value, Phase.Value,
                PointCount.Value, TimeInterval.Value, NoiseLevel, cts.Token);
            await SignalVM.LoadSignalAsync(signal);

            OnPropertyChanged(nameof(CurrentSignalInfo));
            OnPropertyChanged(nameof(HasSignalData));
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            // Игнорируем отмену
        }
        finally
        {
            // Убедимся, что CTS уничтожен даже при исключении
            cts.Dispose();
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task SaveCurrentSignal()
    {
        if (SignalVM.CurrentSignal is null) return;

        var cts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _cancellationTokenSource, cts);

        // Заменяем CTS, отменяем старый (если не null) и освобождаем ресурсы, игнорируя возможные исключения
        SafeCancelAndDispose(oldCts);

        try
        {
            await SignalHistoryVM.SaveSignalAsync(SignalVM.CurrentSignal, cts.Token);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            // Игнорируем
        }
        catch (Exception ex)
        {
            // Опционально: показать ошибку пользователю
            // Например, через MessageBox (но лучше через сервис уведомлений)
            throw;
        }
    }

    /// <summary>
    /// Очищает текущий сигнал и сбрасывает параметры.
    /// </summary>
    [RelayCommand]
    private void ClearSignal()
    {
        SignalVM.ChartSeries.Clear();
        SignalVM.CurrentSignal = null;

        _parameters = new();

        // Уведомляем UI о сбросе всех параметров
        OnPropertyChanged(nameof(Amplitude));
        OnPropertyChanged(nameof(Frequency));
        OnPropertyChanged(nameof(Phase));
        OnPropertyChanged(nameof(PointCount));
        OnPropertyChanged(nameof(TimeInterval));
        OnPropertyChanged(nameof(NoiseLevel));

        ValidateParameters(); // сброс ошибок

        OnPropertyChanged(nameof(CurrentSignalInfo));
        OnPropertyChanged(nameof(HasSignalData));
    }

    /// <summary>
    /// Безопасно отменяет и уничтожает (disposes) указанный <see cref="CancellationTokenSource"/>.
    /// Если <paramref name="cts"/> равен <c>null</c>, метод ничего не делает.
    /// Ошибки <see cref="ObjectDisposedException"/>, возникающие при вызове <see cref="CancellationTokenSource.Cancel"/>
    /// или <see cref="CancellationTokenSource.Dispose"/>, игнорируются.
    /// </summary>
    /// <param name="cts">Экземпляр <see cref="CancellationTokenSource"/> для отмены и освобождения, может быть <c>null</c>.</param>
    private static void SafeCancelAndDispose(CancellationTokenSource? cts)
    {
        if (cts is null) return;

        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Другой поток уже уничтожил CTS. Это допустимо.
        }

        try
        {
            cts.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Другой поток уже уничтожил CTS. Это допустимо.
        }
    }

    #region IDisposable

    private bool _disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            var cts = Interlocked.Exchange(ref _cancellationTokenSource, null);
            cts?.Cancel();
            cts?.Dispose();
        }

        _disposed = true;
    }

    #endregion
}