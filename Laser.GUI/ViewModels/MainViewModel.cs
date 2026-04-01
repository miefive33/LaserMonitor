using Laser.Core.Analyzers;
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
using System.Linq;
using System.Windows.Input;

namespace Laser.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand LoadLogCommand { get; }

        private readonly DashboardService _dashboardService;
        private readonly MachineAnalyzer _machineAnalyzer;
        private readonly SorterAnalyzer _sorterAnalyzer;
        private readonly SystemAnalyzer _systemAnalyzer;

        // ★ ADDED
        private DateTime _currentDate;
        private List<OperationInterval> _currentIntervals;

        public MainViewModel()
        {
            LoadLogCommand = new RelayCommand(LoadLog);
            _dashboardService = new DashboardService();
            _machineAnalyzer = new MachineAnalyzer();
            _sorterAnalyzer = new SorterAnalyzer();
            _systemAnalyzer = new SystemAnalyzer();

            _currentDate = DateTime.Today;
            _currentIntervals = new List<OperationInterval>();

            KpiSummary = new DailySummary { Date = DateTime.Today };
            PieModel = CreatePieModel(new SummaryResult());
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

        // ★ ADDED
        public Array AvailableMachines => Enum.GetValues(typeof(TargetMachine));

        // ★ ADDED
        private TargetMachine _selectedMachine = TargetMachine.Laser;

        // ★ ADDED
        public TargetMachine SelectedMachine
        {
            get => _selectedMachine;
            set
            {
                if (_selectedMachine == value)
                    return;

                _selectedMachine = value;
                OnPropertyChanged(nameof(SelectedMachine));
                RecalculateSelectedSummary();
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
            _currentDate = date;

            var sourceEvents = Events ?? new List<LogEvent>();
            _currentIntervals = _dashboardService.Analyze(sourceEvents);

            RecalculateSelectedSummary();
        }

        // ★ ADDED
        private void RecalculateSelectedSummary()
        {
            var summary = AnalyzeByTarget(_currentIntervals, SelectedMachine);

            KpiSummary = ToDailySummary(_currentDate, summary);
            PieModel = CreatePieModel(summary);

            LossSummary = summary.Breakdown
                .OrderByDescending(x => x.Value)
                .Select(x => $"{x.Key}: {TimeSpan.FromSeconds(x.Value):hh\\:mm\\:ss}")
                .ToList();

            ErrorSummary = new List<string>
            {
                $"Target: {SelectedMachine}",
                $"ActiveRate: {summary.ActiveRate:0.0}%",
                $"LossRate: {summary.LossRate:0.0}%"
            };
        }

        // ★ ADDED
        private SummaryResult AnalyzeByTarget(List<OperationInterval> intervals, TargetMachine target)
        {
            switch (target)
            {
                case TargetMachine.Laser:
                    return _machineAnalyzer.Analyze(intervals);

                case TargetMachine.Sorter:
                    return _sorterAnalyzer.Analyze(intervals);

                case TargetMachine.System:
                    return _systemAnalyzer.Analyze(intervals);

                default:
                    return _machineAnalyzer.Analyze(intervals);
            }
        }

        // ★ ADDED
        private static DailySummary ToDailySummary(DateTime date, SummaryResult summary)
        {
            var totalSeconds = summary.Breakdown.Values.Sum();

            return new DailySummary
            {
                Date = date,
                TotalTime = TimeSpan.FromSeconds(totalSeconds),
                CuttingTime = TimeSpan.FromSeconds(totalSeconds * summary.ActiveRate / 100.0),
                IdleTime = TimeSpan.FromSeconds(totalSeconds * summary.LossRate / 100.0)
            };
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

        // ★ MODIFIED
        private static PlotModel CreatePieModel(SummaryResult summary)
        {
            var model = new PlotModel
            {
                Title = "稼働内訳",
                Background = OxyColors.Transparent,
                PlotAreaBackground = OxyColors.Transparent,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.White
            };

            var total = summary.Breakdown.Values.Sum();
            if (total <= 0)
                return model;

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

            foreach (var item in summary.Breakdown.OrderByDescending(x => x.Value))
            {
                pie.Slices.Add(new PieSlice(item.Key, item.Value / total * 100));
            }

            model.Series.Add(pie);
            return model;
        }
    }
}