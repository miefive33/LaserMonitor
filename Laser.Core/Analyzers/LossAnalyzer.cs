using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class LossAnalyzer
    {
        public LossData Analyze(List<OperationInterval> intervals)
        {
            var result = new LossData();
            if (intervals == null || intervals.Count == 0)
                return result;

            var denominator = intervals
                .Where(i => i.IsScheduleActive)
                .Sum(i => Math.Max(0, i.Duration.TotalSeconds));

            if (denominator <= 0)
                return result;

            var cutSeconds = SumMergedSeconds(intervals.Where(i => i.OperationType == OperationType.Cutting));
            var sortSeconds = SumMergedSeconds(intervals.Where(i => i.OperationType == OperationType.Sorting));
            var systemSeconds = SumMergedSeconds(intervals.Where(i => i.OperationType == OperationType.SystemAction));

            Add(result, "MachineNonCut", Math.Max(0, denominator - cutSeconds));
            Add(result, "SorterIdle", Math.Max(0, denominator - sortSeconds));
            Add(result, "SystemIdle", Math.Max(0, denominator - systemSeconds));

            return result;
        }

        private static void Add(LossData data, string key, double seconds)
        {
            data.TotalTime[key] = seconds;
            data.Count[key] = seconds > 0 ? 1 : 0;

            if (key == "MachineNonCut")
                data.WaitingUpstreamTime = seconds;
            else if (key == "SorterIdle")
                data.WaitingDownstreamTime = seconds;
            else if (key == "SystemIdle")
                data.UnknownTime = seconds;
        }

        private static double SumMergedSeconds(IEnumerable<OperationInterval> intervals)
        {
            var ordered = (intervals ?? Enumerable.Empty<OperationInterval>())
                .Where(i => i != null && i.End > i.Start)
                .OrderBy(i => i.Start)
                .ToList();

            if (ordered.Count == 0)
                return 0;

            var total = 0.0;
            var currentStart = ordered[0].Start;
            var currentEnd = ordered[0].End;

            foreach (var interval in ordered.Skip(1))
            {
                if (interval.Start <= currentEnd)
                {
                    if (interval.End > currentEnd)
                        currentEnd = interval.End;
                }
                else
                {
                    total += (currentEnd - currentStart).TotalSeconds;
                    currentStart = interval.Start;
                    currentEnd = interval.End;
                }
            }

            total += (currentEnd - currentStart).TotalSeconds;
            return Math.Max(0, total);
        }
    }
}