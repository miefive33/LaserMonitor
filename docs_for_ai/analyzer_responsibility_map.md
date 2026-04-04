# Analyzer Responsibility Map

---

# 全体フロー（改訂版 / 日次稼働分析対応）

LogEvent[]
 ↓
OperationAnalyzer
 ↓
ScheduleInterval[]
 ↓
ScheduleSplitter
 ↓
DailyScheduleSegment[]
 ↓
 ├─ MachineAnalyzer
 ├─ SorterAnalyzer
 ├─ SystemAnalyzer
 ├─ LossAnalyzer
 ├─ ErrorAnalyzer
 ├─ TimeEfficiencyAnalyzer
 ├─ BottleneckAnalyzer
 └─ SheetAnalyzer
 ↓
結果統合
 ↓
KpiBuilder / DailyReportBuilder

---

# 🎯 この構造の中心思想

今回の分析で最重要なのは、

> まず「その日の自動運転時間」を確定し、
> その同じ分母の中で、
> 加工機 / 仕分け機 / システム を別々に評価すること

である。

つまり、Analyzer構造は

- event中心
- sheet中心

ではなく、

- schedule中心
- 日次分母中心
- 機器役割中心

でなければならない。

---

# 1. OperationAnalyzer

## 役割
ログから Schedule START / STOP を見つけ、元の自動運転区間を再構築する

## 入力
- List<LogEvent>

## 出力
- List<ScheduleInterval>

## 判定基準
- Start Scheduling
- Scheduling stopped by operator
- Scheduling interrupted

## 重要
ここではまだ日単位に切らない。
前日開始・翌日終了を含んだ「元区間」を作る。

---

# 2. ScheduleSplitter

## 役割
自動運転区間を対象日の範囲に切り出す

## 入力
- List<ScheduleInterval>
- target date

## 出力
- List<DailyScheduleSegment>

## 重要ルール

### 2.1 日の範囲
- 00:00:00 - 23:59:59

### 2.2 日跨ぎ対応
- 前日から継続してきた区間を当日分だけ切り出す
- 当日から翌日へ継続する区間を当日分だけ切り出す

### 2.3 このAnalyzerが決めるもの
- その日の自動運転総時間
- すべての役割分析の分母

---

# 3. MachineAnalyzer

## 役割
加工機の稼働を分析する

## 入力
- List<LogEvent>
- List<DailyScheduleSegment>
- target date

## 出力
- MachineSummary

## 主要指標
- CUT時間
- IDLE時間
- 必要に応じて WAIT / INTERRUPT 補助分類

## ルール
- `Cutting started` → `Cutting completed`
- 当日範囲へクリップ
- 自動運転時間外のCUTは集計しない

---

# 4. SorterAnalyzer

## 役割
仕分け機の稼働を分析する

## 入力
- List<LogEvent>
- List<DailyScheduleSegment>
- target date

## 出力
- SorterSummary

## 主要指標
- SORT時間
- IDLE時間

## ルール
- `Sorting started` → `Sorting completed`
- 当日範囲へクリップ
- 自動運転時間外のSORTは集計しない

---

# 5. SystemAnalyzer

## 役割
システム全体の搬送・段取り・入出庫動作を分析する

## 入力
- List<LogEvent>
- List<DailyScheduleSegment>
- target date

## 出力
- SystemSummary

## 対象イベント例
- Load sheet
- Unload sheet
- Unload/Load
- PlaceProduct
- Pallet change
- Third pallet change
- Drawer movement
- MaterialStockerSelect

## 主要指標
- System working time
- System idle time

## 重要
System working time は union で集計する。
異なる搬送動作の重複二重計上は避ける。

また、System時間は CUT / SORT と重なりうるため、
排他的状態にしてはいけない。

---

# 6. LossAnalyzer

## 役割
ロス構造を分析する

## 入力
- DailyScheduleSegment
- MachineSummary
- SorterSummary
- SystemSummary
- 必要なイベント列

## 出力
- LossData

## 目的
- 何に時間が使われていないか
- どこが Unknown になっているか
- どこに分類改善余地があるか

## 注意
LossAnalyzerは分母を作らない。
分母は常にScheduleSplitter結果。

---

# 7. ErrorAnalyzer

## 役割
異常・中断・再起動を分析する

## 入力
- List<LogEvent>
- DailyScheduleSegment

## 出力
- ErrorData

## 対象例
- FAILED
- Scheduling interrupted
- Software closed
- Software started
- retry / repeated failures

---

# 8. TimeEfficiencyAnalyzer

## 役割
自動運転時間に対する各役割の効率を計算する

## 入力
- operation time
- machine summary
- sorter summary
- system summary

## 出力
- TimeEfficiencyResult

## 例
- laser cut ratio
- sorter active ratio
- system active ratio

---

# 9. BottleneckAnalyzer

## 役割
改善優先順位を決める

## 入力
- LossData
- ErrorData
- TimeEfficiencyResult

## 出力
- Bottleneck result

## 目的
- 長い停止
- 頻発エラー
- 生産性阻害の大きい要因

を優先順で示す

---

# 10. SheetAnalyzer

## 役割
シート / 注文単位の補助分析

## 位置づけ
主分析ではない。
日次稼働の中心はあくまで

- ScheduleInterval
- DailyScheduleSegment
- role-based analyzers

である。

---

# 🔥 最大の修正ポイント

旧設計では、Analyzer責務がやや抽象的で、

- 何が分母か
- 日跨ぎを誰が持つか
- 3機器分析を誰が持つか

が曖昧だった。

今回の改訂では明確に以下へ変更する。

### ① 分母
ScheduleSplitter が日次自動運転時間を確定する

### ② 加工機
MachineAnalyzer が CUT / IDLE を持つ

### ③ 仕分け機
SorterAnalyzer が SORT / IDLE を持つ

### ④ システム
SystemAnalyzer が LOAD / UNLOAD / 搬送実動を持つ

### ⑤ 原因分析
LossAnalyzer / ErrorAnalyzer / BottleneckAnalyzer が持つ

---

# Analyzer間ルール

❌ 相互依存禁止  
❌ GUI参照禁止  
❌ DB参照禁止  
✔ DailyScheduleSegment を共通分母にする  
✔ role-based results を明示的に分離する  

---

# 結論

この構造で初めて

- 日付またぎに強く
- 日次分母がぶれず
- 加工機 / 仕分け機 / システムを同じ日で比較できる

分析になる。
