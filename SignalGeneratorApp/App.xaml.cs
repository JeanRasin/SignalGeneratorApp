using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalGeneratorApp.Repositories;
using SignalGeneratorApp.Services;
using SignalGeneratorApp.Services.Formatting;
using SignalGeneratorApp.Services.Generators;
using SignalGeneratorApp.Services.NoiseGenerations;
using SignalGeneratorApp.Services.Processing;
using SignalGeneratorApp.Validators;
using SignalGeneratorApp.ViewModels;
using SignalGeneratorApp.Visualizers;
using System.IO;
using System.Windows;

namespace SignalGeneratorApp;

/// <summary>
/// Логика приложения: настройка DI-контейнера и запуск главного окна.
/// </summary>
public partial class App : Application
{
    private ServiceProvider _serviceProvider;
    private IConfiguration _configuration;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Создаем билдер конфигурации
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Устанавливаем путь к текущей директории приложения
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); // Загружаем appsettings.json

        _configuration = builder.Build(); // Строим конфигурацию

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        // 🔥 Асинхронное создание MainViewModel
        var mainVM = await CreateMainViewModelAsync();

        var imageExportService = _serviceProvider.GetRequiredService<ImageExportService>();

        var window = new MainWindow(mainVM, imageExportService);
        window.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Регистрируем IConfiguration
        services.AddSingleton(_configuration);

        // Регистрируем сервисы
        services.AddTransient<SignalParametersValidator>();
        services.AddSingleton<ISignalGenerator, SignalGeneratorWithStrategy>();
        services.AddSingleton<ISignalRepository, SignalRepository>();
        services.AddSingleton<ISignalLibraryService, SignalLibraryService>();
        services.AddSingleton<ISignalVisualizer, LineSignalVisualizer>();
        services.AddSingleton<ISignalProcessingService, SignalProcessingService>();
        services.AddSingleton<INoiseGenerationStrategy, GaussianNoiseGenerationStrategy>();
        services.AddSingleton<ISignalInfoFormatter, SignalInfoFormatter>();
        //services.AddSingleton<ISignalHistoryService, SignalHistoryService>();
        services.AddSingleton<ImageExportService>();

        services.AddSingleton<SignalViewModel>();
    }

    private async Task<MainViewModel> CreateMainViewModelAsync()
    {
        // Получаем зависимости из DI-контейнера
        var signalGenerator = _serviceProvider.GetRequiredService<ISignalGenerator>();
        var signalVM = _serviceProvider.GetRequiredService<SignalViewModel>();
        var processingService = _serviceProvider.GetRequiredService<ISignalProcessingService>();
        var validator = _serviceProvider.GetRequiredService<SignalParametersValidator>();
        var infoFormatter = _serviceProvider.GetRequiredService<ISignalInfoFormatter>();
        var signalLibraryService = _serviceProvider.GetRequiredService<ISignalLibraryService>();
        //var signalHistoryService = _serviceProvider.GetRequiredService<ISignalHistoryService>();

        // Создаём и инициализируем асинхронно
        return await MainViewModel.CreateAsync(
            signalGenerator,
            signalVM,
            processingService,
            infoFormatter,
            validator,
            signalLibraryService);
    }
}