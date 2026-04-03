using System;


namespace Laser.Core.Models
{
    public class TimeEfficiencyResult
    {
        public TimeSpan RunningTime { get; set; }
        public TimeSpan ScheduleActiveTime { get; set; }
        public TimeSpan SetupTime { get; set; }
        public TimeSpan IdleTime { get; set; }
        public TimeSpan ErrorTime { get; set; }

        public TimeSpan CuttingTime
        {
            get => RunningTime;
            set => RunningTime = value;
        }

        // ★ CHANGED
        public TimeSpan TotalTime
        {
            get => ScheduleActiveTime;
            set => ScheduleActiveTime = value;
        }

        // ★ ADDED
        public TimeSpan LossTime => ScheduleActiveTime - RunningTime;


        public double OperationRate =>
            ScheduleActiveTime.TotalSeconds == 0
            ? 0
            : RunningTime.TotalSeconds / ScheduleActiveTime.TotalSeconds * 100;
    }
}
