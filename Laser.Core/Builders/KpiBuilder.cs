using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Laser.Core.Builders
{
    public class KpiBuilder
    {
        public List<string> BuildLossSummary(LossData lossData)
        {
            var result = new List<string>();

            if (lossData == null)
                return result;

            foreach (var entry in lossData.TotalTime.OrderByDescending(kv => kv.Value))
            {
                var count = lossData.Count.ContainsKey(entry.Key) ? lossData.Count[entry.Key] : 0;
                var minutes = TimeSpan.FromSeconds(entry.Value).TotalMinutes;
                result.Add($"{entry.Key} : {minutes:0.#} min ({count} times)");
            }

            Debug.WriteLine($"[KpiBuilder] loss output: {string.Join(" | ", result)}");
            return result;
        }

        public List<string> BuildKpiSummary(DailySummary weeklyKpi, LossData lossData)
        {
            var result = new List<string>();

            if (weeklyKpi == null)
                return result;

            result.Add($"OperationRate : {weeklyKpi.OperationRate:0.0}%");
            result.Add($"RunningTime : {weeklyKpi.RunningTime:hh\\:mm\\:ss}");
            result.Add($"LossTime : {weeklyKpi.LossTime:hh\\:mm\\:ss}");
            result.Add($"ScheduleActiveTime : {weeklyKpi.ScheduleActiveTime:hh\\:mm\\:ss}");

            if (lossData != null)
                result.AddRange(BuildLossSummary(lossData));

            return result;
        }

        public List<string> BuildErrorSummary(ErrorData errorData)
        {
            var result = new List<string>();

            if (errorData == null || errorData.Items == null)
                return result;

            foreach (var item in errorData.Items.OrderByDescending(i => i.TotalTime))
            {
                var totalMinutes = TimeSpan.FromSeconds(item.TotalTime).TotalMinutes;
                var avgMinutes = TimeSpan.FromSeconds(item.AvgDuration).TotalMinutes;

                var codePart = string.IsNullOrWhiteSpace(item.ErrorCode) ? string.Empty : $" code {item.ErrorCode}";
                result.Add($"{item.Type}{codePart} : {item.Count} times / {totalMinutes:0.#} min (avg {avgMinutes:0.#} min)");
            }

            Debug.WriteLine($"[KpiBuilder] error output: {string.Join(" | ", result)}");
            return result;
        }

        public List<string> BuildBottleneckSummary(BottleneckData bottleneckData, int top = 3)
        {
            var result = new List<string>();

            if (bottleneckData?.Items == null)
                return result;

            var rank = 1;
            foreach (var item in bottleneckData.Items.Take(top))
            {
                var minutes = TimeSpan.FromSeconds(item.TotalTime).TotalMinutes;
                result.Add($"{rank}位 {item.Category} ({item.Perspective}) {minutes:0.#}分 / {item.Count}回");
                rank++;
            }

            return result;
        }
    }
}