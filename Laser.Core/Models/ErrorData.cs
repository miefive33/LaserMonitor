using System;
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

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }

        public string ErrorCode { get; set; }

        public string OperationContext { get; set; }

        public double InterruptionTime { get; set; }

        public double SeverityScore { get; set; }
    }
}