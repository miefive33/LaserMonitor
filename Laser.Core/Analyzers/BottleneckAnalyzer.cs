using Laser.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class BottleneckAnalyzer
    {
        public BottleneckData Analyze(LossData lossData, ErrorData errorData)
        {
            var result = new BottleneckData();

            if (lossData != null)
            {
                foreach (var entry in lossData.TotalTime)
                {
                    if (entry.Value <= 0)
                        continue;

                    var count = lossData.Count.ContainsKey(entry.Key) ? lossData.Count[entry.Key] : 1;
                    var score = (entry.Value / 60.0) + (count * 3.0);

                    result.Items.Add(new BottleneckItem
                    {
                        Category = entry.Key,
                        Perspective = InferPerspective(entry.Key),
                        Count = count,
                        TotalTime = entry.Value,
                        Severity = 0,
                        Score = score
                    });
                }
            }

            if (errorData?.Items != null)
            {
                foreach (var error in errorData.Items)
                {
                    var score = (error.TotalTime / 60.0) + (error.Count * 5.0) + error.SeverityScore;

                    result.Items.Add(new BottleneckItem
                    {
                        Category = error.Type,
                        Perspective = "Error",
                        Count = error.Count,
                        TotalTime = error.TotalTime,
                        Severity = error.SeverityScore,
                        Score = score
                    });
                }
            }

            result.Items = result.Items
                .GroupBy(i => new { i.Category, i.Perspective })
                .Select(g => new BottleneckItem
                {
                    Category = g.Key.Category,
                    Perspective = g.Key.Perspective,
                    Count = g.Sum(x => x.Count),
                    TotalTime = g.Sum(x => x.TotalTime),
                    Severity = g.Sum(x => x.Severity),
                    Score = g.Sum(x => x.Score)
                })
                .OrderByDescending(i => i.Score)
                .ThenByDescending(i => i.TotalTime)
                .ToList();

            return result;
        }

        private static string InferPerspective(string key)
        {
            if (key.StartsWith("Machine")) return "Machine";
            if (key.StartsWith("Sorter")) return "Sorter";
            if (key.StartsWith("System") || key.Contains("Wait") || key.Contains("Pallet")) return "System";
            return "Cross";
        }
    }
}