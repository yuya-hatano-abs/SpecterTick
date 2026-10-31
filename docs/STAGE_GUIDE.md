# ステージガイド

ステージデータは `StageCatalog.cs` で組み立て、`StageValidator` で検証します。以下の解は `StageSolutionTests`（`SS1`〜`SS5`）と一致します。記録は矢印または WASD、**Enter** で確定、Playback では **Space** で 1 Tick 進行。

記録文字列: **U** 上、**D** 下、**L** 左、**R** 右。

## 一覧

| Index | 名前 | サイズ (W×H) | スイッチ | ドア | メモ |
|------:|------|--------------|----------|------|------|
| 0 | WALK | 4×3 | 0 | 0 | スイッチなしで出口が開く（`StageCatalog.cs`） |
| 1 | ECHO | 5×3 | 1 (S1) | 0 | Specter がプレイヤーと離れた位置から開始 |
| 2 | GATE | 6×4 | 1 (S1) | 1 (D1 → S1) | D1 は S1 に連動 |
| 3 | TWIN | 6×4 | 2 (S1, S2) | 2 (D1→S1, D2→S2) | 2 スイッチと 2 ドア |
| 4 | TANGLE | 6×4 | 2 (S1, S2) | 2 (D1,D2 → 両スイッチ) | AND 条件のドア |

## WALK（index 0）

記録のみ。確定で Playback なしでクリア（`SS1_Walk_Solution`）。

<details>
<summary>解（記録 + Enter）</summary>

- **記録:** `UURRR`（5 手。`SS1_Walk_Solution`）
- **Enter:** 記録確定
- **Enter 後の Space:** **0**（確定時点で Cleared）

</details>

**短い解の全探索:** `StageSolutionTests.SX1_Walk_ExhaustiveShortSolutions` は探索上限 5 手で、`counts[5]` が 2、`solutions` が **`RRRUU`** と **`UURRR`** のみ（**記録長 5 でクリアする解がちょうど 2 つ**）。

## ECHO（index 1）

<details>
<summary>解</summary>

- **記録:** `RRRR`（`SS2_Echo_Solution`）
- **Enter:** 確定 → Playback
- **Space:** **3** 回（tick 3 で Cleared）

</details>

**短い解の全探索:** `SX2_Echo_ExhaustiveShortSolutions` — 長さ 4 の解が 1 つ: `RRRR`。

## GATE（index 2）

<details>
<summary>解</summary>

- **記録:** `UULLDLDD`（`SS3_Gate_Solution`）
- **Enter:** 確定 → Playback
- **Space:** **6** 回（Cleared）

</details>

**短い解の全探索:** `SX3_Gate_ExhaustiveShortSolutions` — 長さ 8 の解が 1 つ: `UULLDLDD`。

## TWIN（index 3）

<details>
<summary>解</summary>

- **記録:** `RRRRUUUDDD`（`SN4_Twin_Prefix_And_Empty` / `SS4_Twin_Solution`）
- **Enter:** 確定 → Playback
- **Space:** **7** 回（Cleared）

</details>

## TANGLE（index 4）

<details>
<summary>解</summary>

- **記録:** `LLURURRLUURR`（12 手。`SS5_Tangle_Solution`）
- **Enter:** 確定 → Playback
- **Space:** **12** 回（Cleared）

</details>

## ナビゲーション

- **N / P:** **次** / **前** のステージ（端でクランプ）。`StageNavigationResolver` と `BoardSessionDriver`。
- **R:** 任意モードからリトライ（`InputGuide.cs`）。

## テスト

- ステージごとの実行: `StageSolutionTests.cs` の `SS1`〜`SS5`
- プレフィックス / 空確定: `SN1`〜`SN5`
- 短い解の全探索件数: `SX1`〜`SX3`（WALK、ECHO、GATE のみ）
