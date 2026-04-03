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
            if (interval == null)
                return "Unknown";

            switch (interval.OperationType)
            {
                case OperationType.Running:
                    return "Running";
                case OperationType.Setup:
                    return "Setup";
                case OperationType.WaitingUpstream:
                    return "WaitingUpstream";
                case OperationType.WaitingDownstream:
                    return "WaitingDownstream";
                case OperationType.SystemInterrupt:
                    return "SystemInterrupt";
                case OperationType.Error:
                    return "Error";
                default:
                    return "Unknown";
            }
        }

        private static SummaryResult AnalyzeByRule(List<OperationInterval> intervals, Func<OperationInterval, string> classifier)
        {
            var result = new SummaryResult();
            var source = intervals ?? new List<OperationInterval>();

            // ★ CHANGED: 分母はScheduleActiveのみ
            var scheduleActiveSeconds = source
                .Where(i => i.IsScheduleActive)
                .Sum(i => Math.Max(0, i.Duration.TotalSeconds));

            if (scheduleActiveSeconds <= 0)
                return result;

            foreach (var interval in source.Where(i => !i.IsScheduleActive))
            {
                var seconds = Math.Max(0, interval.Duration.TotalSeconds);
                if (seconds <= 0)
                    continue;

                var key = classifier(interval);

                if (!result.Breakdown.ContainsKey(key))
                    result.Breakdown[key] = 0;

                result.Breakdown[key] += seconds;
            }

            var runningSeconds = source
                .Where(i => i.IsRunning)
                .Sum(i => Math.Max(0, i.Duration.TotalSeconds));

            result.ActiveRate = runningSeconds / scheduleActiveSeconds * 100.0;
            result.LossRate = 100.0 - result.ActiveRate;

            return result;
        }
    }
}