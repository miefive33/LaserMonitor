using Laser.Core.Analyzers;
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
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Laser.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand LoadLogCommand { get; }

        private readonly DashboardService _dashboardService;
        private readonly DailyReportBuilder _dailyReportBuilder;
        private readonly KpiBuilder _kpiBuilder;
        private readonly LossAnalyzer _lossAnalyzer;
        private readonly ErrorAnalyzer _errorAnalyzer;

        public MainViewModel()
        {
            LoadLogCommand = new RelayCommand(LoadLog);
            _dashboardService = new DashboardService();
            _dailyReportBuilder = new DailyReportBuilder();
            _kpiBuilder = new KpiBuilder();
            _lossAnalyzer = new LossAnalyzer();
            _errorAnalyzer = new ErrorAnalyzer();
            KpiSummary = new DailySummary { Date = DateTime.Today };
            PieModel = CreatePieModel(KpiSummary);
            LossSummary = new List<string>();
            ErrorSummary = new List<string>();
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

        private List<string> _lossSummary;

        public List<string> LossSummary
        {
            get => _lossSummary;
            private set
            {
                _lossSummary = value;
                OnPropertyChanged(nameof(LossSummary));
            }
        }

        private List<string> _errorSummary;

        public List<string> ErrorSummary
        {
            get => _errorSummary;
            private set
            {
                _errorSummary = value;
                OnPropertyChanged(nameof(ErrorSummary));
            }
        }

        public void UpdateKpi(DateTime date)
        {
            var sourceEvents = Events ?? new List<LogEvent>();
            var intervals = _dashboardService.Analyze(sourceEvents);

            var sample = intervals
                .Take(5)
                .Select(i => $"{i.Type} {i.Start:HH:mm:ss}-{i.End:HH:mm:ss} ({i.Duration.TotalSeconds:0.#}s)");
            Debug.WriteLine($"[MainViewModel] OperationInterval count: {intervals.Count}");
            Debug.WriteLine($"[MainViewModel] OperationInterval sample: {string.Join(" | ", sample)}");

            KpiSummary = _dailyReportBuilder.Build(date, intervals);

            var lossData = _lossAnalyzer.Analyze(intervals);
            var errorData = _errorAnalyzer.Analyze(intervals);

            LossSummary = _kpiBuilder.BuildLossSummary(lossData);
            ErrorSummary = _kpiBuilder.BuildErrorSummary(errorData);

            Debug.WriteLine($"[MainViewModel] LossSummary count: {LossSummary?.Count ?? 0}");
            Debug.WriteLine($"[MainViewModel] ErrorSummary count: {ErrorSummary?.Count ?? 0}");

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
            LoadLog(DateTime.Today);
        }

        public void LoadLog(DateTime date)
        {
            UpdateKpi(date);
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