using System;
using System.Linq;
using Laser.Core.Analyzers;
using Laser.Core.Parsers;



namespace Laser.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Laser Monitor CLI ===");

            // 引数でファイル指定（なければ固定）
            string path;

            if (args.Length > 0)
            {
                path = args[0];
            }
            else
            {
                Console.Write("ログファイルのパスを入力してください: ");
                path = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("パスが入力されていません");
                    return;
                }
            }

            // ① ログ読み込み
            var parser = new LogParser();
            var logs = parser.Load(path);

            Console.WriteLine($"ログ件数: {logs.Count}");

            // ② 加工時間解析
            var analyzer = new OperationAnalyzer();
            var intervals = analyzer.Analyze(logs);

            Console.WriteLine("\n=== Cutting Intervals ===");

            foreach (var i in intervals.Take(10))
            {
                Console.WriteLine(i);
            }

            var sheetAnalyzer = new SheetAnalyzer();
            var sheets = sheetAnalyzer.Analyze(logs);

            Console.WriteLine("\n=== Sheet Analysis ===");

            foreach (var s in sheets.Take(10))
            {
                Console.WriteLine(
                    $"Load: {s.LoadTime:HH:mm:ss} | " +
                    $"Setup: {s.SetupTime?.TotalMinutes:F1} min | " +
                    $"Cut: {s.CuttingTime?.TotalMinutes:F1} min"
                );
            }

            var validSheets = sheets
                .Where(s => s.SetupTime.HasValue && s.CuttingTime.HasValue)
                .ToList();

            var avgSetup = validSheets.Average(s => s.SetupTime.Value.TotalMinutes);
            var avgCut = validSheets.Average(s => s.CuttingTime.Value.TotalMinutes);

            Console.WriteLine("\n=== Efficiency ===");
            Console.WriteLine($"平均段取り時間: {avgSetup:F1} 分");
            Console.WriteLine($"平均加工時間: {avgCut:F1} 分");





            // ③ 合計時間
            var totalMinutes = intervals.Sum(i => i.Duration.TotalMinutes);

            Console.WriteLine($"\n合計加工時間: {totalMinutes:F1} 分");

            Console.WriteLine("\n--- END ---");
            Console.ReadKey();
        }
    }
}