using System.Collections.Generic;

namespace Laser.Core.Models
{
    public class LossData
    {
        public Dictionary<string, double> TotalTime { get; set; } = new Dictionary<string, double>();

        public Dictionary<string, int> Count { get; set; } = new Dictionary<string, int>();
        public double SetupTime { get; set; }
        public double WaitingUpstreamTime { get; set; }
        public double WaitingDownstreamTime { get; set; }
        public double SystemInterruptTime { get; set; }
        public double ErrorTime { get; set; }
        public double UnknownTime { get; set; }

        // ★ ADDED
        public double TotalLossTime =>
            SetupTime + WaitingUpstreamTime + WaitingDownstreamTime +
            SystemInterruptTime + ErrorTime + UnknownTime;
    }
}
