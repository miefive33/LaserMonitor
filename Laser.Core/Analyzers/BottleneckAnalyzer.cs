using Laser.Core.Models;
using System.Collections.Generic;
using System.Linq;



namespace Laser.Core.Analyzers
{
    internal class BottleneckAnalyzer
    {
        // ★ ADDED: LossDataベースでボトルネック順序化
        public List<KeyValuePair<string, double>> RankByLossTime(LossData lossData)
        {
            if (lossData == null)
                return new List<KeyValuePair<string, double>>();

            return lossData.TotalTime
                .OrderByDescending(kv => kv.Value)
                .ToList();
        }
    }
}
