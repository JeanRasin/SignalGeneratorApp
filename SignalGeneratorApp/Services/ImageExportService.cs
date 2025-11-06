using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SignalGeneratorApp.Services;

/// <summary>
/// Сервис для экспорта графиков в изображения.
/// </summary>
public class ImageExportService
{
    private const int DefaultDpi = 96; // Стандартный DPI для экрана
    private const string FileFilter = "PNG|*.png"; // Фильтр для диалога сохранения

    /// <summary>
    /// Экспортирует указанный элемент (например, график) в PNG-файл.
    /// </summary>
    /// <param name="chartControl">UIElement, который нужно сохранить (например, CartesianChart).</param>
    public async Task ExportToPngAsync(CartesianChart? chartControl)
    {
        if (chartControl is null)
        {
            MessageBox.Show("Нет данных для экспорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var width = (int)chartControl.ActualWidth;
        var height = (int)chartControl.ActualHeight;

        if (width <= 0 || height <= 0)
        {
            MessageBox.Show("График имеет недопустимый размер.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Создаём RenderTargetBitmap — специальный битмап, который позволяет
        // "отрендерить" (нарисовать) визуальный элемент WPF (в данном случае график)
        // в пиксельное изображение.
        // Параметры:
        // - width, height — ширина и высота в пикселях.
        // - DefaultDpi, DefaultDpi — DPI (точек на дюйм), стандартное значение для экрана.
        // - PixelFormats.Pbgra32 — формат пикселей с премультиплицированным альфа-каналом (прозрачность).
        var rtb = new RenderTargetBitmap(width, height, DefaultDpi, DefaultDpi, PixelFormats.Pbgra32);

        // Вызываем метод Render, чтобы "нарисовать" содержимое chartControl на rtb.
        // После этого rtb содержит пиксельное изображение графика.
        rtb.Render(chartControl);

        // Создаём PngBitmapEncoder — кодировщик, который умеет сохранять битмапы в формат PNG.
        var encoder = new PngBitmapEncoder();
        // Создаём BitmapFrame из растрового изображения (RenderTargetBitmap)
        // и добавляем его как единственный кадр в encoder.
        encoder.Frames.Add(BitmapFrame.Create(rtb));

        var fileDialog = new SaveFileDialog { Filter = FileFilter };
        if (fileDialog.ShowDialog() == true) // Пользователь нажал "Сохранить"
        {
            await SaveImageAsync(encoder, fileDialog.FileName);
        }
    }

    /// <summary>
    /// Асинхронно сохраняет изображение в файл.
    /// </summary>
    /// <param name="encoder">Кодировщик PNG.</param>
    /// <param name="fileName">Путь к файлу.</param>
    private static async Task SaveImageAsync(BitmapEncoder encoder, string fileName)
    {
        try
        {
            await using var stream = File.Create(fileName);
            encoder.Save(stream);

            // Сообщение об успехе лучше показывать в UI-потоке.
            // Так как этот метод асинхронный, нужно вернуться в UI-контекст.
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show($"График успешно сохранён в:\n{fileName}", "Экспорт успешен", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
        catch (Exception ex)
        {
            // Сообщение об ошибке тоже нужно показать в UI-потоке.
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }
}