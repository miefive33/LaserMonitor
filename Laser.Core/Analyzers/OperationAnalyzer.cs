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

                if (next.Timestamp <= current.Timestamp)
                    continue;

                state.Update(current);
                var currentType = state.GetCurrentOperation();

                // ★ ADDED: ScheduleActive interval（分母）
                result.Add(new OperationInterval
                {
                    Start = current.Timestamp,
                    End = next.Timestamp,
                    OperationType = OperationType.ScheduleActive,
                    Type = OperationType.ScheduleActive.ToString()
                });

                // ★ CHANGED: detailed interval（Running / Loss）
                result.Add(new OperationInterval
                {
                    Start = current.Timestamp,
                    End = next.Timestamp,
                    OperationType = currentType,
                    Type = currentType.ToString()
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
        private bool _isRunning;
        private bool _isSetup;
        private bool _isError;

        // ★ ADDED: 待ち状態は最後のイベントヒントを保持
        private OperationType _passiveHint = OperationType.Unknown;

        public void Update(LogEvent e)
        {
            var msg = e?.Message ?? string.Empty;

            // ★ CHANGED: Running（加工中）
            if (ContainsAny(msg, "Cutting started", "Sorting started"))
                _isRunning = true;

            if (ContainsAny(msg, "Cutting completed", "Sorting completed"))
                _isRunning = false;

            // Setup
            if (e.IsLoad || e.IsSetup)
                _isSetup = true;

            if (ContainsAny(msg, "Load/Unload Completed", "Unload/Load Completed"))
                _isSetup = false;

            // Error
            if (e.IsError)
                _isError = true;

            if (msg.IndexOf("Error cleared", StringComparison.OrdinalIgnoreCase) >= 0)
                _isError = false;

            // ★ ADDED: active state優先時は待ちヒントを解除
            if (_isError || _isRunning || _isSetup)
            {
                _passiveHint = OperationType.Unknown;
                return;
            }

            // ★ CHANGED: waiting / interrupt hint
            if (msg.IndexOf("Upstream", StringComparison.OrdinalIgnoreCase) >= 0)
                _passiveHint = OperationType.WaitingUpstream;
            else if (msg.IndexOf("Downstream", StringComparison.OrdinalIgnoreCase) >= 0)
                _passiveHint = OperationType.WaitingDownstream;
            else if (ContainsAny(msg, "2PC", "3PC", "Interrupt"))
                _passiveHint = OperationType.SystemInterrupt;
            else if (msg.IndexOf("Wait", StringComparison.OrdinalIgnoreCase) >= 0)
                _passiveHint = OperationType.WaitingDownstream;
            else
                _passiveHint = OperationType.Unknown;
        }

        /// <summary>
        /// 現在の状態を優先順位で返す
        /// </summary>
        public OperationType GetCurrentOperation()
        {
            if (_isError)
                return OperationType.Error;

            if (_isRunning)
                return OperationType.Running;

            if (_isSetup)
                return OperationType.Setup;

            if (_passiveHint != OperationType.Unknown)
                return _passiveHint;

            return OperationType.Unknown;
        }

        // ★ ADDED
        private static bool ContainsAny(string source, params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}