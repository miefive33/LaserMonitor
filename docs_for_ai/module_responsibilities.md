# Module Responsibilities

このドキュメントは
「どこに何を書くか迷わない」ための絶対ルール

---

# 1. Models（データ定義専用）

## 対象

- LogEvent
- OperationInterval
- DailySummary
- SummaryResult
- LossData
- ErrorData
- TimeEfficiencyResult
- Machine
- TargetMachines
- OrderInfo
- SheetInfo

### 追加で持つべき概念（重要）

必要に応じて、以下のようなモデル概念を導入してよい。

- ScheduleInterval
- DailyScheduleSegment
- MachineSummary
- SorterSummary
- SystemSummary

ただし新規作成する場合でも、責務は **データ保持のみ** とする。

## ルール

✔ プロパティのみ  
✔ DTO  
✔ Analyzerの入出力として使う  

❌ ロジック禁止  
❌ DB禁止  
❌ UI依存禁止  

---

# 2. Parsers

## LogParser

### 入力
- raw log

### 出力
- List<LogEvent>

### 責務
- Day/Month/Year の日時変換
- 1行ごとのイベント化
- イベント種別の正規化
- メッセージ文字列の保持

### ルール

✔ 1行 → 1イベント  
✔ ログ内容を失わない  

❌ 時間集計しない  
❌ 日跨ぎ判定しない  
❌ 稼働率を作らない  

---

# 3. Analyzers（最重要）

## 共通ルール

✔ Model → Model  
✔ 純粋ロジック  
✔ UI非依存  
✔ DB非依存  

❌ UI禁止  
❌ DB禁止  
❌ MessageBox禁止  
❌ ViewModel依存禁止  

---

## 3.1 OperationAnalyzer

### 役割
ログから自動運転区間を再構築する

### 入力
- List<LogEvent>

### 出力
- List<ScheduleInterval> または同等の区間モデル

### やること
- `Start Scheduling` を開始点として認識
- `Scheduling stopped by operator` を終了点として認識
- `Scheduling interrupted` を終了点として認識
- 日付をまたぐ前の「元の運転区間」を作る

### やらないこと
- 日単位への切り分け
- CUT時間集計
- SORT時間集計
- システム稼働時間集計

---

## 3.2 ScheduleSplitter

### 役割
自動運転区間を日単位に切り分ける

### 入力
- List<ScheduleInterval>
- target date

### 出力
- List<DailyScheduleSegment>

### やること
- 00:00:00 - 23:59:59 の日範囲へクリップ
- 前日開始 / 当日継続を切り出す
- 当日開始 / 翌日継続を切り出す
- 日次レポートの分母を確定する

### やらないこと
- CUT/SORT/SYSTEM集計
- ロス原因分類

---

## 3.3 MachineAnalyzer

### 役割
レーザー加工機視点の分析

### 入力
- List<LogEvent>
- List<DailyScheduleSegment>
- target date

### 出力
- MachineSummary または SummaryResultの機械用結果

### やること
- `Cutting started` → `Cutting completed` を抽出
- 当日範囲へクリップ
- 自動運転時間内だけを有効時間として扱う
- CUT時間を集計
- IDLE時間 = 自動運転時間 - CUT時間 を算出

### 将来拡張候補
- WAIT
- INTERRUPT
- PALLET CHANGE関連時間

### やらないこと
- SORT時間分析
- システム搬送時間分析

---

## 3.4 SorterAnalyzer

### 役割
仕分け機視点の分析

### 入力
- List<LogEvent>
- List<DailyScheduleSegment>
- target date

### 出力
- SorterSummary または SummaryResultの仕分け機用結果

### やること
- `Sorting started` → `Sorting completed` を抽出
- 当日範囲へクリップ
- 自動運転時間内だけを有効時間として扱う
- SORT時間を集計
- IDLE時間 = 自動運転時間 - SORT時間 を算出

### やらないこと
- CUT時間分析
- システム搬送時間分析

---

## 3.5 SystemAnalyzer

### 役割
システム全体の搬送・入出庫・段取り動作を分析

### 入力
- List<LogEvent>
- List<DailyScheduleSegment>
- target date

### 出力
- SystemSummary または SummaryResultのシステム用結果

