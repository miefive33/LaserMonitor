using System.Collections.Generic;

namespace Laser.Core.Models
{
    public class DashboardAnalysisResult
    {
        public DailySummary DailySummary { get; set; } = new DailySummary();
        public SummaryResult SelectedSummary { get; set; } = new SummaryResult();
        public LossData LossData { get; set; } = new LossData();
        public ErrorData ErrorData { get; set; } = new ErrorData();
        public BottleneckData BottleneckData { get; set; } = new BottleneckData();
        public List<string> LossSummaryLines { get; set; } = new List<string>();
        public List<string> ErrorSummaryLines { get; set; } = new List<string>();
        public List<string> BottleneckSummaryLines { get; set; } = new List<string>();
    }
}