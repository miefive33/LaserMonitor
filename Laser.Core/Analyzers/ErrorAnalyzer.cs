using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class ErrorAnalyzer
    {
        public ErrorData Analyze(List<OperationInterval> intervals)
        {
            var result = new ErrorData();

            if (intervals == null || intervals.Count == 0)
            {
                Debug.WriteLine("[ErrorAnalyzer] intervals is null or empty.");
                return result;
            }

            var errorIntervals = intervals
                .Where(i => IsErrorType(i.Type))
                .ToList();

            Debug.WriteLine($"[ErrorAnalyzer] total items: {errorIntervals.Count}");

            if (errorIntervals.Count == 0)
            {
                Debug.WriteLine("[ErrorAnalyzer] error types detected: none");
                return result;
            }

            var groups = errorIntervals
                .GroupBy(i => ClassifyError(i.Type))
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var durations = group
                    .Select(i => i.Duration.TotalSeconds)
                    .ToList();

                if (durations.Count == 0)
                    continue;

                result.Items.Add(new ErrorItem
                {
                    Type = group.Key,
                    Count = durations.Count,
                    TotalTime = durations.Sum(),
                    AvgDuration = durations.Average(),
                    MaxDuration = durations.Max()
                });
            }

            Debug.WriteLine($"[ErrorAnalyzer] error types detected: {string.Join(", ", result.Items.Select(i => i.Type))}");

            return result;
        }

        private static bool IsErrorType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return false;

            return type.IndexOf("Error", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ClassifyError(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "Unknown";

            if (type.IndexOf("Machine", StringComparison.OrdinalIgnoreCase) >= 0)
                return "MachineError";

            if (type.IndexOf("Material", StringComparison.OrdinalIgnoreCase) >= 0)
                return "MaterialError";

            if (type.IndexOf("Operator", StringComparison.OrdinalIgnoreCase) >= 0)
                return "OperatorError";

            return "Unknown";
        }
    }
}