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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Laser.GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand LoadLogCommand { get; }
        public ICommand SelectMachineCommand { get; }

        private readonly DashboardService _dashboardService;
        private readonly MachineAnalyzer _machineAnalyzer;
        private readonly SorterAnalyzer _sorterAnalyzer;
        private readonly SystemAnalyzer _systemAnalyzer;

        private DateTime _currentDate;
        private List<OperationInterval> _currentIntervals;

        public MainViewModel()
        {
            LoadLogCommand = new RelayCommand(LoadLog);
            SelectMachineCommand = new ParameterRelayCommand(param =>
            {
                if (param is Machine machine)
                {
                    SelectedMachine = machine;
                }
            });

            _dashboardService = new DashboardService();
            _machineAnalyzer = new MachineAnalyzer();
            _sorterAnalyzer = new SorterAnalyzer();
            _systemAnalyzer = new SystemAnalyzer();

            _currentDate = DateTime.Today;
            _currentIntervals = new List<OperationInterval>();

            Machines = new ObservableCollection<Machine>
            {
                new Machine { Id = "laser", Name = "Laser", Color = "#FF6F00", Order = 1 },
                new Machine { Id = "sorting", Name = "Sorting", Color = "#00ACC1", Order = 2 },
                new Machine { Id = "system", Name = "System", Color = "#8E24AA", Order = 3 }
            };

            SelectedMachine = Machines.OrderBy(x => x.Order).FirstOrDefault();

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

        public ObservableCollection<Machine> Machines { get; }

        private Machine _selectedMachine;

        public Machine SelectedMachine
        {
            get => _selectedMachine;
            set
            {
                if (_selectedMachine == value)
                    return;

                _selectedMachine = value;
                OnPropertyChanged(nameof(SelectedMachine));
                LoadLog(_currentDate);
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
                $"Target: {SelectedMachine?.Name}",
                $"ActiveRate: {summary.ActiveRate:0.0}%",
                $"LossRate: {summary.LossRate:0.0}%"
            };
        }

        private SummaryResult AnalyzeByTarget(List<OperationInterval> intervals, Machine machine)
        {
            switch (machine?.Id)
            {
                case "laser":
                    return _machineAnalyzer.Analyze(intervals);

                case "sorting":
                    return _sorterAnalyzer.Analyze(intervals);

                case "system":
                    return _systemAnalyzer.Analyze(intervals);

                default:
                    return _machineAnalyzer.Analyze(intervals);
            }
        }

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

        private class ParameterRelayCommand : ICommand
        {
            private readonly Action<object> _execute;

            public ParameterRelayCommand(Action<object> execute)
            {
                _execute = execute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }
        }
    }
}