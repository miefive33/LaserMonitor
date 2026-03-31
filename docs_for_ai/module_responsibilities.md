# Module Responsibilities（詳細版）

このドキュメントは
「どこに何を書くか迷わない」ための絶対ルールである

---

# 1. Models（データ定義専用）

## 目的
すべてのデータ構造を定義する

## 対象クラス

- LogEvent
- OperationInterval
- SheetInfo
- OrderInfo
- DailySummary
- KpiData
- LossData
- ErrorData
- BottleneckData
- TimeEfficiencyResult
- WeeklyKpi

## ルール

✔ プロパティのみ持つ  
✔ DTOとして扱う  
✔ シリアライズ可能  

❌ 計算処理を書かない  
❌ DBアクセスしない  
❌ 他クラス呼び出し禁止  

---

# 2. Parsers（入力変換層）

## 目的
ログファイルを「構造化データ」に変換する

## クラス

### LogParser

## 入力
- raw log text

## 出力
- List<LogEvent>

## 責務

- 日付フォーマット変換（Day/Month/Year対応）
- ログ種別判定
  - Load
  - Cutting
  - End
- 行ごとのイベント生成

## ルール

✔ 1行 → 1イベント  
✔ 状態はenum化  

❌ 時間計算しない  
❌ シート概念を持たない  
❌ KPIを作らない  

---

# 3. Analyzers（ロジック層 / 最重要）

## 目的
すべてのビジネスロジックをここに集約する

---

## 共通ルール

✔ 入力は必ずModel  
✔ 出力もModel  
✔ 純粋関数に近づける  

❌ UIを触らない  
❌ DBを触らない  
❌ Console出力しない  

---

## 3.1 OperationAnalyzer

（変更なし）

---

## 3.2 ScheduleSplitter（NEW）

### 目的
スケジュール単位でデータを分割する

### 入力
- List<OperationInterval>

### 出力
- List<OperationInterval>

### 処理

- 日跨ぎ分割
- シート単位分割

---

## 3.3 LossAnalyzer

### 目的
ロス時間の構造を可視化する

### 入力
- List<OperationInterval>

### 出力
- LossData

### 処理

- End → 次のLoadまでをロスとする
- 短時間ノイズ除去
- ロス分類（Setup / Waiting / Idle / Error）
- 合計時間・回数集計

### 役割

👉 「何が起きているか」を把握する

---

## 3.4 ErrorAnalyzer

### 目的
エラーの原因を分析する

### 入力
- List<OperationInterval>

### 出力
- ErrorData

### 処理

- Error区間抽出
- エラー分類（Machine / Material / Operator / Unknown）
- 発生回数・時間・平均・最大を算出
- 再発検出

### 役割

👉 「なぜ止まったか」を特定する

---

## 3.5 BottleneckAnalyzer

### 目的
改善優先順位を決定する

### 入力
- LossData

### 出力
- List<BottleneckData>

### 処理

- Impactスコア算出

Impact = TotalTime × Count

- 降順ソート
- 上位抽出

### 役割

👉 「どれを直すべきか」を決定する

---

## 3.6 SheetAnalyzer

（変更なし）

---

## 3.7 TimeEfficiencyAnalyzer（NEW）

### 目的
時間効率を分析する

### 入力
- List<OperationInterval>

### 出力
- TimeEfficiencyResult

### 処理

- Cutting / Setup / Idle の比率算出
- 稼働率算出

### 役割

👉 「どれだけ効率的か」を評価する

---

## 3.8 WeeklyAnalyzer

（変更なし）

---

# 4. Builders（UI変換層）

## 目的
Analyzerの結果をUI用に整形

## クラス

### KpiBuilder

---

## 入力

- OperationInterval
- LossData
- ErrorData
- BottleneckData
- WeeklyKpi
- TimeEfficiencyResult

---

## 出力

- KpiData（UI専用）

---

## 処理

- 円グラフデータ作成
- 棒グラフデータ作成
- 表示用テキスト生成

---

## ルール

✔ 表示形式に変換するだけ  

❌ 再計算しない  
❌ ロジックを書かない  

---

# 5. Services（インフラ層）

（変更なし）

---

# 6. GUI（表示層）

（変更なし）

---

# 7. CLI（開発用）

（変更なし）

---

# 8. 最重要ルールまとめ

1. ロジックはAnalyzerのみ  
2. 表示はGUIのみ  
3. DBはServiceのみ  
4. 変換はBuilderのみ  

👉 この分離を崩すとプロジェクトは壊れる

---

# 9. 分析の全体構造（NEW）

このプロジェクトの分析は以下の4軸で構成される

- LossAnalyzer → 状況把握
- ErrorAnalyzer → 原因分析
- BottleneckAnalyzer → 優先順位
- TimeEfficiencyAnalyzer → 効率評価

👉 この4つで改善ループが完成する

---

## 🤖 AI RESPONSIBILITY RULES

### GUI

CAN:

* display data
* trigger events

CANNOT:

* calculate
* analyze
* access DB

---

### Analyzer

CAN:

* calculate KPI
* analyze data

CANNOT:

* access UI
* format UI data

---

### Builder

CAN:

* format for UI

CANNOT:

* recalculate KPI

---

### SqliteService

CAN:

* SELECT
* INSERT

CANNOT:

* analyze data