using Laser.Core.Models;
using System;
using System.Collections.Generic;
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
                return result;

            var ordered = intervals.OrderBy(i => i.Start).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var current = ordered[i];

                if (!IsEndType(current.Type))
                    continue;

                for (int j = i + 1; j < ordered.Count; j++)
                {
                    var candidate = ordered[j];

                    if (IsLoadType(candidate.Type))
                        break;

                    var seconds = candidate.Duration.TotalSeconds;
                    if (seconds < NoiseThresholdSeconds)
                        continue;

                    var lossType = ClassifyLoss(candidate.Type);
                    AddAggregate(result, lossType, seconds);
                }
            }

            return result;
        }

        private static bool IsEndType(string type)
        {
            return IsType(type, "End");
        }

        private static bool IsLoadType(string type)
        {
            return IsType(type, "Load");
        }

        private static bool IsType(string value, string keyword)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ClassifyLoss(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "Waiting";

            if (type.IndexOf("Setup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                type.IndexOf("Load", StringComparison.OrdinalIgnoreCase) >= 0)
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
