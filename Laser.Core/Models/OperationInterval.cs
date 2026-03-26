using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Core.Models
{
    public class OperationInterval
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public TimeSpan Duration => End - Start;

        public string Type { get; set; } // Cutting / Setup / Idle など

        public override string ToString()
        {
            return $"{Type} | {Start:HH:mm:ss} - {End:HH:mm:ss} ({Duration.TotalMinutes:F1} min)";
        }

    }
}
