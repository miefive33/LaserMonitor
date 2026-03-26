# SQLite Design (LaserMonitor)

## 1. Purpose

SQLite is used to:
- store parsed and analyzed log data
- avoid reprocessing the same logs
- support incremental updates
- support full rebuild when needed

---

## 2. Data Strategy（重要）

### 基本方針
- ログは追記型
- 過去データは基本変更されない
- 新しい日付のみ追加される

## 3. Key Design Concepts

### ① 日付単位で管理する
データは「日付単位」で扱う
例：
- 2026-03-24 のデータ
- 2026-03-25 のデータ


### ② 重複防止（最重要）
同じデータを二重登録しない
方法：
- Date + UniqueKey を使う
- またはログ行ベースでユニーク制約

### ③ 増分更新
既存DBに存在する日付はスキップ
新しい日付のみ追加
④ 全再構築
DBをクリア
→ 全ログを再解析
→ 再保存

## 4. SqliteService Responsibilities
SqliteServiceは以下のみ行う：

OK;
DB接続
INSERT
SELECT
DELETE（再構築時）
重複チェック
日付存在チェック

NG;
ログ解析
時間計算
KPI算出
UI処理

## 5. Suggested Tables（シンプル版）
DailySummary
Column          Description
Date            日付
OperationRate   稼働率
CuttingTime	    カット時間
SetupTime	    段取り時間
StopTime	    停止時間

OperationIntervals
Column	        Description
Id	PK
Date	        日付
StartTime	    開始
EndTime	        終了
Type	        状態（Cutting等）

SheetResults
Column	        Description
Id	            PK
Date	        日付
SheetName	    シート名
CuttingTime	    加工時間
SetupTime	    段取り

## 6. Import Flow
ログ読み込み
 ↓
日付抽出
 ↓
DBに存在チェック
 ↓
未登録日付のみ処理
 ↓
Analyzer
 ↓
Builder
 ↓
DB保存

## 7. Important Rules（AI用）
DB操作はSqliteServiceに限定
Analyzer結果のみ保存する
GUIはDBを直接触らない
重複チェックは必須
増分更新を基本とする

## 8. Future Extension
インデックス追加
クエリ最適化
週次・月次テーブル