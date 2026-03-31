using System;
using System.Windows;
using System.Windows.Controls;

namespace Laser.GUI.Views
{
    public partial class HeaderView : UserControl
    {
        public event Action? OnUpdateRequested;
        public event Action? OnReloadRequested;

        // 追加：日付変更イベント
        public event Action<DateTime>? DateChanged;

        // 追加：現在選択中の日付
        private DateTime _currentDate;

        public HeaderView()
        {
            InitializeComponent();

            _currentDate = DateTime.Today;
            UpdateDateDisplay();
        }

        private void UpdateClicked(object sender, RoutedEventArgs e)
        {
            OnUpdateRequested?.Invoke();
        }

        private void ReloadClicked(object sender, RoutedEventArgs e)
        {
            OnReloadRequested?.Invoke();
        }

        private void PrevDate_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddDays(-1);
            UpdateDateDisplay();
        }

        private void NextDate_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddDays(1);
            UpdateDateDisplay();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePicker.SelectedDate.HasValue)
            {
                _currentDate = DatePicker.SelectedDate.Value.Date;
                UpdateDateDisplay();
            }
        }

        private void UpdateDateDisplay()
        {
            if (DateText != null)
            {
                DateText.Text = _currentDate.ToString("yyyy年M月d日");
            }

            if (DatePicker != null)
            {
                DatePicker.SelectedDate = _currentDate;
            }

            DateChanged?.Invoke(_currentDate);
        }
    }
}