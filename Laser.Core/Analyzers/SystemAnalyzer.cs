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

            var denominator = source
                .Where(i => i.IsScheduleActive)
                .Sum(i => Math.Max(0, i.Duration.TotalSeconds));

            if (denominator <= 0)
                return result;

            var systemSeconds = MergeAndSumSeconds(source
                .Where(i => i.OperationType == OperationType.SystemAction)
                .ToList());

            var idleSeconds = Math.Max(0, denominator - systemSeconds);

            result.Breakdown["SystemActive"] = systemSeconds;
            result.Breakdown["SystemIdle"] = idleSeconds;
            result.ActiveRate = systemSeconds / denominator * 100.0;
            result.LossRate = 100.0 - result.ActiveRate;

            return result;
        }

        private static double MergeAndSumSeconds(List<OperationInterval> intervals)
        {
            if (intervals == null || intervals.Count == 0)
                return 0;

            var ordered = intervals.OrderBy(i => i.Start).ToList();
            var merged = new List<OperationInterval> { ordered[0] };

            foreach (var interval in ordered.Skip(1))
            {
                var last = merged[merged.Count - 1];
                if (interval.Start <= last.End)
                {
                    if (interval.End > last.End)
                    {
                        last.End = interval.End;
                    }
                }
                else
                {
                    merged.Add(new OperationInterval { Start = interval.Start, End = interval.End });
                }
            }

            return merged.Sum(i => Math.Max(0, i.Duration.TotalSeconds));
        }
    }
}