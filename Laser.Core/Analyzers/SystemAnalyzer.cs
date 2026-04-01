using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class SystemAnalyzer
    {
        public SummaryResult Analyze(List<OperationInterval> intervals)
        {
            var result = new SummaryResult();
            var source = intervals ?? new List<OperationInterval>();
            double total = source.Sum(i => Math.Max(0, i.Duration.TotalSeconds));
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

            var activeSeconds = result.Breakdown.ContainsKey("SETUP") ? result.Breakdown["SETUP"] : 0;
            result.ActiveRate = activeSeconds / total * 100.0;
            result.LossRate = 100.0 - result.ActiveRate;

            return result;
        }

        private static string Classify(string type)
        {
            var value = type ?? string.Empty;

            if (value.IndexOf("Setup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("Load", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("Unload", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("Stock", StringComparison.OrdinalIgnoreCase) >= 0)
                return "SETUP";

            if (value.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("Alarm", StringComparison.OrdinalIgnoreCase) >= 0)
                return "ERROR";

            if (value.IndexOf("Interrupt", StringComparison.OrdinalIgnoreCase) >= 0)
                return "INTERRUPT";

            return "IDLE";
        }
    }
}
