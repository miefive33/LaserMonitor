using Laser.Core.Models;
using System;
using System.Collections.Generic;

namespace Laser.Core.Analyzers
{
    public class TimeEfficiencyAnalyzer
    {
        public TimeEfficiencyResult Analyze(List<OperationInterval> intervals)
        {
            var result = new TimeEfficiencyResult();

            if (intervals == null)
                return result;


            foreach (var i in intervals)
            {
                if (i.IsScheduleActive)
                {
                    // ★ CHANGED
                    result.ScheduleActiveTime += i.Duration;
                    continue;
                }

                switch (i.OperationType)
                {
                    case OperationType.Running:
                        result.RunningTime += i.Duration;
                        break;

                    case OperationType.Setup:
                        result.SetupTime += i.Duration;
                        break;

                    case OperationType.Error:
                        result.ErrorTime += i.Duration;
                        break;

                    default:
                        result.IdleTime += i.Duration;
                        break;
                }
            }

            return result;
        }
    }
}
