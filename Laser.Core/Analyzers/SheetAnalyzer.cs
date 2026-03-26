using Laser.Core.Models;
using System;
using System.Collections.Generic;

namespace Laser.Core.Analyzers
{
    public class SheetAnalyzer
    {
        public List<SheetInfo> Analyze(List<LogEvent> events)
        {
            var sheets = new List<SheetInfo>();

            DateTime? lastLoad = null;
            DateTime? lastStart = null;

            foreach (var e in events)
            {
                if (e.IsLoad)
                {
                    lastLoad = e.Timestamp;
                }
                else if (e.IsCutStart)
                {
                    lastStart = e.Timestamp;
                }
                else if (e.IsCutEnd)
                {
                    if (lastLoad.HasValue && lastStart.HasValue)
                    {
                        sheets.Add(new SheetInfo
                        {
                            LoadTime = lastLoad.Value,
                            StartCutTime = lastStart,
                            EndCutTime = e.Timestamp
                        });
                    }

                    // リセット（次のシートへ）
                    lastStart = null;
                }
            }

            return sheets;
        }
    }
}