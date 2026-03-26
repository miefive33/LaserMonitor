👉 Analyzerの“縄張り争い”防止用（超重要）

```md id="analyzer-map-01"
# Analyzer Responsibility Map

## 1. Purpose

Define clear boundaries between analyzers.

Avoid:
- duplicated logic
- conflicting calculations
- inconsistent results

---

## 2. Analyzer Roles

---

### OperationAnalyzer（中心）

責務：
- LogEvent → OperationInterval
- 状態遷移を決定
- Cutting / Setup / Idle / Stop 判定

出力：
- OperationInterval[]

---

### LossAnalyzer

責務：
- ロス時間を分類
- 待機・エラー・段取りなど

入力：
- OperationInterval

出力：
- Loss集計

---

### BottleneckAnalyzer

責務：
- 最大ロス原因を特定
- 上位ランキング生成

入力：
- LossAnalyzer結果

出力：
- Bottleneckランキング

---

### SheetAnalyzer

責務：
- シート単位の流れ解析
- Load → Cutting → End

入力：
- LogEvent / OperationInterval

出力：
- SheetInfo[]

---

### TimeEfficiencyAnalyzer

責務：
- 稼働率計算
- 有効時間 vs 無効時間

入力：
- OperationInterval

出力：
- KPI値

---

## 3. Dependency Flow

LogParser
 ↓
OperationAnalyzer
 ↓
 ├─ LossAnalyzer
 │   ↓
 │   BottleneckAnalyzer
 │
 ├─ SheetAnalyzer
 │
 └─ TimeEfficiencyAnalyzer

## 4. Rules（重要）
OperationAnalyzerが唯一の状態ソース
他AnalyzerはOperationIntervalを使う
Analyzer同士で再計算しない
同じ計算を複数箇所でやらない

## 5. NGパターン
❌ SheetAnalyzerで状態判定する
❌ LossAnalyzerで時間再計算する
❌ GUIでKPI計算する

## 6. AIへの指示
新しいロジックは既存Analyzerに追加する
Analyzerを増やす場合は責務を明確にする
重複する処理は統合する