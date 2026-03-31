# Analyzer Responsibility Map（拡張版）

このドキュメントは
「どのAnalyzerが何をするか」を完全に定義する

---

# 0. 全体フロー

LogEvent[]
 ↓
OperationAnalyzer
 ↓
OperationInterval[]
 ↓
LossAnalyzer
ErrorAnalyzer
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

# 2. LossAnalyzer

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

# 3. ErrorAnalyzer（NEW）

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

# 4. BottleneckAnalyzer（重要アップデート）

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

## 出力イメージ

1位：Waiting（Impact 1600）  
2位：Setup（Impact 120）  

---

## ポイント

- ロス分析とは異なり「優先順位」を出す
- 時間だけで判断しない

---

# 5. WeeklyAnalyzer

（変更なし）

---

# 6. Analyzer間の関係

OperationAnalyzer
 ├─ LossAnalyzer
 ├─ ErrorAnalyzer
 ├─ BottleneckAnalyzer
 └─ WeeklyAnalyzer

---

## ルール

- Analyzer同士で直接呼び出さない
- 循環依存禁止

---

# 7. KpiBuilderとの関係

Analyzer → Builder → GUI

---

# 8. 設計思想（重要）

この構造は：

「分解 → 優先順位 → 原因特定」

---

# 9. 結論

- LossAnalyzer → 状況把握
- BottleneckAnalyzer → 意思決定
- ErrorAnalyzer → 原因解決

👉 この3つで改善サイクルが完成する