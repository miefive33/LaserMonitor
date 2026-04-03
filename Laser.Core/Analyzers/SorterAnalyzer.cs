using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class SorterAnalyzer
    {
        public SummaryResult Analyze(List<OperationInterval> intervals)
        {
            var result = new SummaryResult();
            var source = (intervals ?? new List<OperationInterval>())
                 .Where(i => !i.IsScheduleActive)
                 .ToList();
            double total = source.Sum(i => Math.Max(0, i.Duration.TotalSeconds));
            // ★ CHANGED: ignore ScheduleActive wrapper intervals
            if (total <= 0)
                return result;

            foreach (var interval in source)
            {
                var seconds = Math.Max(0, interval.Duration.TotalSeconds);
                var key = Classify(interval?.Type);

                if (!result.Breakdown.ContainsKey(key))
                    result.Breakdown[key] = 0;

                result.Breakdown[key] += seconds;
            }

            var activeSeconds = result.Breakdown.ContainsKey("Sorting active") ? result.Breakdown["Sorting active"] : 0;
            result.ActiveRate = activeSeconds / total * 100.0;
            result.LossRate = 100.0 - result.ActiveRate;

            return result;
        }

        private static string Classify(string type)
        {
            var value = type ?? string.Empty;

            if (value.IndexOf("Sort", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Sorting active";

            if (value.IndexOf("Wait", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Waiting for input";

            if (value.IndexOf("Stop", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("Interrupt", StringComparison.OrdinalIgnoreCase) >= 0)
                return "System stop";

            return "Waiting for input";
        }
    }
}
