# Module Responsibilities

## LogParser.cs
ログ解析専用
→ LogEvent生成

---

## OperationAnalyzer.cs
状態判定の中核

---

## LossAnalyzer.cs
ロス分類

---

## BottleneckAnalyzer.cs
ボトルネック検出

---

## SheetAnalyzer.cs
シート単位解析

---

## TimeEfficiencyAnalyzer.cs
稼働率・効率算出

---

## DailyReportBuilder.cs
日次サマリー生成

---

## SqliteService.cs
DB操作専用

責務：
- 保存
- 読み込み
- 重複チェック

NG：
- 計算
- UI

---

## MainViewModel.cs
GUIとの橋渡し

---

## BottomPanelView.xaml
週次サマリー表示

---

## TimelineView.xaml
時系列表示