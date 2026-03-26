using Laser.Core.Analyzers;
using Laser.Core.Models;
using System.Collections.Generic;

namespace Laser.Core.Services
{
    public class DashboardService
    {
        private readonly ScheduleSplitter _splitter = new ScheduleSplitter();
        private readonly OperationAnalyzer _operationAnalyzer = new OperationAnalyzer();

        public List<OperationInterval> Analyze(List<LogEvent> events)
        {
            var result = new List<OperationInterval>();

            var schedules = _splitter.Split(events);

            foreach (var schedule in schedules)
            {
                var intervals = _operationAnalyzer.Analyze(schedule.Events);
                result.AddRange(intervals);
            }

            return result;
        }
    }
}
