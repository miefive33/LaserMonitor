using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class ScheduleSplitter
    {
        public List<OperationInterval> Split(List<OperationInterval> scheduleIntervals, DateTime targetDate)
        {
            var result = new List<OperationInterval>();
            if (scheduleIntervals == null || scheduleIntervals.Count == 0)
                return result;

            var dayStart = targetDate.Date;
            var dayEnd = dayStart.AddDays(1);

            foreach (var interval in scheduleIntervals.Where(i => i != null && i.IsScheduleActive && i.End > i.Start))
            {
                var clippedStart = interval.Start > dayStart ? interval.Start : dayStart;
                var clippedEnd = interval.End < dayEnd ? interval.End : dayEnd;

                if (clippedEnd <= clippedStart)
                    continue;

                result.Add(new OperationInterval
                {
                    Start = clippedStart,
                    End = clippedEnd,
                    OperationType = OperationType.ScheduleActive,
                    Type = OperationType.ScheduleActive.ToString()
                });
            }

            return result.OrderBy(i => i.Start).ToList();
        }
    }
}