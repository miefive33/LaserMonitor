using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using System.Collections.Generic;
using Laser.Core.Models;
using Laser.Core.Parsers;
using Laser.Core.Services;
using Laser.GUI.Commands;

namespace Laser.GUI.ViewModels
{
    public class MainViewModel
    {
        public ICommand LoadLogCommand { get; }

        public MainViewModel()
        {
            LoadLogCommand = new RelayCommand(LoadLog);
        }

        private void LoadLog()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() != true) return;

            var filePath = dialog.FileName;

            var lines = File.ReadAllLines(filePath);

            var parser = new LogParser();
            List<LogEvent> events = parser.Load(filePath);

            var db = new SqliteService("laser.db");
            db.InsertLogEvents(events);

            System.Windows.MessageBox.Show("DB保存完了！");
        }
    }
}
