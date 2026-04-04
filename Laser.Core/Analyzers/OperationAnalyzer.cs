using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class OperationAnalyzer
    {
        public List<OperationInterval> Analyze(List<LogEvent> events)
        {
            var result = new List<OperationInterval>();
            if (events == null || events.Count == 0)
                return result;

            var ordered = events
                .Where(e => e != null)
                .OrderBy(e => e.Timestamp)
                .ToList();

            if (ordered.Count == 0)
                return result;

            DateTime? currentStart = null;

            foreach (var logEvent in ordered)
            {
                var message = logEvent.Message ?? string.Empty;

                if (Contains(message, "Start Scheduling"))
                {
                    if (currentStart.HasValue && logEvent.Timestamp > currentStart.Value)
                    {
                        result.Add(CreateScheduleInterval(currentStart.Value, logEvent.Timestamp));
                    }

                    currentStart = logEvent.Timestamp;
                    continue;
                }

                if (currentStart.HasValue &&
                    (Contains(message, "Scheduling stopped by operator") || Contains(message, "Scheduling interrupted")))
                {
                    if (logEvent.Timestamp > currentStart.Value)
                    {
                        result.Add(CreateScheduleInterval(currentStart.Value, logEvent.Timestamp));
                    }

                    currentStart = null;
                }
            }

            if (currentStart.HasValue)
            {
                var last = ordered.Last().Timestamp;
                if (last > currentStart.Value)
                {
                    result.Add(CreateScheduleInterval(currentStart.Value, last));
                }
            }

            return result;
        }

        private static OperationInterval CreateScheduleInterval(DateTime start, DateTime end)
        {
            return new OperationInterval
            {
                Start = start,
                End = end,
                OperationType = OperationType.ScheduleActive,
                Type = OperationType.ScheduleActive.ToString()
            };
        }

        private static bool Contains(string value, string keyword)
        {
            return value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}