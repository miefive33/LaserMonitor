using Laser.Core.Models;
using Laser.Core.Services;
using OxyPlot;
using OxyPlot.Series;
using System.Windows;
using System.Windows.Controls;



namespace Laser.GUI.Views
{
    public partial class KpiPanelView : UserControl
    {
        public PlotModel PieModel { get; set; }

        public KpiPanelView()
        {
            InitializeComponent();
            this.DataContext = this;

            //DataContextChanged += OnDataContextChanged;

        }
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.MessageBox.Show($"DataContext type = {DataContext?.GetType()}");

            if (DataContext is not List<LogEvent> events)
                return;

            BuildChart(events);
        }

        public void SetEvents(List<LogEvent> events)
        {
            if (events == null || events.Count == 0)
            {
                System.Windows.MessageBox.Show("Eventsが空です");
                return;
            }

            System.Windows.MessageBox.Show($"SetEvents: {events.Count}件");

            BuildChart(events);
        }

        private void BuildChart(List<LogEvent> events)
        {
            var service = new DashboardService();

            var intervals = service.Analyze(events);

            // =========================
            // KPI集計
            // =========================
            var summary = new DailySummary();

            foreach (var i in intervals)
            {
                var duration = i.End - i.Start;

                switch (i.Type)
                {
                    case "Cutting":
                        summary.CuttingTime += duration;
                        break;
                    case "Setup":
                        summary.SetupTime += duration;
                        break;
                    case "Error":
                        summary.ErrorTime += duration;
                        break;
                    default:
                        summary.IdleTime += duration;
                        break;
                }

                summary.TotalTime += duration;
            }

            // =========================
            // グラフ生成（既存そのまま）
            // =========================
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

            PieModel = model;
            PiePlot.Model = PieModel;
            PieModel.InvalidatePlot(true);
            OnPropertyChanged(nameof(PieModel));

            // デバッグ表示
            /*System.Windows.MessageBox.Show(
                $"[KPI VIEW]\n" +
                $"Cutting={summary.CuttingTime.TotalHours}\n" +
                $"Setup={summary.SetupTime.TotalHours}\n" +
                $"Idle={summary.IdleTime.TotalHours}\n" +
                $"Error={summary.ErrorTime.TotalHours}\n" +
                $"Total={summary.TotalTime.TotalHours}"
            );*/
        }

        // INotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
}
