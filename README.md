# LaserMonitor

レーザー加工機のログを解析し、稼働状況・ロス・ボトルネックを可視化するWPFアプリケーションです。

---

## 📊 概要

LaserMonitorは以下を目的としたシステムです：

- ログファイルの解析
- 稼働率（KPI）の算出
- ロス分析
- ボトルネック特定
- タイムライン可視化
- SQLiteによるデータ蓄積

---

## 🖥️ 主な機能

### ■ ダッシュボード表示
- 本日のサマリー（稼働率・内訳）
- ロス分析
- ボトルネックランキング

### ■ タイムライン表示
- シートごとの稼働状況を時系列表示

### ■ 週間サマリー
- 1週間の稼働率推移（BottomPanel中央）

### ■ 時間帯別分析
- 時間帯ごとの稼働率

---

## 🏗️ アーキテクチャ

本プロジェクトはレイヤー分離を重視した構成です。

### Core（ビジネスロジック）
- Parser：ログ解析
- Analyzer：状態判定・KPI算出
- Builder：データ整形
- Service：SQLite操作

### GUI（表示）
- View：UI表示
- ViewModel：データバインディング

👉 詳細は以下参照：
- `docs_for_ai/architecture.md`
- `docs_for_ai/analyzer_responsibility_map.md`
- `docs_for_ai/sqlite_design.md`

---

## 📁 プロジェクト構成

詳細な構成は以下を参照：

👉 `docs_for_ai/project_tree.md` :contentReference[oaicite:0]{index=0}

---

## 🎯 設計方針

### ■ Analyzer中心設計
- すべての計算はAnalyzerに集約
- GUIは表示のみ

### ■ DB責務分離
- SQLite操作は `SqliteService` のみ
- 計算ロジックは禁止

### ■ UI固定
- レイアウト変更は禁止
- 既存パネルの改善のみ許可

👉 詳細：
- `docs_for_ai/ui_design_constraints.md` :contentReference[oaicite:1]{index=1}

---

## 🤖 AI開発について

本プロジェクトはAIを活用した開発を前提としています。

### ■ ルール
AIは以下の設計ドキュメントに従うこと：

- architecture.md
- dependency_rules.md
- module_responsibilities.md :contentReference[oaicite:2]{index=2}
- project_tree.md
- ui_design_constraints.md
- sqlite_design.md :contentReference[oaicite:3]{index=3}
- analyzer_responsibility_map.md :contentReference[oaicite:4]{index=4}

## 🤖 AI Development Rule

AI must follow:

* Docs/AGENTS.md (highest priority)
* architecture.md
* module_responsibilities.md
* ui_design_constraints.md

If conflict occurs:
→ Follow AGENTS.md
---

## 🚀 実行方法

（※後で追記でもOK）

---

## 📌 今後の予定

- SQLite最適化（インデックス・パフォーマンス）
- 週次・月次分析の強化
- UI改善（視認性向上）
- リアルタイム更新対応

---

## 📄 ライセンス

未定（必要に応じて追加）

---

## ✨ 補足

本プロジェクトは

👉 「設計をAIに理解させて開発する」

ことを目的とした構成になっています。


