using Laser.Core.Models;
using Laser.Core.Parsers;
using Laser.GUI.Commands;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Laser.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand LoadLogCommand { get; }

        public MainViewModel()
        {
            LoadLogCommand = new RelayCommand(LoadLog);

            // 起動時に自動読み込み（必要なら残す）
            LoadLogFromFile(@"C:\Users\nanoa\Documents\Cs\LaserMonitor\2603M1.txt");
        }

        // =========================
        // ★ ここが重要
        // =========================
        private List<LogEvent> _events;

        public List<LogEvent> Events
        {
            get => _events;
            set
            {
                _events = value;
                OnPropertyChanged(nameof(Events));
            }
        }

        // =========================
        // INotifyPropertyChanged
        // =========================
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // =========================
        // ログ読み込み
        // =========================
        private void LoadLog()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() != true) return;

            LoadLogFromFile(dialog.FileName);
        }

        private void LoadLogFromFile(string filePath)
        {
            var parser = new LogParser();
            var events = parser.Load(filePath);

            // ★ ここだけでOK（解析しない）
            Events = events;
        }
    }
}