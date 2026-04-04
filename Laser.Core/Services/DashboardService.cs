using Laser.Core.Analyzers;
using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Services
{
    public class DashboardService
    {
        private readonly ScheduleSplitter _splitter = new ScheduleSplitter();
        private readonly OperationAnalyzer _operationAnalyzer = new OperationAnalyzer();

        public List<OperationInterval> Analyze(List<LogEvent> events, DateTime targetDate)
        {
            var sourceEvents = events ?? new List<LogEvent>();

            var schedules = _operationAnalyzer.Analyze(sourceEvents);
            var dailySchedules = _splitter.Split(schedules, targetDate);

            var result = new List<OperationInterval>();
            result.AddRange(dailySchedules);

            if (dailySchedules.Count == 0)
                return result;

            result.AddRange(ExtractIntervalsInSchedules(sourceEvents, dailySchedules, "Cutting started", "Cutting completed", OperationType.Cutting, "CUT"));
            result.AddRange(ExtractIntervalsInSchedules(sourceEvents, dailySchedules, "Sorting started", "Sorting completed", OperationType.Sorting, "SORT"));
            result.AddRange(ExtractSystemIntervals(sourceEvents, dailySchedules));

            return result.OrderBy(i => i.Start).ToList();
        }

        public List<OperationInterval> Analyze(List<LogEvent> events)
        {
            return Analyze(events, DateTime.Today);
        }

        private static List<OperationInterval> ExtractSystemIntervals(List<LogEvent> events, List<OperationInterval> schedules)
        {
            var result = new List<OperationInterval>();
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "Load sheet", "Load sheet complete", OperationType.SystemAction, "Load"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "Unload sheet", "Unload sheet complete", OperationType.SystemAction, "Unload"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "Unload/Load started", "Unload/Load Completed", OperationType.SystemAction, "Unload/Load"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "Load/Unload started", "Load/Unload Completed", OperationType.SystemAction, "Load/Unload"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "PlaceProduct started", "PlaceProduct completed", OperationType.SystemAction, "PlaceProduct"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "Pallet change started", "Pallet change completed", OperationType.SystemAction, "PalletChange"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "Third pallet change started", "Third pallet change completed", OperationType.SystemAction, "ThirdPalletChange"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "drawer transfer started", "drawer transfer completed", OperationType.SystemAction, "DrawerTransfer"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "warehouse transfer started", "warehouse transfer completed", OperationType.SystemAction, "WarehouseTransfer"));
            result.AddRange(ExtractIntervalsInSchedules(events, schedules, "MaterialStockerSelect started", "MaterialStockerSelect completed", OperationType.SystemAction, "MaterialStockerSelect"));

            return result;
        }

        private static List<OperationInterval> ExtractIntervalsInSchedules(
            List<LogEvent> events,
            List<OperationInterval> schedules,
            string startToken,
            string endToken,
            OperationType operationType,
            string label)
        {
            var result = new List<OperationInterval>();
            if (events == null || schedules == null || schedules.Count == 0)
                return result;

            var ordered = events
                .Where(e => e != null)
                .OrderBy(e => e.Timestamp)
                .ToList();

            DateTime? currentStart = null;

            foreach (var logEvent in ordered)
            {
                var message = logEvent.Message ?? string.Empty;

                if (currentStart == null && Contains(message, startToken))
                {
                    currentStart = logEvent.Timestamp;
                    continue;
                }

                if (currentStart.HasValue && Contains(message, endToken))
                {
                    var rawStart = currentStart.Value;
                    var rawEnd = logEvent.Timestamp;

                    if (rawEnd > rawStart)
                    {
                        foreach (var clipped in ClipToSchedules(rawStart, rawEnd, schedules, operationType, label))
                        {
                            result.Add(clipped);
                        }
                    }

                    currentStart = null;
                }
            }

            return result;
        }

        private static IEnumerable<OperationInterval> ClipToSchedules(
            DateTime start,
            DateTime end,
            List<OperationInterval> schedules,
            OperationType operationType,
            string label)
        {
            foreach (var schedule in schedules)
            {
                var clippedStart = start > schedule.Start ? start : schedule.Start;
                var clippedEnd = end < schedule.End ? end : schedule.End;

                if (clippedEnd <= clippedStart)
                    continue;

                yield return new OperationInterval
                {
                    Start = clippedStart,
                    End = clippedEnd,
                    OperationType = operationType,
                    Type = label
                };
            }
        }

        private static bool Contains(string value, string token)
        {
            return value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}