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

            var ordered = intervals.OrderBy(i => i.Start).ToList();
            Debug.WriteLine($"[LossAnalyzer] input intervals: {ordered.Count}");

            foreach (var interval in ordered)
            {
                var seconds = interval.Duration.TotalSeconds;
                if (seconds < NoiseThresholdSeconds)
                    continue;

                var lossType = ClassifyLoss(interval.Type);
                if (lossType == null)
                    continue;

                AddAggregate(result, lossType, seconds);
            }

            var totals = result.TotalTime
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value:0.##}s");

            Debug.WriteLine($"[LossAnalyzer] total items: {result.Count.Values.Sum()}");
            Debug.WriteLine($"[LossAnalyzer] values per type: {string.Join(", ", totals)}");

            return result;
        }

        private static string ClassifyLoss(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "Waiting";

            if (type.IndexOf("Cutting", StringComparison.OrdinalIgnoreCase) >= 0 ||
                type.IndexOf("Sorting", StringComparison.OrdinalIgnoreCase) >= 0)
                return null;

            if (type.IndexOf("Setup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                type.IndexOf("Load", StringComparison.OrdinalIgnoreCase) >= 0 ||
                type.IndexOf("Handling", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Setup";

            if (type.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Error";

            if (type.IndexOf("Idle", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Idle";

            return "Waiting";
        }

        private static void AddAggregate(LossData data, string type, double seconds)
        {
            if (!data.TotalTime.ContainsKey(type))
                data.TotalTime[type] = 0;

            data.TotalTime[type] += seconds;

            if (!data.Count.ContainsKey(type))
                data.Count[type] = 0;

            data.Count[type] += 1;
        }
    }
}