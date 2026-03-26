using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Core.Analyzers
{
    public class OperationAnalyzer
    {
        public List<OperationInterval> Analyze(List<LogEvent> events)
        {
            var result = new List<OperationInterval>();

            if (events == null || events.Count < 2)
                return result;

            // 時系列順に並び替え（念のため）
            var ordered = events.OrderBy(e => e.Timestamp).ToList();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                var current = ordered[i];
                var next = ordered[i + 1];

                var type = DetectType(current);

                result.Add(new OperationInterval
                {
                    Start = current.Timestamp,
                    End = next.Timestamp,
                    Type = type.ToString()
                });
            }

            return result;
        }

        /// <summary>
        /// ログイベントから状態を判定する
        /// ※ここが超重要（ログ仕様に応じて拡張）
        /// </summary>
        private OperationType DetectType(LogEvent e)
        {
            if (e.IsCutStart) return OperationType.Cutting;
            if (e.IsSetup) return OperationType.Setup;
            if (e.IsLoad) return OperationType.Load;
            if (e.IsError) return OperationType.Error;

            return OperationType.Idle;
        }
    }

    /// <summary>
    /// 稼働状態の種類
    /// </summary>
    public enum OperationType
    {
        Cutting,
        Setup,
        Load,
        Sorting,
        Error,
        Idle
    }
}