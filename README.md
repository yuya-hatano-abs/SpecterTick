# SPECTER//TICK

**SPECTER//TICK** は、移動コマンドを記録し **Specter（残像）** が再生してスイッチとドアを協力させる **2D グリッドパズル** です。本リポジトリは Unity 上の入力・描画・UI（Presentation）を含み、ロジックは別リポジトリの UPM パッケージ `com.yuya_hatano.specter-tick-core` にあります。

**位置づけ:** 転職用ポートフォリオとして公開している **AI 協働開発の実験** です（詳細は [AI協働開発について](#ai協働開発について)）。

**遊び方:** [遊び方](#遊び方) / [docs/STAGE_GUIDE.md](docs/STAGE_GUIDE.md)

**English summary:** SPECTER//TICK is a 2D grid puzzle: record moves, press Enter to confirm, then advance the Specter with Space during Playback to cooperate on switches and doors. Core logic is in a Unity-free UPM package; this repo is the Presentation layer. Five stages are in `StageCatalog`. An English how-to-play overlay appears at launch (any key dismisses; **H** reopens). For a Windows 64-bit build, see [docs/BUILD.md](docs/BUILD.md).

## AI協働開発について

本プロジェクト全体（ゲームリポジトリ・Core リポジトリ・ドキュメント・テスト・実装）は **AI 協働開発の実験** として位置づけています。

| 担当 | 内容 |
|------|------|
| **人間（作者）** | 最初の企画。設計相談中の **タスク細分化の粒度** の指示 |
| **AI** | 上記以外の **すべて**（設計判断、実装、テスト、レビュー、承認） |

- リポジトリ内の [`.cursor/rules`](.cursor/rules/) は、AI に渡す **指示ルール**（開発方針・制約）です。
- **現時点の限界（事実）**
  - レビューと承認も AI が行うため、**独立した人間による検証はない**。
  - 実機性能（FPS・メモリ）は **未測定**。
  - 演出・サウンドは **未実装**。

## プレイ画面

![プレイ画面](docs/images/screenshot.png)

<img width="960" height="556" alt="gameplay" src="https://github.com/user-attachments/assets/9c083a50-6618-4ac5-962f-b3b463b150cc" />

## 遊び方

| モード | 主な操作（`InputGuide.cs`） |
|--------|------------------------------|
| Recording | 移動: 矢印 / WASD、確定: **Enter**、リセット: **R** |
| Playback | 1 Tick 進行: **Space**、リセット: **R** |
| Cleared | リセット: **R** |

流れ: **記録 → Enter で確定 → Playback で Space を押して Specter を進める → 出口条件を満たしてクリア**。起動時に **How to play** オーバーレイ（英語）が表示され、任意のキーで閉じられます。**H** でいつでも開き直せます。ステージ切替: **N**（次） / **P**（前、端でクランプ）。

ルールの詳細は [DESIGN_SPEC.md](DESIGN_SPEC.md) と [docs/STAGE_GUIDE.md](docs/STAGE_GUIDE.md) を参照してください。

## ステージ（全5種）

| 名前 | 概要 |
|------|------|
| **WALK** | スイッチなし。記録のみでクリアできる導入（`StageCatalog` index 0） |
| **ECHO** | 1 スイッチ。Specter が離れた位置から再生 |
| **GATE** | スイッチと 1 ドア |
| **TWIN** | 2 スイッチと 2 ドア |
| **TANGLE** | 2 スイッチと AND 条件の 2 ドア |

解の手順は [docs/STAGE_GUIDE.md](docs/STAGE_GUIDE.md) に記載しています。

## 技術的な特徴

- **3 層:** Core（Data + Logic、Unity 非依存）と Presentation（入力・描画・UI）。Core は `noEngineReferences: true`（`SpecterTick.Core.asmdef`）。
- **UPM 参照:** `Packages/manifest.json` の `file:../../SpecterTick.Core`（兄弟フォルダ配置）。詳細は [docs/BUILD.md](docs/BUILD.md)。
- **決定性:** 同じ初期状態と同じコマンド列で同じ結果になるよう Core をテスト（例: `GridPuzzleEngineSessionDeterminismTests`）。
- **自動テスト:** Core / Presentation の EditMode および PlayMode（件数の数え方は [docs/BUILD.md](docs/BUILD.md) を参照）。

## セットアップとビルド

1. `SpecterTick` と `SpecterTick.Core` を同じ親フォルダに並べる。
2. Unity **6000.3.26f1** で `SpecterTick` を開く。
3. テスト・CLI ビルド手順: [docs/BUILD.md](docs/BUILD.md)

リポジトリ URL: `https://github.com/yuya-hatano-abs/SpecterTick.git`

## リポジトリ構成（主要パス）

```text
SpecterTick/
  Assets/
    Scenes/SampleScene.unity
    SpecterTick/Presentation/
      Runtime/
      Tests/Editor/
      Tests/PlayMode/
      Resources/Skin/
  Packages/manifest.json
  ProjectSettings/
  ArtSource/
  DESIGN_SPEC.md
  docs/
SpecterTick.Core/       … 別リポジトリ（UPM）
```

## 既知の制限（要点）

- 演出・サウンドなし。仕様 MVP の EditorWindow デバッグツールは未実装。
- 図形描画時は Player / Specter の区別が弱い（輝度コントラスト比約 **1.13**）。スキン PNG 使用時は輝度比 **1.5 以上** をテストで要求。
- FPS / メモリは未測定。AI 協働のため独立した人間レビューはない。

詳細: [docs/KNOWN_LIMITATIONS.md](docs/KNOWN_LIMITATIONS.md)

## 素材とライセンス

- **ソースコード:** [LICENSE](LICENSE)（MIT）。著作権表示は Core の `LICENSE` と同じ文言です。
- **画像素材**（`Assets/SpecterTick/Presentation/Resources/Skin/`、`ArtSource/`）: Grok で生成した画像を加工したもの。**MIT の対象外**です。**再配布・再利用は想定していません。**

## ドキュメント

| ファイル | 内容 |
|----------|------|
| [DESIGN_SPEC.md](DESIGN_SPEC.md) | ゲーム仕様・設計（§7 まで）+ 実装対応（末尾追記） |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | アーキテクチャとテスト方針 |
| [docs/BUILD.md](docs/BUILD.md) | セットアップ・テスト・ビルド |
| [docs/STAGE_GUIDE.md](docs/STAGE_GUIDE.md) | ステージと解 |
| [docs/KNOWN_LIMITATIONS.md](docs/KNOWN_LIMITATIONS.md) | 既知の制限 |
| [CHANGELOG.md](CHANGELOG.md) | 変更履歴 |

## Core パッケージ

ロジック本体は `SpecterTick.Core`（`https://github.com/yuya-hatano-abs/SpecterTick.Core.git`）。
