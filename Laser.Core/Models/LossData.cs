using System.Collections.Generic;

namespace Laser.Core.Models
{
    public class LossData
    {
        public Dictionary<string, double> TotalTime { get; set; } = new Dictionary<string, double>();

        public Dictionary<string, int> Count { get; set; } = new Dictionary<string, int>();
    }
}
