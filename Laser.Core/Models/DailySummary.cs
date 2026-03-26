using System;

namespace Laser.Core.Models
{
    public class DailySummary
    {
        public DateTime Date { get; set; }

        public TimeSpan CuttingTime { get; set; }
        public TimeSpan SetupTime { get; set; }
        public TimeSpan IdleTime { get; set; }
        public TimeSpan ErrorTime { get; set; }

        public TimeSpan TotalTime { get; set; }

        public double OperationRate =>
            TotalTime.TotalSeconds == 0
            ? 0
            : CuttingTime.TotalSeconds / TotalTime.TotalSeconds * 100;
    }
}