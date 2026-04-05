using System.Collections.Generic;

namespace Laser.Core.Models
{
    public class BottleneckData
    {
        public List<BottleneckItem> Items { get; set; } = new List<BottleneckItem>();
    }

    public class BottleneckItem
    {
        public string Category { get; set; }
        public string Perspective { get; set; }
        public int Count { get; set; }
        public double TotalTime { get; set; }
        public double Severity { get; set; }
        public double Score { get; set; }
    }
}