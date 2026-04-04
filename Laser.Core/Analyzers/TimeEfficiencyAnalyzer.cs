using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class TimeEfficiencyAnalyzer
    {
        public TimeEfficiencyResult Analyze(List<OperationInterval> intervals)
        {
            var result = new TimeEfficiencyResult();
            if (intervals == null)
                return result;

            var denominatorSeconds = intervals
                .Where(i => i.IsScheduleActive)
                .Sum(i => Math.Max(0, i.Duration.TotalSeconds));

            result.ScheduleActiveTime = TimeSpan.FromSeconds(denominatorSeconds);

            if (denominatorSeconds <= 0)
                return result;

            var cutSeconds = SumMergedSeconds(intervals.Where(i => i.OperationType == OperationType.Cutting));
            var systemSeconds = SumMergedSeconds(intervals.Where(i => i.OperationType == OperationType.SystemAction));

            result.RunningTime = TimeSpan.FromSeconds(cutSeconds);
            result.SetupTime = TimeSpan.FromSeconds(systemSeconds);
            result.IdleTime = TimeSpan.FromSeconds(Math.Max(0, denominatorSeconds - cutSeconds));
            result.ErrorTime = TimeSpan.Zero;

            return result;
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