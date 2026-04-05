# Module Responsibilities

このドキュメントは
「どこに何を書くか迷わない」ための絶対ルールである。

今回の更新では、
CNC単位タイムライン表示の責務を明確化し、
KPIパネル保護ルールを追加する。

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
- CNCタイムライン表示用DTO（必要に応じて追加可）

## ルール
✔ プロパティのみ  
✔ DTOとして扱う  

❌ ロジック禁止  
❌ DB禁止  
❌ UI禁止  

---

# 2. Parsers

## LogParser

### 入力
- raw log

### 出力
- List<LogEvent>

### 責務
- 日付変換
- ログ行の構造化
- イベント種別の識別

### NOT responsible
- 稼働時間計算
- 日跨ぎ分割
- ロス分析
- エラー分析
- CNCタイムライン再構築

---

# 3. Analyzers（最重要）

## 共通ルール
✔ Model → Model  
✔ 純粋ロジック中心  
✔ 日次分母は共通  

❌ UI禁止  
❌ DB禁止  
❌ Console出力禁止  

---

## 3.1 分析レイヤ構造

OperationAnalyzer
↓
ScheduleSplitter
↓
機能別Analyzer

---

## 3.2 OperationAnalyzer

### 役割
ログから自動運転区間を復元する。

### 責務
- `Start Scheduling` を開始点とする
- `Scheduling stopped by operator` / `Scheduling interrupted` を終了点とする
- 自動運転区間の元データを生成する

### NOT responsible
- 日跨ぎ処理
- 日次帰属判定
- 機器別集計
- Loss / Error / Bottleneck 判定
- CNCタイムラインの工程割付

---

## 3.3 ScheduleSplitter

### 役割
日跨ぎ処理を一箇所へ集約する。

### 責務
- 自動運転区間を日単位へ切る
- 対象日との重なり時間を返す
- 日次稼働時間の分母を確定する

### 重要
どの日に属するかを判断するのは
ScheduleSplitter のみである。

---

## 3.4 MachineAnalyzer

### 役割
加工機視点の分析。

### 責務
- CUT時間の集計
- 非CUT時間の集計
- 加工機のアイドル時間の算出
- 加工機視点の稼働率算出

### 解釈
加工機の非CUT時間には
- 段取り
- 材料待ち
- 制約待ち
- パレット切替
などが含まれる。

---

## 3.5 SorterAnalyzer

### 役割
仕分け機視点の分析。

### 責務
- SORT時間の集計
- 非SORT時間の集計
- 仕分け機アイドル時間の算出

### 解釈
仕分け機の非SORT時間は、
出番がない時間と、
制約で待たされている時間の両方を含み得る。

---

## 3.6 SystemAnalyzer

### 役割
システム側の動作と制約の分析。

### 責務
- Load / Unload / Unload/Load を集計
- PlaceProduct を集計
- Pallet change / Third pallet change を集計
- Drawer / Warehouse / Loader 関連動作と待ちを抽出

### 重要
System の動作は CUT / SORT と重なり得る。
よって排他的単一状態を前提にしない。

---

## 3.7 SheetAnalyzer

### 役割
CNC単位タイムライン用の工程列を再構築する。

### 責務
- 1行 = 1CNC のタイムライン行を生成する
- CNC名を主名称として扱う
- OrderNo をサブ名称として扱う
- CNC名の接頭辞でフローを切り替える
  - `S****` → Load / Cut / Sort / Unload
  - `P****` → Load / Cut / Unload
- 対象日の中で各工程の開始・終了を求める
- GUIで描画しやすい単純な工程列へ落とす

### 重要
SheetAnalyzer は
UI描画そのものをしてはいけない。

### NOT responsible
- 色決定
- スクロール制御
- 行高制御
- KPI計算

---

## 3.8 LossAnalyzer

### 役割
自動運転中の非付加価値時間を
「現象」として整理する。

### 責務
- 対象設備ごとの Loss を分類する
- Loss を時間構造として集計する
- 同系統の待ちや遅れをまとめる

### 例
#### Machine loss
- CUT待ち
- 供給待ち
- パレットチェンジ待ち

