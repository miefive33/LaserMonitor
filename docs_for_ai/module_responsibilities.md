# Module Responsibilities

このドキュメントは
「どこに何を書くか迷わない」ための絶対ルール

---

# 1. Models（データ定義専用）

## 対象

- LogEvent
- OperationInterval
- SheetInfo
- OrderInfo
- Machine
- DailySummary
- SummaryResult
- LossData
- ErrorData
- TimeEfficiencyResult
- TargetMachines

## ルール

✔ プロパティのみ  
✔ DTO  

❌ ロジック禁止  
❌ DB禁止  

---

# 2. Parsers

## LogParser

入力：
- raw log

出力：
- List<LogEvent>

責務：
- 日付変換
- イベント分類

---

# 3. Analyzers（最重要）

## 共通ルール

✔ Model → Model  
✔ 純粋ロジック  

❌ UI禁止  
❌ DB禁止  

---

## 分析レイヤ構造（超重要）

OperationAnalyzer
↓
ScheduleSplitter
↓
機能別Analyzer

---

## Analyzer一覧（最新版）

### ■ OperationAnalyzer
ログ → 稼働区間

---

### ■ ScheduleSplitter
日跨ぎ・単位分割

---

### ■ LossAnalyzer
ロス構造分析

---

### ■ ErrorAnalyzer
エラー原因分析

---

### ■ TimeEfficiencyAnalyzer
稼働効率

---

### ■ BottleneckAnalyzer
改善優先順位

---

### ■ MachineAnalyzer（NEW）
加工機単位の分析

👉 CUT / WAIT / INTERRUPT を分類

---

### ■ SorterAnalyzer（NEW）
仕分け機分析

👉 SORT動作の時間分析

---

### ■ SystemAnalyzer（NEW）
システム全体分析

👉 機械以外の停止要因

---

### ■ SheetAnalyzer
シート単位分析

---

---

# 4. Builders

## KpiBuilder

入力：
- Analyzer結果

出力：
- UI表示用データ

❌ 再計算禁止

---

# 5. Services

## SqliteService

✔ DB操作のみ  

---

## DashboardService（NEW）

✔ Analyzer呼び出し統括  
✔ GUIへのデータ供給

❌ ロジック実装しない

👉 “オーケストレーター”

---

# 6. GUI

## View

- HeaderView
- KpiPanelView
- TimelineView
- BottomPanelView

---

## ViewModel

- MainViewModel

---

## ルール

✔ 表示のみ  
✔ コマンド発火  

❌ 計算禁止  

---

# 7. CLI

テスト専用

---

# 8. 最重要ルール

Analyzer = 頭脳  
Builder = 翻訳  
Service = 指揮  
GUI = 表示  

👉 役割を混ぜた瞬間に崩壊する