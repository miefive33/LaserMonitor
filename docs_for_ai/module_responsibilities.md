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
- KpiData
- LossData
- BottleneckData
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

### 目的
ログから「稼働区間」を生成する

### 入力
- List<LogEvent>

### 出力
- List<OperationInterval>

### 処理

- Load → Cutting → End を1サイクルとして認識
- 各区間の開始・終了を記録
- durationを算出

### 補足

- 不正ログはスキップ or 補正
- End欠損対応が重要

---

## 3.2 LossAnalyzer

### 目的
ロス時間を分類する

### 入力
- List<OperationInterval>

### 出力
- LossData

### 分類例

- Idle
- Setup
- Waiting
- Unknown

### 処理

- 稼働していない時間帯を抽出
- 種類ごとに集計

---

## 3.3 BottleneckAnalyzer

### 目的
停止要因ランキングを作る

### 入力
- List<OperationInterval>

### 出力
- List<BottleneckData>

### 処理

- 長時間停止を抽出
- 原因別に集計
- 上位N件を返す

---

## 3.4 WeeklyAnalyzer

### 目的
週間トレンドを生成する

### 入力
- List<OperationInterval>

### 出力
- WeeklyKpi

### 処理

- 日単位に分割
- 稼働率計算
- 時系列配列生成

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
- BottleneckData
- WeeklyKpi

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

## 目的
外部リソース操作

## クラス

### SqliteService

---

## 責務

- INSERT
- SELECT

---

## ルール

✔ SQLはここだけ  

❌ Analyzerから直接呼ばない  
❌ UIから直接SQL書かない  

---

# 6. GUI（表示層）

## 目的
データを見せる

---

## ルール

✔ ViewModel経由で受け取る  

❌ 計算しない  
❌ ログ解析しない  

---

# 7. CLI（開発用）

## 目的
ロジック検証

---

## 用途

- Parserテスト
- Analyzerテスト
- デバッグ出力

---

# 8. 最重要ルールまとめ

1. ロジックはAnalyzerのみ  
2. 表示はGUIのみ  
3. DBはServiceのみ  
4. 変換はBuilderのみ  

👉 この分離を崩すとプロジェクトは壊れる