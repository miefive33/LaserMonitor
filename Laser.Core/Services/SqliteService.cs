using Laser.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Laser.Core.Services
{
    public class SqliteService
    {
        private readonly string _connectionString;

        public SqliteService(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
            Initialize();
        }

        private void Initialize()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                CREATE TABLE IF NOT EXISTS LogEvents (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT,
                    Message TEXT,
                    UNIQUE(Timestamp, Message)
                );
                ";
                command.ExecuteNonQuery();
            }
        }

        // =========================
        // 🔵 最新Timestamp取得
        // =========================
        public DateTime? GetLatestTimestamp()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT MAX(Timestamp) FROM LogEvents";

                var result = cmd.ExecuteScalar();

                if (result == DBNull.Value || result == null)
                    return null;

                return DateTime.Parse(result.ToString());
            }
        }

        // =========================
        // 🟢 差分＋通常INSERT
        // =========================
        public void InsertLogEvents(List<LogEvent> events, DateTime? lastTimestamp = null)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var e in events)
                    {
                        // 🔥 差分チェック
                        if (lastTimestamp != null && e.Timestamp <= lastTimestamp)
                            continue;

                        var command = connection.CreateCommand();
                        command.CommandText =
                        @"
                        INSERT OR IGNORE INTO LogEvents (Timestamp, Message)
                        VALUES ($timestamp, $message);
                        ";

                        command.Parameters.AddWithValue("$timestamp", e.Timestamp.ToString("o"));
                        command.Parameters.AddWithValue("$message", e.Message);

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        // =========================
        // 🔴 全削除（フル再取込用）
        // =========================
        public void ClearAll()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM LogEvents";
                command.ExecuteNonQuery();
            }
        }
    }
}