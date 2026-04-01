using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class MachineAnalyzer
    {
        public SummaryResult Analyze(List<OperationInterval> intervals)
        {
            return AnalyzeByRule(intervals, Classify);
        }

        private static string Classify(OperationInterval interval)
        {
            var type = interval?.Type ?? string.Empty;

            if (type.IndexOf("Cut", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Cutting";

            if (type.IndexOf("Wait", StringComparison.OrdinalIgnoreCase) >= 0)
                return "2PC / 3PC";

            if (type.IndexOf("Interrupt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                type.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Schedule stop / Error impact";

            return "2PC / 3PC";
        }

        private static SummaryResult AnalyzeByRule(List<OperationInterval> intervals, Func<OperationInterval, string> classifier)
        {
            var result = new SummaryResult();
            var source = intervals ?? new List<OperationInterval>();

            double total = source.Sum(i => Math.Max(0, i.Duration.TotalSeconds));
            if (total <= 0)
                return result;

            foreach (var interval in source)
            {
                var seconds = Math.Max(0, interval.Duration.TotalSeconds);
                var key = classifier(interval);

                if (!result.Breakdown.ContainsKey(key))
                    result.Breakdown[key] = 0;

                result.Breakdown[key] += seconds;
            }

            var activeSeconds = result.Breakdown.ContainsKey("Cutting") ? result.Breakdown["Cutting"] : 0;
            result.ActiveRate = activeSeconds / total * 100.0;
            result.LossRate = 100.0 - result.ActiveRate;

            return result;
        }
    }
}