### やること
- Load sheet 区間抽出
- Unload sheet 区間抽出
- Unload/Load 区間抽出
- PlaceProduct 区間抽出
- Pallet change / Third pallet change 区間抽出
- Drawer movement / MaterialStockerSelect 区間抽出
- それらの区間の union を作り、システム実動時間を集計
- システムアイドル時間 = 自動運転時間 - システム実動時間 を算出

### 重要
システム時間はCUTやSORTと重なりうる。
よって排他的状態として扱わない。

---

## 3.6 LossAnalyzer

### 役割
非稼働時間の構造分析

### 入力
- 日次の自動運転時間
- 各Analyzer結果
- 必要なイベント列

### 出力
- LossData

### やること
- 自動運転中に何が使われていないかを分類
- Unknownを最小化する方向で分類精度を上げる
- 機械別 / システム別のロス補助情報を作る

### 注意
LossAnalyzerは分母を作らない。
分母は必ずScheduleSplitter結果を使う。

---

## 3.7 ErrorAnalyzer

### 役割
中断・失敗・再起動などの異常事象を分析

### 入力
- List<LogEvent>
- DailyScheduleSegment

### 出力
- ErrorData

### やること
- `Scheduling interrupted`
- `FAILED`
- software close / software start
- retry patterns

を抽出し、件数・時間帯・影響を整理する

---

## 3.8 TimeEfficiencyAnalyzer

### 役割
時間配分の効率分析

### 入力
- 日次自動運転時間
- Machine / Sorter / System の各結果

### 出力
- TimeEfficiencyResult

### やること
- 分母に対する各役割の比率を作る
- CUT率
- SORT率
- SYSTEM実動率

---

## 3.9 BottleneckAnalyzer

### 役割
改善優先順位を決める

### 入力
- LossData
- ErrorData
- TimeEfficiencyResult

### 出力
- ボトルネック結果

### やること
- 長時間ロス
- 頻発エラー
- 生産阻害の大きい要因

を優先度順に並べる

---

## 3.10 SheetAnalyzer

### 役割
シート単位・注文単位の補助分析

### 入力
- List<LogEvent>
- 必要な注文情報

### 出力
- SheetInfo / OrderInfo 系結果

### 注意
SheetAnalyzerは日次の主分母を作らない。
日次稼働分析の中心は ScheduleInterval ベースである。

---

# 4. Builders

## KpiBuilder

### 入力
- Analyzer結果

### 出力
- UI表示用データ

### 役割
- 表示用文字列作成
- カード用数値作成
- グラフ入力形式への整形

### ルール

✔ フォーマットのみ  
❌ 再計算禁止  
❌ 分母の再構築禁止  
❌ Analyzer相当の推論禁止  

---

## DailyReportBuilder

### 役割
日報用の出力整形

Analyzerの結果をまとめ、
1日分のレポート構造に変換する。

---

# 5. Services

## SqliteService

✔ DB操作のみ  
✔ INSERT / SELECT / DELETE  

❌ 分析禁止  
❌ UI禁止  

---

## DashboardService

### 役割
Analyzer呼び出し統括

### やること
- LogParser呼び出し
- OperationAnalyzer呼び出し
- ScheduleSplitter呼び出し
- 各Analyzer呼び出し
- Builder呼び出し
- ViewModelへ返す

### やらないこと
- 分析ロジックを自前実装しない
- UIフォーマットを持ちすぎない

👉 “オーケストレーター”

---

# 6. GUI

## View

- HeaderView
- KpiPanelView
- TimelineView
- BottomPanelView

## ViewModel

- MainViewModel

## ルール

✔ 表示のみ  
✔ コマンド発火  
✔ 日付選択  

❌ 計算禁止  
❌ ログ解析禁止  
❌ 日跨ぎ処理禁止  

---

# 7. CLI

テスト専用

### 用途
- Parser検証
- Analyzer検証
- 日跨ぎ検証
- 3機器分析の妥当性確認

---

# 8. 最重要ルール

Analyzer = 頭脳  
Builder = 翻訳  
Service = 指揮  
GUI = 表示  

さらに今回の分析基準として:

- 分母 = 自動運転時間
- 加工機 = CUT基準
- 仕分け機 = SORT基準
- システム = LOAD / UNLOAD / 搬送基準
- 日跨ぎは ScheduleSplitter が責任を持つ

👉 この前提を崩すと、日次分析は信用できなくなる
