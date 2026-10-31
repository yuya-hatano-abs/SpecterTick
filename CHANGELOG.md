# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-10-31

### Added

- SPECTER//TICK ゲームリポジトリ: コマンド記録と Specter 再生、カタログ 5 ステージ、HUD・クリア画面・ステージラベル。
- Unity Presentation 層（`SpecterTick.Presentation`）と UPM `com.yuya_hatano.specter-tick-core` の `file:` 参照。
- EditMode テスト `SpecterTick.Presentation.Tests`（163 件）と PlayMode 回帰 `SpecterTick.Presentation.PlayModeTests`（11 件）。
- 任意ボードスキン（`Assets/SpecterTick/Presentation/Resources/Skin/`。スプライト欠落時は図形描画のまま）。
- 画像スキン用 EditMode テスト（コントラスト関連を含む）。
- ビルド設定 EditMode テスト、PlayMode ドライバテスト、Windows 64 ビットスタンドアロンビルドの batchmode 検証手順（`docs/BUILD.md`）。
- ドキュメント: README、アーキテクチャ、ビルド手順、ステージ解説、`DESIGN_SPEC.md` 末尾の実装対応。
