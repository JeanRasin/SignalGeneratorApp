using System.ComponentModel;

namespace SignalGeneratorApp.Models
{
    /// <summary>
    /// Модель данных для отображения в пользовательской подсказке (tooltip).
    /// </summary>
    public class TooltipData : INotifyPropertyChanged
    {
        private string _timeText = "";
        private string _valueText = "";

        public string TimeText
        {
            get => _timeText;
            set { _timeText = value; OnPropertyChanged(nameof(TimeText)); }
        }

        public string ValueText
        {
            get => _valueText;
            set { _valueText = value; OnPropertyChanged(nameof(ValueText)); }
        }

        // INotifyPropertyChanged реализация (для обновления UI при изменении свойств)
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}