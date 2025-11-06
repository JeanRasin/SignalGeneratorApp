using SignalGeneratorApp.Services;
using SignalGeneratorApp.ViewModels;
using System.Windows;

namespace SignalGeneratorApp;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainVM;
    private readonly ImageExportService _imageExportService;

    public MainWindow(MainViewModel mainVM, ImageExportService imageExportService)
    {
        InitializeComponent();
        _mainVM = mainVM;
        DataContext = mainVM;
        _imageExportService = imageExportService;
    }

    // Метод для экспорта графика в изображение
    private async void ExportChartToImage(object sender, RoutedEventArgs e)
    {
       await _imageExportService.ExportToPngAsync(SignalChartControl);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _mainVM?.Dispose();
    }
}