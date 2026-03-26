using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class Schedule
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<LogEvent> Events { get; set; } = new List<LogEvent>();
    }

    public class ScheduleSplitter
    {
        public List<Schedule> Split(List<LogEvent> events)
        {
            var schedules = new List<Schedule>();

            if (events == null || events.Count == 0)
                return schedules;

            var ordered = events.OrderBy(e => e.Timestamp).ToList();

            Schedule current = null;

            foreach (var e in ordered)
            {
                var msg = e.Message ?? "";

                // =========================
                // Start Scheduling
                // =========================
                if (msg.Contains("Start Scheduling"))
                {
                    // 前スケジュールを閉じる
                    if (current != null)
                    {
                        current.EndTime = e.Timestamp;
                        schedules.Add(current);
                    }

                    // 新スケジュール開始
                    current = new Schedule
                    {
                        StartTime = e.Timestamp
                    };

                    continue;
                }

                // =========================
                // 手動停止
                // =========================
                if (msg.Contains("Scheduling stopped by operator"))
                {
                    if (current != null)
                    {
                        current.Events.Add(e);
                        current.EndTime = e.Timestamp;
                        schedules.Add(current);
                        current = null;
                    }

                    continue;
                }

                // =========================
                // 通常イベント
                // =========================
                if (current != null)
                {
                    current.Events.Add(e);
                }
            }

            // =========================
            // 最後のスケジュール補完
            // =========================
            if (current != null && current.Events.Count > 0)
            {
                current.EndTime = current.Events.Last().Timestamp;
                schedules.Add(current);
            }

            // =========================
            // ノイズ除去（短すぎるスケジュール）
            // =========================
            schedules = schedules
                .Where(s => (s.EndTime - s.StartTime).TotalSeconds > 5)
                .ToList();

            return schedules;
        }
    }
}
