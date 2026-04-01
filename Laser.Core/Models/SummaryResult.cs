using System.Collections.Generic;

namespace Laser.Core.Models
{
    public class SummaryResult
    {
        public double ActiveRate;
        public double LossRate;

        public Dictionary<string, double> Breakdown = new Dictionary<string, double>();
    }
}