#### Sorter loss
- SORT開始待ち
- 3PC遅延
- 外部パレット都合待ち

#### System loss
- Warehouse ready待ち
- Loader ready待ち
- Drawer ready待ち
- 順序制約待ち

### NOT responsible
- 異常コードの主判定
- エラー原因の決定
- 改善優先順位の決定

---

## 3.9 ErrorAnalyzer

### 役割
明示的な異常イベントと、その影響を扱う。

### 責務
- 失敗イベントの抽出
- エラーコードの抽出
- interrupt の検出
- 停止影響時間の算出
- 再起動や復旧の追跡

### 例
- `Command 'Pickup' FAILED (error code: -6)`
- `Scheduling interrupted`

### 重要
ErrorAnalyzer は
「何が異常だったか」だけでなく、
「その異常でどれだけ止まったか」まで扱う。

---

## 3.10 TimeEfficiencyAnalyzer

### 役割
時間の使われ方を比較しやすい形でまとめる。

### 責務
- 自動運転時間を共通分母として使用
- 機器別 active / idle を比較可能にする
- 日次の効率指標を返す

---

## 3.11 BottleneckAnalyzer

### 役割
改善優先順位を決める。

### 責務
- Loss / Error / System 系の結果を統合
- 総時間、回数、重さ、反復性から優先度化
- 「最初に潰すべき問題」を返す

### 評価軸
- duration
- occurrence count
- interruption severity
- repeatability

### 重要
単純な停止時間ランキングだけにしない。
構造的に詰まっている要因を上位に出せるようにする。

---

# 4. Builders

## KpiBuilder

### 入力
- Analyzer結果

### 出力
- UI表示用データ

### 役割
- 表示用に整形する
- グラフ・カード・テキスト向けに翻訳する

### 禁止
❌ 再計算  
❌ KPIロジック追加  
❌ Loss/Error/Bottleneck の判定  

---

## Timeline表示用Builder処理

### 役割
- SheetAnalyzer の結果を TimelineView 向けに整形する
- CNC表示名
- 工程バー配列
- 時間軸に対する描画用データ
を作る

### 禁止
❌ CNC工程判定  
❌ S/Pルール判定  
❌ ログ直接解釈  

---

# 5. Services

## SqliteService
✔ DB操作のみ  

❌ 分析ロジック禁止  
❌ UI操作禁止  

---

## DashboardService

### 役割
Analyzer群を呼び出し、
GUI / ViewModel が使う結果をまとめる。

### 責務
- Analyzer呼び出しの統括
- 結果の受け渡し
- 必要な分析順序の制御
- KPIデータとタイムラインデータを独立して供給する

### 禁止
❌ 分析ロジック実装  
❌ UI整形ロジック実装  
❌ DBロジック混在  

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
✔ Serviceの結果を受け取る  
✔ 既存タイムライン領域に CNC行を表示する  
✔ 縦スクロールを持たせる  

❌ 計算禁止  
❌ Analyzer呼び出しの乱立  
❌ DB直接操作禁止  
❌ KPIパネル再設計禁止  

---

# 7. KPI Panel Protection Rule（最重要）

KpiPanelView は今回のタイムライン対応で壊してはいけない。

### 禁止
- 既存のKPI構造変更
- KPI用バインディング破壊
- タイムライン都合でのKPI計算変更
- タイムライン表示の責務をKPIパネルへ混入

### 許可
- KPIと無関係な画面領域だけ変更
- KpiPanelView を未変更のまま維持

---

# 8. CLI

テスト専用

## 用途
- Parser検証
- Analyzer検証
- 実ログでの集計確認
- CNCタイムライン再構築結果の確認

---

# 9. 最重要ルール

Analyzer = 頭脳  
Builder = 翻訳  
Service = 指揮  
GUI = 表示  

さらに今回の更新で、

- Loss = 現象
- Error = 異常原因
- Bottleneck = 改善優先度
- Timeline = CNC単位の工程可視化

という境界を明確化する。

この責務を混ぜた瞬間に、
分析結果は解釈不能になる。
