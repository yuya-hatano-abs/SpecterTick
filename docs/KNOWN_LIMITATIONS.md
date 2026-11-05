# 既知の制限

本ドキュメントは、リポジトリ上の実装・テストから読み取れる制限を列挙します（優先度順ではありません）。

## ゲーム内容・演出

| 項目 | 状態 |
|------|------|
| 演出・アニメーション・サウンド | 未実装 |
| HUD / ステージラベル / クリア画面 | スキン対象外（図形＋テキスト） |
| How to play オーバーレイ（起動時・**H**） | 実装済み（英語 ASCII、`HowToPlayView`） |

## MVP・デバッグ

| 項目 | 状態 |
|------|------|
| MVP: グリッド移動・記録・再生・スイッチ/ドア・リトライ | Core・Presentation・自動テストあり |
| MVP: 決定性の単体テスト | Core にテストあり（例: `GridPuzzleEngineSessionDeterminismTests`） |
| MVP: 簡易デバッグ | **一部:** Playback の **Space**（`InputGuide`）と **HUD**。**未実装:** 仕様書 §4.1 の **EditorWindow** デバッグツール |

## 描画・視認性

| 項目 | 状態 |
|------|------|
| 図形描画時の Player / Specter の区別 | 輝度コントラスト比 **約 1.13**（`PaletteContrastTests`）。**色相のみで区別しており、区別が弱い** |
| スキン PNG 使用時の Player / Specter | 不透明ピクセルの平均輝度比が **1.5 以上**（`SkinAssetQualityTests` の `MinPlayerSpecterContrast`） |

## 性能

| 項目 | 状態 |
|------|------|
| 実機・内蔵 GPU での FPS / メモリ | **未測定**（記録なし） |

## ステージ解・探索テスト

| 項目 | 状態 |
|------|------|
| WALK / ECHO / GATE の短い解 | `StageSolutionTests.cs` 内の WALK / ECHO / GATE 向け全探索テスト |
| TWIN / TANGLE の最短性 | 全探索テスト **なし**（`StageSolutionTests` クラスコメント） |

## その他

| 項目 | 状態 |
|------|------|
| 画面サイズ追従 | `ScreenSizeTrackerTests`（EditMode）のみ |
| `LoadStage` 失敗時 | 自動テスト **なし** |
| 同時多キー | EditMode `InputIntentResolverTests`（3 件）で `InputIntentResolver.Resolve` の優先順位を検証。ドライバー経由の PlayMode は、N と Right を同一フレームで押す **1 パターン**（`BoardSessionDriverPlayModeTests`）のみ。それ以外の組み合わせのドライバー経由自動テスト **なし** |

## AI 協働開発に伴う限界

[README の「AI協働開発について」](../README.md#ai協働開発について) を参照。レビューと承認も AI が行うため、**独立した人間による検証は行われていない**。
