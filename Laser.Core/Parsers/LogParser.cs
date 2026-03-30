using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Laser.Core.Parsers
{
    public class LogParser
    {
        public List<LogEvent> Load(string path)
        {
            var list = new List<LogEvent>();

            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(new[] { " - " }, 2, StringSplitOptions.None);

                if (parts.Length < 2)
                    continue;

                if (DateTime.TryParseExact(
                    parts[0].Trim(),
                    "dd/MM/yy HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime dt))
                {
                    var message = parts[1].Trim();

                    string sheetName = "";

                    if (message.Contains("Starting Load sheet"))
                    {
                        var index = message.IndexOf("sheet");

                        if (index >= 0)
                        {
                            sheetName = message.Substring(index + 5).Trim();
                        }
                    }

                    list.Add(new LogEvent
                    {
                        Timestamp = dt,
                        Message = message,
                        SheetName = sheetName
                    });
                }
            }

            return list;
        }
    }
}
