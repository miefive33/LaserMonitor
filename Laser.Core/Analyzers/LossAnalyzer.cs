using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class LossAnalyzer
    {
        private const double NoiseThresholdSeconds = 5.0;

        public LossData Analyze(List<OperationInterval> intervals)
        {
            var result = new LossData();

            if (intervals == null || intervals.Count == 0)
            {
                Debug.WriteLine("[LossAnalyzer] intervals is null or empty.");
                return result;
            }

            var ordered = intervals
                .Where(i => i != null && i.End > i.Start)
                .OrderBy(i => i.Start)
                .ToList();

            // ★ CHANGED: ScheduleActive内の非Runningのみを対象
            var scheduleActive = ordered.Where(i => i.IsScheduleActive).ToList();
            var candidateLosses = ordered
                .Where(i => !i.IsScheduleActive && !i.IsRunning)
                .ToList();

            foreach (var interval in candidateLosses)
            {
                var inScheduleSeconds = GetOverlappedSeconds(interval, scheduleActive);
                if (inScheduleSeconds < NoiseThresholdSeconds)
                    continue;

                var lossType = ClassifyLoss(interval.OperationType);
                AddAggregate(result, lossType, inScheduleSeconds);
            }

            var totals = result.TotalTime
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value:0.##}s");

            Debug.WriteLine($"[LossAnalyzer] total items: {result.Count.Values.Sum()}");
            Debug.WriteLine($"[LossAnalyzer] values per type: {string.Join(", ", totals)}");

            return result;
        }

        // ★ CHANGED: overlap秒数で厳密にScheduleActive内を抽出
        private static double GetOverlappedSeconds(OperationInterval target, List<OperationInterval> schedules)
        {
            double seconds = 0;

            foreach (var schedule in schedules)
            {
                var start = target.Start > schedule.Start ? target.Start : schedule.Start;
                var end = target.End < schedule.End ? target.End : schedule.End;

                if (end > start)
                    seconds += (end - start).TotalSeconds;
            }

            return seconds;
        }

        private static string ClassifyLoss(OperationType type)
        {
            switch (type)
            {
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

        private static void AddAggregate(LossData data, string type, double seconds)
        {
            if (!data.TotalTime.ContainsKey(type))
                data.TotalTime[type] = 0;

            data.TotalTime[type] += seconds;

            if (!data.Count.ContainsKey(type))
                data.Count[type] = 0;

            data.Count[type] += 1;

            switch (type)
            {
                case "Setup":
                    data.SetupTime += seconds;
                    break;
                case "WaitingUpstream":
                    data.WaitingUpstreamTime += seconds;
                    break;
                case "WaitingDownstream":
                    data.WaitingDownstreamTime += seconds;
                    break;
                case "SystemInterrupt":
                    data.SystemInterruptTime += seconds;
                    break;
                case "Error":
                    data.ErrorTime += seconds;
                    break;
                default:
                    data.UnknownTime += seconds;
                    break;
            }
        }
    }
}