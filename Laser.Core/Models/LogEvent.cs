using System;

namespace Laser.Core.Models
{
    public class LogEvent
    {
        // ===== 元データ =====
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string EventType { get; set; }
        public string RawText { get; set; }
        public string SheetName { get; set; } = "";

        // ===== 時間分解（追加：DBはそのまま） =====

        public DateTime Date => Timestamp.Date;          // 2026-03-24
        public TimeSpan Time => Timestamp.TimeOfDay;     // 14:23:10
        public int Hour => Timestamp.Hour;               // 14（時間帯分析用）

        // ===== 状態判定 =====

        public bool IsCutStart => Message?.Contains("Cutting started") == true;
        public bool IsCutEnd => Message?.Contains("Cutting completed") == true;

        public bool IsLoad => Message?.Contains("Load") == true;
        public bool IsUnload => Message?.Contains("Unload") == true;

        public bool IsSetup => Message?.Contains("Setup") == true
                            || Message?.Contains("Prepare") == true;

        public bool IsError => Message?.Contains("Error") == true
                            || Message?.Contains("Alarm") == true;

        public bool IsIdle =>
            !(IsCutStart || IsCutEnd || IsLoad || IsUnload || IsSetup || IsError);

        // ===== 状態分類（追加：UI/分析で超便利） =====

        public string State
        {
            get
            {
                if (IsCutStart || IsCutEnd) return "Cutting";
                if (IsLoad || IsUnload) return "Handling";
                if (IsSetup) return "Setup";
                if (IsError) return "Error";
                return "Idle";
            }
        }

        // ===== シート情報抽出 =====

        public string SheetId => ExtractSheetId();

        private string ExtractSheetId()
        {
            if (string.IsNullOrEmpty(Message))
                return null;

            var parts = Message.Split(' ');

            foreach (var p in parts)
            {
                if (p.StartsWith("A") || p.StartsWith("B"))
                    return p;
            }

            return null;
        }

        // ===== デバッグ・表示 =====

        public override string ToString()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss} | {State} | {Message}";
        }
    }
}