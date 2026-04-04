using System;

namespace Laser.Core.Models
{
    public enum OperationType
    {
        ScheduleActive,
        Cutting,
        Sorting,
        SystemAction,
        Running,
        Setup,
        WaitingUpstream,
        WaitingDownstream,
        SystemInterrupt,
        Error,
        Unknown
    }

    public class OperationInterval
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public TimeSpan Duration => End - Start;

        public string Type { get; set; }

        public OperationType OperationType { get; set; }

        public bool IsScheduleActive => OperationType == OperationType.ScheduleActive;

        public bool IsRunning => OperationType == OperationType.Running || OperationType == OperationType.Cutting;

        public override string ToString()
        {
            return $"{Type} | {Start:HH:mm:ss} - {End:HH:mm:ss} ({Duration.TotalMinutes:F1} min)";
        }
    }
}