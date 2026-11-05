# ステージガイド

ステージデータは `StageCatalog.cs` で組み立て、`StageValidator` で検証します。以下の解は `StageSolutionTests` の各ステージ向けテストと一致します。記録は矢印または WASD、**Enter** で確定、Playback では **Space** で 1 Tick 進行。

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

記録のみ。確定で Playback なしでクリア（`StageSolutionTests` の WALK 向け解テスト）。

<details>
<summary>解（記録 + Enter）</summary>

- **記録:** `UURRR`（5 手）
- **Enter:** 記録確定
- **Enter 後の Space:** **0**（確定時点で Cleared）

</details>

**短い解の全探索:** `StageSolutionTests.cs` 内の WALK 向け全探索テストは探索上限 5 手で、`counts[5]` が 2、`solutions` が **`RRRUU`** と **`UURRR`** のみ（**記録長 5 でクリアする解がちょうど 2 つ**）。

## ECHO（index 1）

<details>
<summary>解</summary>

- **記録:** `RRRR`
- **Enter:** 確定 → Playback
- **Space:** **3** 回（tick 3 で Cleared）

</details>

**短い解の全探索:** ECHO 向け全探索テスト — 長さ 4 の解が 1 つ: `RRRR`。

## GATE（index 2）

<details>
<summary>解</summary>

- **記録:** `UULLDLDD`
- **Enter:** 確定 → Playback
- **Space:** **6** 回（Cleared）

</details>

**短い解の全探索:** GATE 向け全探索テスト — 長さ 8 の解が 1 つ: `UULLDLDD`。

## TWIN（index 3）

<details>
<summary>解</summary>

- **記録:** `RRRRUUUDDD`
- **Enter:** 確定 → Playback
- **Space:** **7** 回（Cleared）

</details>

## TANGLE（index 4）

<details>
<summary>解</summary>

- **記録:** `LLURURRLUURR`（12 手）
- **Enter:** 確定 → Playback
- **Space:** **12** 回（Cleared）

</details>

## ナビゲーション

- **N / P:** **次** / **前** のステージ（端でクランプ）。`StageNavigationResolver` と `BoardSessionDriver`。
- **R:** 任意モードからリトライ（`InputGuide.cs`）。

## テスト

- ステージごとの解: `StageSolutionTests.cs`
- プレフィックス / 空確定: 同ファイル内のプレフィックス・空確定向けテスト
- 短い解の全探索: WALK / ECHO / GATE のみ（`StageSolutionTests.cs`）
