using Laser.Core.Builders;
using Laser.Core.Models;
using Laser.Core.Parsers;
using Laser.Core.Services;
using Laser.GUI.Commands;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Laser.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand LoadLogCommand { get; }

        private readonly DashboardService _dashboardService;
        private readonly DailyReportBuilder _dailyReportBuilder;

        public MainViewModel()
        {
            LoadLogCommand = new RelayCommand(LoadLog);
            _dashboardService = new DashboardService();
            _dailyReportBuilder = new DailyReportBuilder();
            KpiSummary = new DailySummary { Date = DateTime.Today };
            PieModel = CreatePieModel(KpiSummary);
        }

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

        private DailySummary _kpiSummary;

        public DailySummary KpiSummary
        {
            get => _kpiSummary;
            private set
            {
                _kpiSummary = value;
                OnPropertyChanged(nameof(KpiSummary));
            }
        }

        private PlotModel _pieModel;

        public PlotModel PieModel
        {
            get => _pieModel;
            private set
            {
                _pieModel = value;
                OnPropertyChanged(nameof(PieModel));
            }
        }

        public void UpdateKpi(DateTime date)
        {
            var sourceEvents = Events ?? new List<LogEvent>();
            var intervals = _dashboardService.Analyze(sourceEvents);
            KpiSummary = _dailyReportBuilder.Build(date, intervals);
            PieModel = CreatePieModel(KpiSummary);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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

            Events = events;
            UpdateKpi(DateTime.Today);
        }

        private static PlotModel CreatePieModel(DailySummary summary)
        {
            var model = new PlotModel
            {
                Title = "稼働内訳",
                Background = OxyColors.Transparent,
                PlotAreaBackground = OxyColors.Transparent,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.White
            };

            double total = summary.TotalTime.TotalSeconds;
            if (total == 0) total = 1;

            var pie = new PieSeries
            {
                StrokeThickness = 0.25,
                InsideLabelPosition = 0.8,
                InsideLabelFormat = "{1}: {0:0.0}%",
                AngleSpan = 360,
                StartAngle = 0,
                FontSize = 14,
                InnerDiameter = 0.6,
            };

            pie.Slices.Add(new PieSlice("Cutting", summary.CuttingTime.TotalSeconds / total * 100)
            { Fill = OxyColor.FromRgb(0, 200, 0) });

            pie.Slices.Add(new PieSlice("Setup", summary.SetupTime.TotalSeconds / total * 100)
            { Fill = OxyColor.FromRgb(150, 0, 200) });

            pie.Slices.Add(new PieSlice("Idle", summary.IdleTime.TotalSeconds / total * 100)
            { Fill = OxyColors.Gray });

            pie.Slices.Add(new PieSlice("Error", summary.ErrorTime.TotalSeconds / total * 100)
            { Fill = OxyColors.Red });

            model.Series.Add(pie);
            return model;
        }
    }
}