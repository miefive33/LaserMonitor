using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Series;



namespace Laser.GUI.Views
{
    public partial class KpiPanelView : UserControl
    {
        public PlotModel PieModel { get; set; }
        public KpiPanelView()
        {
            InitializeComponent();

            var model = new PlotModel { Title = "稼働内訳" };
            model.Background = OxyColors.Transparent;
            model.PlotAreaBackground = OxyColors.Transparent;
            model.TextColor = OxyColors.White;
            model.TitleColor = OxyColors.White;

            var pie = new PieSeries
            {
                StrokeThickness = 0.25,
                InsideLabelPosition = 0.8,
                InsideLabelFormat = "{1}: {0}%",
                AngleSpan = 360,
                StartAngle = 0,
                FontSize = 14,
            };

            pie.Slices.Add(new PieSlice("Cutting", 60) { Fill = OxyColor.FromRgb(0, 200, 0) });
            pie.Slices.Add(new PieSlice("Setup", 20) { Fill = OxyColor.FromRgb(150, 0, 200) });
            pie.Slices.Add(new PieSlice("Idle", 15) { Fill = OxyColors.Gray });
            pie.Slices.Add(new PieSlice("Error", 5) { Fill = OxyColors.Red });

            model.Series.Add(pie);

            PieModel = model;

            DataContext = this;
        }
    }
}
