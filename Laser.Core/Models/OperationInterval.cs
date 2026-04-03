using System;


namespace Laser.Core.Models
{
    public enum OperationType
    {
        ScheduleActive,
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

        // ★ ADDED
        public OperationType OperationType { get; set; }

        // ★ ADDED
        public bool IsScheduleActive => OperationType == OperationType.ScheduleActive;

        // ★ ADDED
        public bool IsRunning => OperationType == OperationType.Running;

        public override string ToString()
        {
            return $"{Type} | {Start:HH:mm:ss} - {End:HH:mm:ss} ({Duration.TotalMinutes:F1} min)";
        }

    }
}
