using System;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Laser.GUI.Views
{
    public partial class BottomPanelView : UserControl
    {
        public PlotModel WeeklyModel { get; set; } = null!;

        public BottomPanelView()
        {
            InitializeComponent();
            CreateChart();

            // XAML側のPlotViewにセット
            WeeklyChart.Model = WeeklyModel;
        }

        private void CreateChart()
        {
            WeeklyModel = new PlotModel
            {
                Title = "Weekly Operation Rate",
                Background = OxyColors.Transparent,
                PlotAreaBorderColor = OxyColors.Transparent,
                TextColor = OxyColors.White
            };

            // X軸（日付）
            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                TextColor = OxyColors.White
            };

            xAxis.Labels.Add("Mon");
            xAxis.Labels.Add("Tue");
            xAxis.Labels.Add("Wed");
            xAxis.Labels.Add("Thu");
            xAxis.Labels.Add("Fri");
            xAxis.Labels.Add("Sat");
            xAxis.Labels.Add("Sun");

            // Y軸（稼働率）
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 100,
                Title = "%",
                TextColor = OxyColors.White
            };

            // 折れ線グラフ
            var series = new LineSeries
            {
                Title = "Operation Rate",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                StrokeThickness = 2,
                Color = OxyColors.SkyBlue
            };

            // 🔧 ダミーデータ（あとでログ解析結果に差し替え）
            series.Points.Add(new DataPoint(0, 65));
            series.Points.Add(new DataPoint(1, 70));
            series.Points.Add(new DataPoint(2, 55));
            series.Points.Add(new DataPoint(3, 80));
            series.Points.Add(new DataPoint(4, 75));
            series.Points.Add(new DataPoint(5, 60));
            series.Points.Add(new DataPoint(6, 50));

            WeeklyModel.Axes.Add(xAxis);
            WeeklyModel.Axes.Add(yAxis);
            WeeklyModel.Series.Add(series);
        }
    }
}