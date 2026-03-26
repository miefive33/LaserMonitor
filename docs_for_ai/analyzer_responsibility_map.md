# Analyzer Responsibility Map（詳細版）

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

---

## 入力

- List<LogEvent>

---

## 出力

- List<OperationInterval>

---

## 処理詳細

### 状態遷移

Load → Cutting → End

---

### アルゴリズム

1. Load検出 → start
2. Cutting開始 → 稼働開始
3. End検出 → 終了
4. duration算出

---

## 重要ポイント

- ログの順序保証
- 欠損イベント対応
- 同一Sheet識別

---

## NG

- KPI計算
- ロス分析

---

# 2. LossAnalyzer

## 役割
「なぜ止まっているか」を分解する

---

## 入力

- List<OperationInterval>

---

## 出力

- LossData

---

## 処理

- 稼働していない区間抽出
- 前後関係から原因推定

---

## 分類例

- Setup
- Idle
- Waiting
- Error

---

## ポイント

- 連続停止の統合
- 短時間ノイズ除去

---

# 3. BottleneckAnalyzer

## 役割
「どこが一番遅いか」を特定する

---

## 入力

- List<OperationInterval>

---

## 出力

- List<BottleneckData>

---

## 処理

- 停止時間でソート
- 原因別に集約
- 上位ランキング作成

---

## 出力イメージ

1位：Setup（120分）  
2位：Waiting（80分）

---

# 4. WeeklyAnalyzer

## 役割
「時間軸の流れ」を作る

---

## 入力

- List<OperationInterval>

---

## 出力

- WeeklyKpi

---

## 処理

### 日別分解

- 日ごとにIntervalを分割

---

### 稼働率計算

稼働率 = Cutting時間 / 総時間

---

### 出力

- 日付配列
- 稼働率配列

---

# 5. Analyzer間の関係

OperationAnalyzer
 ├─ LossAnalyzer
 ├─ BottleneckAnalyzer
 └─ WeeklyAnalyzer

---

## ルール

- Analyzer同士で直接呼び出さない
- 循環依存禁止

---

# 6. KpiBuilderとの関係

Analyzer → Builder → GUI

---

## 理由

- Analyzerはロジック専用
- Builderは表示変換専用

---

# 7. データ責務の境界

| 項目 | 担当 |
|------|------|
| 時間計算 | OperationAnalyzer |
| ロス分類 | LossAnalyzer |
| ランキング | BottleneckAnalyzer |
| 時系列 | WeeklyAnalyzer |
| 表示整形 | KpiBuilder |

---

# 8. 設計思想（重要）

この構造は：

「分解 → 分析 → 可視化」

---

# 9. よくある間違い

- Analyzerでグラフ作る
- GUIで計算する
- Builderで再計算する

---

# 10. 結論

Analyzerは

「意味を作る層」

Builderは

「見せ方を作る層」