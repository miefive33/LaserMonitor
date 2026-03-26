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

            var ordered = events.OrderBy(e => e.Timestamp).ToList();

            var state = new MachineState();

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                var current = ordered[i];
                var next = ordered[i + 1];

                // 状態更新（イベントを食わせる）
                state.Update(current);

                // 現在状態取得（優先順位付き）
                var type = state.GetCurrentOperation();

                result.Add(new OperationInterval
                {
                    Start = current.Timestamp,
                    End = next.Timestamp,
                    Type = type.ToString()
                });
            }

            return result;
        }
    }

    /// <summary>
    /// 状態管理クラス（将来拡張の中心）
    /// </summary>
    public class MachineState
    {
        private Dictionary<OperationType, bool> flags = new Dictionary<OperationType, bool>()
        {
            { OperationType.Cutting, false },
            { OperationType.Sorting, false },
            { OperationType.Setup, false },
            { OperationType.Error, false }
        };

        // 優先順位（上にあるほど強い）
        private readonly List<OperationType> priority = new List<OperationType>
        {
            OperationType.Error,
            OperationType.Cutting,
            OperationType.Sorting,
            OperationType.Setup,
            OperationType.Idle
        };

        public void Update(LogEvent e)
        {
            var msg = e.Message ?? "";

            // =========================
            // Cutting
            // =========================
            UpdateState(msg,
                "Cutting started",
                "Cutting completed",
                OperationType.Cutting);

            // =========================
            // Sorting（今回追加）
            // =========================
            UpdateState(msg,
                "Sorting started",
                "Sorting completed",
                OperationType.Sorting);

            // =========================
            // Setup / Load
            // =========================
            if (e.IsLoad || e.IsSetup)
                flags[OperationType.Setup] = true;

            if (msg.Contains("Load/Unload Completed") ||
                msg.Contains("Unload/Load Completed"))
                flags[OperationType.Setup] = false;

            // =========================
            // Error
            // =========================
            if (e.IsError)
                flags[OperationType.Error] = true;

            if (msg.Contains("Error cleared"))
                flags[OperationType.Error] = false;
        }

        /// <summary>
        /// Start/End型イベントを共通処理
        /// </summary>
        private void UpdateState(string msg, string startKey, string endKey, OperationType type)
        {
            if (msg.Contains(startKey))
                flags[type] = true;

            if (msg.Contains(endKey))
                flags[type] = false;
        }

        /// <summary>
        /// 現在の状態を優先順位で返す
        /// </summary>
        public OperationType GetCurrentOperation()
        {
            foreach (var type in priority)
            {
                if (type == OperationType.Idle)
                    continue;

                if (flags.ContainsKey(type) && flags[type])
                    return type;
            }

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