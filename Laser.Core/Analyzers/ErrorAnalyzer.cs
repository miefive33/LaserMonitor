using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Laser.Core.Analyzers
{
    public class ErrorAnalyzer
    {
        private static readonly Regex ErrorCodeRegex = new Regex(@"error code:\s*([\-0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ErrorData Analyze(List<LogEvent> events, List<OperationInterval> dailySchedules)
        {
            var result = new ErrorData();
            var sourceEvents = (events ?? new List<LogEvent>()).OrderBy(e => e.Timestamp).ToList();
            var schedules = (dailySchedules ?? new List<OperationInterval>()).OrderBy(i => i.Start).ToList();

            if (sourceEvents.Count == 0 || schedules.Count == 0)
                return result;

            var explicitErrors = sourceEvents
                .Where(e => IsInsideSchedules(e.Timestamp, schedules) && IsExplicitAbnormal(e.Message))
                .ToList();

            var grouped = explicitErrors
                .Select(e => new
                {
                    Category = Classify(e.Message),
                    Event = e,
                    Interruption = EstimateInterruptionSeconds(e.Timestamp, sourceEvents, schedules)
                })
                .GroupBy(x => x.Category)
                .OrderByDescending(g => g.Sum(x => x.Interruption));

            foreach (var group in grouped)
            {
                var interruptions = group.Select(x => x.Interruption).ToList();
                var first = group.First();
                var totalInterruption = interruptions.Sum();
                var count = interruptions.Count;

                result.Items.Add(new ErrorItem
                {
                    Type = group.Key,
                    Count = count,
                    TotalTime = totalInterruption,
                    AvgDuration = count == 0 ? 0 : totalInterruption / count,
                    MaxDuration = interruptions.Count == 0 ? 0 : interruptions.Max(),
                    Timestamp = first.Event.Timestamp,
                    Message = first.Event.Message,
                    ErrorCode = ExtractErrorCode(first.Event.Message),
                    OperationContext = InferOperationContext(first.Event.Timestamp, sourceEvents),
                    InterruptionTime = totalInterruption,
                    SeverityScore = totalInterruption + (count * 60.0)
                });
            }

            return result;
        }

        private static bool IsInsideSchedules(DateTime timestamp, List<OperationInterval> schedules)
        {
            return schedules.Any(s => timestamp >= s.Start && timestamp <= s.End);
        }

        private static bool IsExplicitAbnormal(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            return message.IndexOf("FAILED", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("Scheduling interrupted", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("error code", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("Alarm", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Classify(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "UnknownAbnormal";

            if (message.IndexOf("Scheduling interrupted", StringComparison.OrdinalIgnoreCase) >= 0)
                return "SchedulingInterrupted";

            if (message.IndexOf("Pickup", StringComparison.OrdinalIgnoreCase) >= 0 &&
                message.IndexOf("FAILED", StringComparison.OrdinalIgnoreCase) >= 0)
                return "PickupFailed";

            if (message.IndexOf("FAILED", StringComparison.OrdinalIgnoreCase) >= 0)
                return "CommandFailed";

            return "AbnormalEvent";
        }

        private static string ExtractErrorCode(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            var m = ErrorCodeRegex.Match(message);
            return m.Success ? m.Groups[1].Value : string.Empty;
        }

        private static string InferOperationContext(DateTime at, List<LogEvent> events)
        {
            var contextEvent = events
                .Where(e => e.Timestamp <= at)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault(e => (e.Message ?? string.Empty).IndexOf("started", StringComparison.OrdinalIgnoreCase) >= 0);

            return contextEvent?.Message ?? string.Empty;
        }

        private static double EstimateInterruptionSeconds(DateTime at, List<LogEvent> events, List<OperationInterval> schedules)
        {
            var nextResume = events.FirstOrDefault(e => e.Timestamp > at &&
                (e.Message ?? string.Empty).IndexOf("Start Scheduling", StringComparison.OrdinalIgnoreCase) >= 0);

            var scheduleContaining = schedules.FirstOrDefault(s => at >= s.Start && at <= s.End);
            var resumeAt = nextResume != null ? nextResume.Timestamp : scheduleContaining?.End ?? at;

            if (resumeAt <= at)
                return 0;

            return (resumeAt - at).TotalSeconds;
        }
    }
}