using Laser.Core.Models;
using System;
using System.Collections.Generic;
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

            var order = new[] { "Setup", "Waiting", "Idle", "Error" };

            foreach (var type in order)
            {
                var totalSeconds = lossData.TotalTime.ContainsKey(type)
                    ? lossData.TotalTime[type]
                    : 0;

                var count = lossData.Count.ContainsKey(type)
                    ? lossData.Count[type]
                    : 0;

                var minutes = TimeSpan.FromSeconds(totalSeconds).TotalMinutes;
                result.Add($"{type} : {minutes:0.#} min ({count} times)");
            }

            return result;
        }

        public List<string> BuildErrorSummary(ErrorData errorData)
        {
            var result = new List<string>();

            if (errorData == null || errorData.Items == null)
                return result;

            foreach (var item in errorData.Items.OrderByDescending(i => i.Count))
            {
                var totalMinutes = TimeSpan.FromSeconds(item.TotalTime).TotalMinutes;
                var avgMinutes = TimeSpan.FromSeconds(item.AvgDuration).TotalMinutes;

                result.Add($"{item.Type} : {item.Count} times / {totalMinutes:0.#} min (avg {avgMinutes:0.#} min)");
            }

            return result;
        }
    }
}