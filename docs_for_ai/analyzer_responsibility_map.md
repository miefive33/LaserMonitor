# Analyzer Responsibility Map

このドキュメントは
「各Analyzerが何を分析し、どこまで責任を持つか」
を明確に定義する。

今回の設計方針では、
日次分析の分母はすべて共通であり、

- 自動運転時間 = `Start Scheduling` から `Scheduling stopped by operator` または `Scheduling interrupted` まで
- 日次稼働時間 = 上記自動運転区間と対象日の重なり時間

とする。

さらに今回の更新では、
GUIタイムラインを CNC単位ガント として表示するため、
SheetAnalyzer の責務を明確化する。

---

# 1. 全体フロー

LogEvent[]
 ↓
OperationAnalyzer
 ↓
OperationInterval[]
 ↓
ScheduleSplitter
 ↓
日次境界で分割済みの OperationInterval[] / day-bounded events
 ↓
 ├─ MachineAnalyzer
 ├─ SorterAnalyzer
 ├─ SystemAnalyzer
 ├─ SheetAnalyzer
 ├─ LossAnalyzer
 ├─ ErrorAnalyzer
 ├─ TimeEfficiencyAnalyzer
 ├─ BottleneckAnalyzer
 ↓
Builder / Service
 ↓
GUI

---

# 2. 設計の本質

この構造では、
分析を以下の5層に分ける。

## ① Denominator
- OperationAnalyzer
- ScheduleSplitter

## ② Role activity
- MachineAnalyzer
- SorterAnalyzer
- SystemAnalyzer

## ③ CNC flow reconstruction
- SheetAnalyzer

## ④ Cause / phenomenon
- LossAnalyzer
- ErrorAnalyzer
- TimeEfficiencyAnalyzer

## ⑤ Improvement priority
- BottleneckAnalyzer

---

# 3. 共通ルール

## 3.1 共通の分母
すべてのAnalyzerは、
同じ「日次自動運転時間」を基準に分析しなければならない。

勝手に別の分母を持ってはいけない。

## 3.2 日跨ぎの責任
日跨ぎの分割は ScheduleSplitter の責任である。

- OperationAnalyzer は元の自動運転区間を復元する
- ScheduleSplitter は対象日との重なり部分へ分割する

## 3.3 重なりの許容
Machine / Sorter / System の活動時間は、
互いに排他的とは限らない。

特に System の動作は、
CUT や SORT と並行して進行することがある。

したがって、
SystemAnalyzer は CUT / SORT と排他的な単一状態を前提にしてはならない。

## 3.4 KPI保護
タイムラインの拡張は、
KPIパネルの責務と完全に分離しなければならない。

---

# 4. OperationAnalyzer

## 役割
ログから自動運転区間の元データを復元する。

## 入力
- List<LogEvent>

## 出力
- List<OperationInterval>

## 責務
- `Start Scheduling` を開始点として検出
- `Scheduling stopped by operator` を終了点として検出
- `Scheduling interrupted` を終了点として検出
- 自動運転区間を復元

## NOT responsible
- 日跨ぎ分割
- 日次帰属判定
- 機器別集計
- ロス分類
- エラー分類
- CNC工程の再構築

---

# 5. ScheduleSplitter

## 役割
日跨ぎの責任を一箇所に集約する。

## 入力
- List<OperationInterval>
- target date

## 出力
- target date に収まる OperationInterval[]
- target date に重なるイベント列

## 責務
- 日跨ぎ区間を対象日との重なり時間に切る
- 深夜0時を跨ぐ区間を日単位へ分割する
- 日次稼働時間の分母を確定する

## 重要
「どの日に何秒属するか」は ScheduleSplitter のみが決める。

---

# 6. MachineAnalyzer

## 役割
加工機視点の分析を行う。

## 分母
- 日次自動運転時間

## 主な指標
- CUT時間
- 非CUT時間
- 加工機視点のアイドル時間

## 責務
- `Cutting started` ～ `Cutting completed` を CUT時間として集計
- 自動運転中だが CUTしていない時間を非CUT時間として集計
- 加工機視点の稼働率を出す

## 注意
加工機の非CUT時間は、
必ずしも「エラー」ではない。
段取り、待機、供給待ち、制約待ちが含まれる。

---

# 7. SorterAnalyzer

## 役割
仕分け機視点の分析を行う。

## 分母
- 日次自動運転時間

## 主な指標
- SORT時間
- 非SORT時間
- 仕分け機視点のアイドル時間

## 責務
- `Sorting started` ～ `Sorting completed` を SORT時間として集計
- 自動運転中だが SORTしていない時間を非SORT時間として集計
- 仕分け機の出番が少ない日と、遅延が多い日を識別できるようにする

---

# 8. SystemAnalyzer

## 役割
システム側の実動作と制約要因を分析する。

## 分母
- 日次自動運転時間

