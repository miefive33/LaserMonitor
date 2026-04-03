using System;
using System.Collections.Generic;
using System.Linq;
using Laser.Core.Models;
using Laser.Core.Analyzers;

namespace Laser.Core.Builders
{
    public class DailyReportBuilder
    {
        private readonly TimeEfficiencyAnalyzer _efficiencyAnalyzer;

        public DailyReportBuilder()
        {
            _efficiencyAnalyzer = new TimeEfficiencyAnalyzer();
        }

        /// <summary>
        /// 日次サマリーを生成
        /// </summary>
        public DailySummary Build(DateTime date, List<OperationInterval> intervals)
        {
            if (intervals == null || intervals.Count == 0)
            {
                return new DailySummary { Date = date };
            }

            // 日付フィルタ（重要）
            var targetIntervals = intervals
                .Where(i => i.Start.Date == date.Date)
                .ToList();

            var efficiency = _efficiencyAnalyzer.Analyze(targetIntervals);

            var summary = new DailySummary
            {
                Date = date,
                RunningTime = efficiency.RunningTime,
                ScheduleActiveTime = efficiency.ScheduleActiveTime,
                SetupTime = efficiency.SetupTime,
                IdleTime = efficiency.IdleTime,
                ErrorTime = efficiency.ErrorTime
            };

            return summary;
        }
    }
}