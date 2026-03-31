using System.Collections.Generic;

namespace Laser.Core.Models
{
    public class ErrorData
    {
        public List<ErrorItem> Items { get; set; } = new List<ErrorItem>();
    }

    public class ErrorItem
    {
        public string Type { get; set; }

        public int Count { get; set; }

        public double TotalTime { get; set; }

        public double AvgDuration { get; set; }

        public double MaxDuration { get; set; }
    }
}