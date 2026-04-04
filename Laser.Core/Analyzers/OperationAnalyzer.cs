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

            var ordered = events
                .Where(e => e != null)
                .OrderBy(e => e.Timestamp)
                .ToList();

            if (ordered.Count < 2)
                return result;

            var tracker = new TransitionStateTracker();
            DateTime timelineStart = ordered[0].Timestamp;

            foreach (var logEvent in ordered)
            {
                if (logEvent.Timestamp < timelineStart)
                    continue;

                var before = tracker.GetCurrentOperationType();
                var scheduleBefore = tracker.IsScheduleActive;

                tracker.Apply(logEvent);

                var after = tracker.GetCurrentOperationType();
                var scheduleAfter = tracker.IsScheduleActive;

                if (before != after || scheduleBefore != scheduleAfter)
                {
                    AddIntervalIfValid(result, timelineStart, logEvent.Timestamp, before);
                    AddScheduleIntervalIfValid(result, timelineStart, logEvent.Timestamp, scheduleBefore);
                    timelineStart = logEvent.Timestamp;
                }
            }

            var endTime = ordered[ordered.Count - 1].Timestamp;
            AddIntervalIfValid(result, timelineStart, endTime, tracker.GetCurrentOperationType());
            AddScheduleIntervalIfValid(result, timelineStart, endTime, tracker.IsScheduleActive);

            return result;
        }

        private static void AddIntervalIfValid(List<OperationInterval> result, DateTime start, DateTime end, OperationType operationType)
        {
            if (end <= start)
                return;

            result.Add(new OperationInterval
            {
                Start = start,
                End = end,
                OperationType = operationType,
                Type = operationType.ToString()
            });
        }

        private static void AddScheduleIntervalIfValid(List<OperationInterval> result, DateTime start, DateTime end, bool isScheduleActive)
        {
            if (!isScheduleActive || end <= start)
                return;

            result.Add(new OperationInterval
            {
                Start = start,
                End = end,
                OperationType = OperationType.ScheduleActive,
                Type = OperationType.ScheduleActive.ToString()
            });
        }

        private enum TransitionEvent
        {
            None,
            RunningStarted,
            RunningCompleted,
            SetupStarted,
            SetupCompleted,
            ErrorStarted,
            ErrorCleared,
            ScheduleStarted,
            ScheduleStopped,
            WaitingUpstreamStarted,
            WaitingUpstreamEnded,
            WaitingDownstreamStarted,
            WaitingDownstreamEnded,
            InterruptStarted,
            InterruptEnded
        }

        private sealed class TransitionStateTracker
        {
            private bool _isError;
            private bool _isRunning;
            private bool _isSetup;
            private bool _isWaitingUpstream;
            private bool _isWaitingDownstream;
            private bool _isInterrupt;

            public bool IsScheduleActive { get; private set; } = true;

            public void Apply(LogEvent logEvent)
            {
                foreach (var transitionEvent in ParseEvents(logEvent))
                {
                    switch (transitionEvent)
                    {
                        case TransitionEvent.RunningStarted:
                            _isRunning = true;
                            _isWaitingUpstream = false;
                            _isWaitingDownstream = false;
                            _isInterrupt = false;
                            break;

                        case TransitionEvent.RunningCompleted:
                            _isRunning = false;
                            break;

                        case TransitionEvent.SetupStarted:
                            _isSetup = true;
                            _isWaitingUpstream = false;
                            _isWaitingDownstream = false;
                            _isInterrupt = false;
                            break;

                        case TransitionEvent.SetupCompleted:
                            _isSetup = false;
                            break;

                        case TransitionEvent.ErrorStarted:
                            _isError = true;
                            break;

                        case TransitionEvent.ErrorCleared:
                            _isError = false;
                            break;

                        case TransitionEvent.ScheduleStarted:
                            IsScheduleActive = true;
                            break;

                        case TransitionEvent.ScheduleStopped:
                            IsScheduleActive = false;
                            break;

                        case TransitionEvent.WaitingUpstreamStarted:
                            _isWaitingUpstream = true;
                            _isWaitingDownstream = false;
                            _isInterrupt = false;
                            break;

                        case TransitionEvent.WaitingUpstreamEnded:
                            _isWaitingUpstream = false;
                            break;

                        case TransitionEvent.WaitingDownstreamStarted:
                            _isWaitingDownstream = true;
                            _isWaitingUpstream = false;
                            _isInterrupt = false;
                            break;

                        case TransitionEvent.WaitingDownstreamEnded:
                            _isWaitingDownstream = false;
                            break;

                        case TransitionEvent.InterruptStarted:
                            _isInterrupt = true;
                            _isWaitingUpstream = false;
                            _isWaitingDownstream = false;
                            break;

                        case TransitionEvent.InterruptEnded:
                            _isInterrupt = false;
                            break;
                    }
                }
            }

            public OperationType GetCurrentOperationType()
            {
                if (_isError)
                    return OperationType.Error;

                if (_isRunning)
                    return OperationType.Running;

                if (_isSetup)
                    return OperationType.Setup;

                if (_isWaitingUpstream)
                    return OperationType.WaitingUpstream;

                if (_isWaitingDownstream)
                    return OperationType.WaitingDownstream;

                if (_isInterrupt)
                    return OperationType.SystemInterrupt;

                return OperationType.Unknown;
            }

            private static IEnumerable<TransitionEvent> ParseEvents(LogEvent logEvent)
            {
                if (logEvent == null)
                    yield break;

                var message = logEvent.Message ?? string.Empty;
                if (string.IsNullOrWhiteSpace(message))
                    yield break;

                if (Contains(message, "Start Scheduling"))
                    yield return TransitionEvent.ScheduleStarted;

                if (Contains(message, "Scheduling stopped"))
                    yield return TransitionEvent.ScheduleStopped;

                if (Contains(message, "Cutting started") || Contains(message, "Sorting started"))
                    yield return TransitionEvent.RunningStarted;

                if (Contains(message, "Cutting completed") || Contains(message, "Sorting completed"))
                    yield return TransitionEvent.RunningCompleted;

                if (logEvent.IsLoad || logEvent.IsSetup || Contains(message, "Load/Unload started") || Contains(message, "Unload/Load started"))
                    yield return TransitionEvent.SetupStarted;

                if (Contains(message, "Load/Unload Completed") || Contains(message, "Unload/Load Completed"))
                    yield return TransitionEvent.SetupCompleted;

                if (logEvent.IsError && !Contains(message, "Error cleared"))
                    yield return TransitionEvent.ErrorStarted;

                if (Contains(message, "Error cleared"))
                    yield return TransitionEvent.ErrorCleared;

                if ((Contains(message, "2PC") || Contains(message, "Upstream")) && Contains(message, "start"))
                    yield return TransitionEvent.WaitingUpstreamStarted;

                if ((Contains(message, "2PC") || Contains(message, "Upstream")) && (Contains(message, "end") || Contains(message, "complete") || Contains(message, "clear") || Contains(message, "resume")))
                    yield return TransitionEvent.WaitingUpstreamEnded;

                if ((Contains(message, "3PC") || Contains(message, "Downstream")) && Contains(message, "start"))
                    yield return TransitionEvent.WaitingDownstreamStarted;

                if ((Contains(message, "3PC") || Contains(message, "Downstream")) && (Contains(message, "end") || Contains(message, "complete") || Contains(message, "clear") || Contains(message, "resume")))
                    yield return TransitionEvent.WaitingDownstreamEnded;

                if (Contains(message, "Interrupt") && Contains(message, "start"))
                    yield return TransitionEvent.InterruptStarted;

                if (Contains(message, "Interrupt") && (Contains(message, "end") || Contains(message, "clear") || Contains(message, "resume")))
                    yield return TransitionEvent.InterruptEnded;
            }

            private static bool Contains(string value, string keyword)
            {
                return value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }
    }
}