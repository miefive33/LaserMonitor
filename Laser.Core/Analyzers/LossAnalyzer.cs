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
            var source = intervals ?? new List<OperationInterval>();

            var denominator = source
                .Where(i => i.IsScheduleActive)
                .Sum(i => Math.Max(0, i.Duration.TotalSeconds));

            if (denominator <= 0)
                return result;

            var cutSeconds = SumMergedSeconds(source.Where(i => i.OperationType == OperationType.Cutting));
            var sortSeconds = SumMergedSeconds(source.Where(i => i.OperationType == OperationType.Sorting));
            var systemSeconds = SumMergedSeconds(source.Where(i => i.OperationType == OperationType.SystemAction));

            Add(result, "MachineNonCut", Math.Max(0, denominator - cutSeconds), 1);
            Add(result, "SorterNonSort", Math.Max(0, denominator - sortSeconds), 1);
            Add(result, "SystemNonAction", Math.Max(0, denominator - systemSeconds), 1);

            AddByType(result, source, "PalletChange", "System", "PalletChange");
            AddByType(result, source, "ThirdPalletChange", "System", "ThirdPalletChange");
            AddByType(result, source, "LoaderReadyWait", "System", "LoaderReadyWait");
            AddByType(result, source, "WarehouseReadyWait", "System", "WarehouseReadyWait");
            AddByType(result, source, "DrawerMoveWait", "System", "DrawerMoveWait");
            AddByType(result, source, "SortDelayUnloadPriority", "Sorter", "SortDelayUnloadPriority");

            result.WaitingUpstreamTime = GetTotal(result, "LoaderReadyWait") + GetTotal(result, "WarehouseReadyWait");
            result.WaitingDownstreamTime = GetTotal(result, "SortDelayUnloadPriority");
            result.SetupTime = GetTotal(result, "PalletChange");
            result.UnknownTime = GetTotal(result, "DrawerMoveWait") + GetTotal(result, "SystemNonAction");

            return result;
        }

        private static void AddByType(LossData result, List<OperationInterval> source, string key, string perspective, string type)
        {
            var matched = source
                .Where(i => i.OperationType == OperationType.SystemAction && string.Equals(i.Type, type, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matched.Count == 0)
                return;

            Add(result, key, SumMergedSeconds(matched), matched.Count);
            Add(result, $"{perspective}:{key}", SumMergedSeconds(matched), matched.Count);
        }

        private static double GetTotal(LossData data, string key)
        {
            return data.TotalTime.ContainsKey(key) ? data.TotalTime[key] : 0;
        }

        private static void Add(LossData data, string key, double seconds, int count)
        {
            data.TotalTime[key] = seconds;
            data.Count[key] = count;
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