## 主な対象
- Load
- Unload
- Unload/Load
- PlaceProduct
- Pallet change
- Third pallet change
- Drawer移動
- Warehouse関連待ち
- Loader関連待ち

## 責務
- システム実動時間を集計する
- 供給・搬送・段取り系の活動を検出する
- システム側の待ちや制約を抽出する

## 重要
System の実動時間は CUT / SORT と重なることがある。
したがって SystemAnalyzer は
「全時間を単一の状態に塗り分ける」前提では設計しない。

---

# 9. SheetAnalyzer

## 役割
CNC単位の1日ガント表示用フローを再構築する。

## 分母
- 対象日のイベント列
- 必要に応じて日次自動運転時間の内部イベントのみを対象とする

## 出力イメージ
1行 = 1 CNC

例:
- S1-02535084 (2539821)
- P4-02536627 (2539079)

## フロー定義

### S系（仕分けあり）
- Load
- Cut
- Sort
- Unload

### P系（仕分けなし）
- Load
- Cut
- Unload

## 判定ルール
- CNC名が `S****` → sorting required
- CNC名が `P****` → no sorting

## 責務
- CNC名を主キーとして行を生成する
- OrderNo を副名称として保持する
- CNCごとに Load / Cut / Sort / Unload の時間帯を再構成する
- 同日の中で GUI 表示用に扱いやすいシンプル工程列を返す

## 重要
SheetAnalyzer は
「詳細な搬送機内部状態の完全再現」ではなく、
CNC単位の工程可視化を目的とする。

## NOT responsible
- 色決定
- UI描画
- スクロール制御
- KPI計算
- Loss/Error/Bottleneck の再解釈

---

# 10. LossAnalyzer

## 役割
自動運転中の非付加価値時間を「現象」として整理する。

## 定義
Loss = 自動運転中に対象設備が本来の仕事をしていない時間
ただし、明示的な異常停止は ErrorAnalyzer の主担当とする。

## 分類対象
### Machine loss
- CUT待ち
- パレットチェンジ待ち
- 材料供給待ち
- アンロード制約待ち

### Sorter loss
- SORT開始待ち
- 3PC切替待ち
- 外部パレット優先による SORT遅延

### System loss
- Drawer ready待ち
- Warehouse ready待ち
- Loader ready待ち
- 順序制約待ち
- 次工程成立待ち

## 重要
Loss は「止まり方・遅れ方」の分類であり、
原因を断定するものではない。

---

# 11. ErrorAnalyzer

## 役割
明示的な異常イベントと、その影響を分析する。

## 定義
Error = ログに明示的な失敗・異常コード・interrupt が現れ、
自動運転の継続性が壊れたもの

## 例
- `Command 'Pickup' FAILED (error code: -6)`
- `Scheduling interrupted`

## 責務
- エラー種別の抽出
- エラー発生時刻の記録
- エラーコードの抽出
- どの動作中に起きたかの特定
- 中断時間・復旧時間の算出
- 再起動を伴ったかの追跡

## 重要
ErrorAnalyzer は単なるエラー一覧ではなく、
「その異常が何分止めたか」まで扱う。

---

# 12. TimeEfficiencyAnalyzer

## 役割
時間の使われ方を俯瞰する。

## 例
- 日次自動運転時間
- 機器別アクティブ時間
- 非アクティブ時間
- 各視点の比率

## 重要
TimeEfficiencyAnalyzer は
共通分母を使って比較可能な形に整える。

---

# 13. BottleneckAnalyzer

## 役割
改善優先順位を決める。

## 定義
Bottleneck = その日の稼働を最も強く妨げた制約・異常・繰り返し要因

## 入力
- LossAnalyzer の結果
- ErrorAnalyzer の結果
- SystemAnalyzer の結果
- 必要に応じて MachineAnalyzer / SorterAnalyzer の結果

## 評価軸
- total duration（総時間）
- occurrence count（発生回数）
- interruption severity（止まり方の重さ）
- repeatability（同じ原因の反復性）

## 重要
単純な「長時間順ランキング」にしてはいけない。
短くても頻発し全体テンポを壊す要因は
ボトルネック候補になり得る。

---

# 14. Analyzer間ルール

- Analyzer同士の相互依存は禁止
- 共通の基準は OperationInterval / ScheduleSplitter 後の結果とする
- Loss は現象
- Error は異常原因
- Bottleneck は改善優先度
- Timeline は CNC工程可視化

この境界を崩してはいけない。

---

# 15. 結論

今後のAnalyzer設計は、
単なる時間集計ではなく、

- 共通分母
- 日跨ぎの厳密処理
- 3機器別視点
- CNC単位タイムライン
- Loss / Error / Bottleneck の責務分離
- KPIパネル保護

を守る必要がある。

この構造によって初めて、
現場改善に使える分析になる。