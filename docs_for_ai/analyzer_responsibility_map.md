# Analyzer Responsibility Map（拡張版）

このドキュメントは
「どのAnalyzerが何をするか」を完全に定義する

---

# 0. 全体フロー（最新版）

LogEvent[]
 ↓
OperationAnalyzer
 ↓
OperationInterval[]
 ↓
ScheduleSplitter
 ↓
分割済OperationInterval[]
 ↓
LossAnalyzer
ErrorAnalyzer
TimeEfficiencyAnalyzer
BottleneckAnalyzer
WeeklyAnalyzer
 ↓
各結果
 ↓
KpiBuilder

---

# 1. OperationAnalyzer（最上流）

## 役割
ログ → 稼働区間へ変換

## 入力
- List<LogEvent>

## 出力
- List<OperationInterval>

## 処理
- Load → Cutting → End を1サイクルとして認識
- duration算出

## NG
- KPI計算
- ロス分析

---

# 2. ScheduleSplitter（NEW）

## 役割
データを分析単位に分割する

## 入力
- List<OperationInterval>

## 出力
- List<OperationInterval>

## 処理

### ① 日跨ぎ分割
- 1つの区間が複数日に跨る場合は分割

### ② シート単位分割
- 必要に応じてシート単位で分割

## ポイント

- すべてのAnalyzerの前処理
- データ粒度を揃える役割

---

# 3. LossAnalyzer

## 役割
「どこで止まっているか」を可視化する

## 入力
- List<OperationInterval>

## 出力
- LossData

## 処理

### ① ロス区間抽出
- End → 次のLoadまでをロスとする

### ② ノイズ除去
- 短時間（例：5秒以下）は無視

### ③ ロス分類
- Setup
- Waiting
- Idle
- Error

### ④ 集計
- 総時間
- 発生回数

## ポイント
- ロスは「全体構造」を示す

---

# 4. ErrorAnalyzer

## 役割
エラーの原因を深掘りする

## 入力
- List<OperationInterval>

## 出力
- ErrorData

## 処理

### ① エラー抽出
- Type == Error の区間

### ② 分類
- MachineError
- MaterialError
- OperatorError
- Unknown

### ③ 指標
- 発生回数
- 総時間
- 平均復旧時間
- 最大停止時間

### ④ 再発分析
- 同一エラーの繰り返し検出

## ポイント
- 「なぜ止まったか」を明確にする

---

# 5. TimeEfficiencyAnalyzer（NEW）

## 役割
稼働効率を評価する

## 入力
- List<OperationInterval>

## 出力
- TimeEfficiencyResult

## 処理

### ① 時間分類
- Cutting
- Setup
- Idle

### ② 指標算出
- 稼働率
- 各時間割合

## ポイント
- 「どれだけ効率的か」を測る

---

# 6. BottleneckAnalyzer（重要）

## 役割
「どれを改善すべきか」を決定する

## 入力
- LossData

## 出力
- List<BottleneckData>

---

## 🔥 コアロジック（Impactスコア）

### 基本式

Impact = TotalTime × Count

---

### 拡張（将来）

Impact = TotalTime × Count × RecurrenceRate × ImprovementFactor

---

## 処理

1. LossDataから各カテゴリを取得
2. Impactスコア計算
3. 降順ソート
4. 上位N件抽出

---

## ポイント

- ロス分析とは異なり「優先順位」を出す
- 時間だけで判断しない

---

# 7. WeeklyAnalyzer

## 役割
時系列トレンドを生成する

## 入力
- List<OperationInterval>

## 出力
- WeeklyKpi

## 処理

- 日単位に分割
- 稼働率算出
- 時系列配列生成

---

# 8. Analyzer間の関係（更新）

OperationAnalyzer
 ↓
ScheduleSplitter
 ↓
 ├─ LossAnalyzer
 ├─ ErrorAnalyzer
 ├─ TimeEfficiencyAnalyzer
 ├─ BottleneckAnalyzer
 └─ WeeklyAnalyzer

---

## ルール

- Analyzer同士で直接呼び出さない
- 循環依存禁止
- 必ずOperationIntervalを基準にする

---

# 9. KpiBuilderとの関係

Analyzer → Builder → GUI

---

# 10. 設計思想（重要）

この構造は：

「分解 → 可視化 → 優先順位 → 原因特定 → 効率評価」

---

# 11. 結論（アップデート）

- LossAnalyzer → 状況把握
- ErrorAnalyzer → 原因分析
- BottleneckAnalyzer → 優先順位
- TimeEfficiencyAnalyzer → 効率評価

👉 この4つで改善サイクルが完成する