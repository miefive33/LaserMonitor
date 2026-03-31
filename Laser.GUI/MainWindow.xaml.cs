using Laser.Core.Parsers;
using Laser.Core.Services;
using Laser.GUI.ViewModels;
using Laser.GUI.Views;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Laser.GUI
{
    public partial class MainWindow : Window
    {
        private SqliteService _sqliteService;
        private LogParser _logParser;

        public MainWindow()
        {
            InitializeComponent();

            // 🔥 日付変更イベント
            HeaderView.DateChanged += OnDateChanged;

            DataContext = new MainViewModel();

            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.db");

            _sqliteService = new SqliteService(dbPath);
            _logParser = new LogParser();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FindName("HeaderView") is HeaderView header)
            {
                header.OnUpdateRequested += HandleUpdate;
                header.OnReloadRequested += HandleReload;
            }
            LoadLog(DateTime.Today);
        }

        // =========================
        // 🟢 差分更新
        // =========================
        private void HandleUpdate()
        {
            try
            {
                string path = GetLogFilePath();

                if (!File.Exists(path))
                {
                    MessageBox.Show("ログファイルが存在しません");
                    return;
                }

                var logs = _logParser.Load(path);

                var latest = _sqliteService.GetLatestTimestamp();

                _sqliteService.InsertLogEvents(logs, latest);

                LoadLog(DateTime.Today);


            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラー: {ex.Message}");
            }
        }

        // =========================
        // 🔴 フル再取込
        // =========================
        private void HandleReload()
        {
            try
            {
                var result = MessageBox.Show(
                    "全データを削除して再取込します。よろしいですか？",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                string path = GetLogFilePath();

                if (!File.Exists(path))
                {
                    MessageBox.Show("ログファイルが存在しません");
                    return;
                }

                var logs = _logParser.Load(path);

                _sqliteService.ClearAll();
                _sqliteService.InsertLogEvents(logs, null);

                LoadLog(DateTime.Today);

                MessageBox.Show("全再取込が完了しました 👍");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラー: {ex.Message}");
            }
        }

        // =========================
        // 📂 ファイルパス取得
        // =========================
        private string GetLogFilePath()
        {
            if (this.FindName("HeaderView") is HeaderView header)
            {
                var textBox = header.FindName("FilePathTextBox") as System.Windows.Controls.TextBox;

                if (textBox != null)
                    return textBox.Text;
            }

            return "";
        }

        // =========================
        // 📅 日付変更
        // =========================
        private void OnDateChanged(DateTime date)
        {
            LoadLog(date);
        }

        // =========================
        // 🔥 日付フィルタ処理
        // =========================
        private void LoadLog(DateTime date)
        {
            if (DataContext is not MainViewModel vm)
                return;

            var allEvents = _sqliteService.GetAllLogEvents();

            var filtered = allEvents
                .Where(e => e.Timestamp.Date == date.Date)
                .ToList();

            vm.Events = filtered;
            vm.UpdateKpi(date);
        }
    }
}