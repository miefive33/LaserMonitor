using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Core.Analyzers
{
    public class TimeEfficiencyAnalyzer
    {
        public TimeEfficiencyResult Analyze(List<OperationInterval> intervals)
        {
            var result = new TimeEfficiencyResult();

            foreach (var i in intervals)
            {
                result.TotalTime += i.Duration;

                switch (i.Type)
                {
                    case "Cutting":
                        result.CuttingTime += i.Duration;
                        break;

                    case "Setup":
                        result.SetupTime += i.Duration;
                        break;

                    case "Error":
                        result.ErrorTime += i.Duration;
                        break;

                    case "Idle":
                        result.IdleTime += i.Duration;
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
