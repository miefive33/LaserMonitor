# UI Design Constraints（最重要）

## 1. Base UI

This project MUST follow the existing UI layout.

The reference image is:
👉 system dashboard screenshot (provided)

---

## 2. Layout Structure（固定）

### Top Row
- 左：本日のサマリー（円グラフ）
- 中：ロス分析（テキスト）
- 右：ボトルネック（ランキング）

---

### Middle
- タイムライン表示（CNCごとの稼働ガント）

---

### Bottom Row（重要）
- 左：時間帯別稼働率
- 中：週間稼働率（グラフ）
- 右：詳細情報表示エリア

---

## 3. Absolute Rules

❌ 新しいパネルを勝手に追加しない  
❌ レイアウト構造を変更しない  
❌ UI階層を変えない  
❌ KPIパネルの構造を壊さない  

⭕ 既存パネルの中身だけ改善する  

---

## 4. KPI Panel Protection Rule（最重要）

Top Row 左の KPI パネルは既存の主表示である。

AI MUST NOT:

- KPIパネルの位置を変える
- KPIパネルのサイズ思想を変える
- KPIパネルの既存バインドを壊す
- タイムライン改善のために KPI を作り直す

AI MAY:

- 既存データをそのまま使う
- KPIパネルに手を入れずに他パネルだけ更新する

---

## 5. Timeline Rule（重要）

Middle のタイムラインは、今後以下の方針で表示する。

### 5.1 表示単位
- 1行 = 1 CNC

### 5.2 ラベル
- 主名称 = CNC名
- サブ名称 = OrderNo を括弧内表示

例:
- S1-02535084 (2539821)
- P4-02536627 (2539079)

### 5.3 フロー
#### S系（仕分けあり）
- Load
- Cut
- Sort
- Unload

#### P系（仕分けなし）
- Load
- Cut
- Unload

### 5.4 分類ルール
- CNC名が `S****` → 仕分けあり
- CNC名が `P****` → 仕分けなし

### 5.5 表示方針
- シンプルなガント表示を優先
- まずは工程の流れを一目で分かるようにする
- 細かすぎる内部状態は初期表示に入れない
- 必要なら将来 Waiting を追加可能にする

---

## 6. Timeline Scrolling Rule

一度に表示するCNC行数は、
視認性を優先して 3～4 行程度を基本とする。

そのため:

- 縦スクロールを許可する
- すべてのCNCを確認できるようにする
- 行高は読みやすさを優先する

---

## 7. Weekly Summary Rule

BottomPanel中央：

👉 週間稼働率グラフを表示する

- データはCoreから渡す
- GUIで計算しない

---

## 8. Bottom Right Area Rule

BottomPanel右は、
将来の詳細表示や選択中CNCの追加情報表示に使える。

ただし現時点では:

- 新規パネル追加は禁止
- 既存レイアウト枠の中でのみ改善可能

---

## 9. Design Philosophy

- ダークテーマ維持
- 情報密度を優先
- “現場で一目で分かる”UI
- KPIは上段で概要把握
- タイムラインは中段で流れ把握
- 読みやすさを優先し、詰め込みすぎない

---

## 10. If UI change is required

If UI change is required:
→ MUST explain before modifying

---

## 🤖 AI UI RULES（Codex用）

### Layout（絶対固定）

DO NOT:

* add new panels
* change layout structure
* move existing components
* break KpiPanelView

---

### ✔ ALLOWED

* add controls inside HeaderView
* update content only
* replace timeline contents inside the existing timeline area
* add scrolling inside the existing timeline area

---

### HeaderView Rule（重要）

HeaderView is the ONLY place for:

* date selection
* filters

---

### Data Handling

DO NOT:

* compute data in UI
* transform data in UI

ONLY:

* display data