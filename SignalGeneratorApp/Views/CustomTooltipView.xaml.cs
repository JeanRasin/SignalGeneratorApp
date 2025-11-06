using System.Windows.Controls;

namespace SignalGeneratorApp.Views;

public partial class CustomTooltipView : UserControl
{
    public CustomTooltipView()
    {
        InitializeComponent();
        // Подписываемся на событие изменения DataContext
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        UpdateData(e.NewValue);
    }

    // Метод для обновления содержимого тултипа на основе данных
    // LiveCharts передаёт DataContext в виде коллекции TooltipPointViewModel
    public void UpdateData(object dataContext)
    {
        // Очищаем текст на случай, если данных нет
        TimeTextBlock.Text = "Время: N/A";
        ValueTextBlock.Text = "Значение: N/A";

        if (dataContext is System.Collections.IEnumerable pointsCollection)
        {
            // Берём первую точку из коллекции (обычно при наведении на одну точку)
            var firstPoint = pointsCollection.Cast<object>().FirstOrDefault();
            if (firstPoint != null)
            {
                // Используем рефлексию или приведение типа, чтобы получить X и Y
                // Универсальный способ - через свойства X и Y у ViewModel точки
                var pointVMType = firstPoint.GetType();
                var xProperty = pointVMType.GetProperty("X");
                var yProperty = pointVMType.GetProperty("Y");

                if (xProperty != null && yProperty != null)
                {
                    var xValue = xProperty.GetValue(firstPoint);
                    var yValue = yProperty.GetValue(firstPoint);

                    if (xValue is double time && yValue is double value)
                    {
                        TimeTextBlock.Text = $"Время: {time:F1} сек";
                        ValueTextBlock.Text = $"Значение: {value:F2}";
                    }
                }
            }
        }
    }
}