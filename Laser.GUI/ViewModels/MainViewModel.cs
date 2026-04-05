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

        private DateTime _currentDate;

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

            _currentDate = DateTime.Today;

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
            BottleneckSummary = new List<string>();
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

        private List<string> _bottleneckSummary;

        public List<string> BottleneckSummary
        {
            get => _bottleneckSummary;
            private set
            {
                _bottleneckSummary = value;
                OnPropertyChanged(nameof(BottleneckSummary));
            }
        }

        public void UpdateKpi(DateTime date)
        {
            _currentDate = date;

            var sourceEvents = Events ?? new List<LogEvent>();
            var machineId = SelectedMachine?.Id ?? "laser";
            var dashboardResult = _dashboardService.AnalyzeDashboard(sourceEvents, _currentDate, machineId);

            KpiSummary = dashboardResult.DailySummary;
            PieModel = CreatePieModel(dashboardResult.SelectedSummary);
            LossSummary = dashboardResult.LossSummaryLines;
            ErrorSummary = dashboardResult.ErrorSummaryLines;
            BottleneckSummary = dashboardResult.BottleneckSummaryLines;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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
            private readonly Action<object?> _execute;

            public ParameterRelayCommand(Action<object?> execute)
            {
                _execute = execute;
            }

            public event EventHandler? CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                _execute(parameter);
            }
        }
    }
}