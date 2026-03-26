using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Core.Models
{
    public class SheetInfo
    {
        public DateTime LoadTime { get; set; }
        public DateTime? StartCutTime { get; set; }
        public DateTime? EndCutTime { get; set; }

        public TimeSpan? SetupTime =>
            StartCutTime.HasValue ? StartCutTime - LoadTime : null;

        public TimeSpan? CuttingTime =>
            (StartCutTime.HasValue && EndCutTime.HasValue)
                ? EndCutTime - StartCutTime
                : null;
        public int Index { get; set; }
    }
}
