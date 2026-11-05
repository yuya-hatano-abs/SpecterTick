# ビルドとテスト

本ドキュメントは、ゲームプロジェクトを開き、自動テストを実行し、Windows 64 ビットスタンドアロンを出力する手順を説明します。**Windows** 上で **Unity 6000.3.26f1** を使い、作業用コピー（兄弟 Core 配置）での batchmode 実行を想定しています。ここで記載している検証対象は **Windows 64 ビット Mono** スタンドアロンの手順のみです。本リポジトリは [AI 協働開発の実験](../README.md#ai協働開発について) の一部です。

## 前提

- Unity Hub にエディタ **6000.3.26f1** をインストール済み。
- 次の 2 フォルダを同じ親の下に並べる（`manifest.json` の既定パスに合わせる）:

```text
C:\Work\
  SpecterTick/          ← ゲーム（本プロジェクト）
  SpecterTick.Core/     ← Core UPM パッケージ
```

`Packages/manifest.json` の参照:

```json
"com.yuya_hatano.specter-tick-core": "file:../../SpecterTick.Core"
```

`Packages/` から見た `../../SpecterTick.Core` は `C:\Work\SpecterTick.Core` に解決されます。

Core の Git リモート URL（参照用）: `https://github.com/yuya-hatano-abs/SpecterTick.Core.git` — クローンする場合。ゲーム側の manifest は上記のローカル `file:` 配置を前提とします。

**testables:** ゲームの `manifest.json` に `com.yuya_hatano.specter-tick-core` を `testables` に含めないと、Core の EditMode テストは走らず **0 件** になる（終了コードが 0 のままの場合あり）。

## batchmode と作業用コピー

**実プロジェクトのルートを `-projectPath` に向けると、Unity が `Library/` や `.meta` を書き換えます。** 本ドキュメントの CLI 手順は、リポジトリ本体を汚さないよう **作業用コピー** に対して実行することを想定しています。

### 作業用コピーの作り方

PowerShell で（パスは環境に合わせて変更する）。

**`/MIR` はコピー先にあってソースにないファイルを削除します。** `$SrcGame` / `$SrcCore` と `$Game` / `$Core` が、実リポジトリそのものを指していないことを確認してから実行してください。

```powershell
# 例: 実リポジトリの場所（環境に合わせて変更する）
$SrcGame = "D:\repos\SpecterTick"
$SrcCore = "D:\repos\SpecterTick.Core"

# 作業用（環境に合わせて変更する）
$Game = "C:\Work\SpecterTick"
$Core = "C:\Work\SpecterTick.Core"

New-Item -ItemType Directory -Force -Path (Split-Path $Game), (Split-Path $Core) | Out-Null

robocopy $SrcGame $Game /MIR /XD Library Temp Logs UserSettings obj .git /NFL /NDL /NJH /NJS
robocopy $SrcCore $Core /MIR /XD Library Temp Logs UserSettings obj .git /NFL /NDL /NJH /NJS
```

`$Game` と `$Core` は **兄弟**（同じ親の下）に置き、`manifest.json` の `file:../../SpecterTick.Core` が `$Core` を指すようにします。

## シェル変数

batch コマンドの前に PowerShell で設定（**例の値。環境に合わせて変更する**）:

```powershell
$Unity = "C:\Program Files\Unity\Hub\Editor\6000.3.26f1\Editor\Unity.exe"
$Game  = "C:\Work\SpecterTick"
$Core  = "C:\Work\SpecterTick.Core"
$Out   = "C:\Work\SpecterTick-Out"
```

テスト結果 XML とログは `$Out` に出力します。ビルド前にも `$Out` を作成します。

```powershell
New-Item -ItemType Directory -Force -Path $Out | Out-Null
```

`$Game`・`$Core`・`$Out` は、いずれも **空白を含まないパスを推奨**します（例: `C:\Work\SpecterTick` / `C:\Work\SpecterTick.Core` / `C:\Work\SpecterTick-Out`）。`Start-Process` の `-ArgumentList` に `$results` / `$log` をそのまま渡す形は、`$Out` が空白なしのときに検証済みです。

## エディタで開く

1. `$Game` と `$Core` を兄弟配置（上記）。
2. Unity **6000.3.26f1** で `$Game` を開く。
3. `Assets/Scenes/SampleScene.unity` を再生（`EditorBuildSettings` に登録済み）。

## 自動テスト（コマンドライン）

`-runTests` では **`-quit` を付けない**（Unity がテスト後に終了する）。結果 XML の **total** 件数を必ず確認する。

### EditMode

```powershell
New-Item -ItemType Directory -Force -Path $Out | Out-Null
$results = Join-Path $Out 'editmode-results.xml'
$log     = Join-Path $Out 'editmode.log'
$p = Start-Process -FilePath $Unity -PassThru -Wait -ArgumentList @(
  "-batchmode","-nographics",
  "-projectPath",$Game,
  "-runTests","-testPlatform","EditMode",
  "-assemblyNames","SpecterTick.Core.Tests;SpecterTick.Presentation.Tests",
  "-testResults",$results,
  "-logFile",$log
)
$p.ExitCode
```

結果 XML の **`total`** を確認する（正の件数）。リポジトリ内の `[Test]` / `[UnityTest]` 属性数（2026-11-05 時点）: `SpecterTick.Core.Tests` **209** + `SpecterTick.Presentation.Tests` **206** = **415**（Core の `[TestCase]` / `[TestCaseSource]` により Unity 実行件数はこれより多い）。

### PlayMode（19 件 — `[Test]` / `[UnityTest]` 属性数）

```powershell
New-Item -ItemType Directory -Force -Path $Out | Out-Null
$results = Join-Path $Out 'playmode-results.xml'
$log     = Join-Path $Out 'playmode.log'
$p = Start-Process -FilePath $Unity -PassThru -Wait -ArgumentList @(
  "-batchmode","-nographics",
  "-projectPath",$Game,
  "-runTests","-testPlatform","PlayMode",
  "-assemblyNames","SpecterTick.Presentation.PlayModeTests",
  "-testResults",$results,
  "-logFile",$log
)
$p.ExitCode
```

結果 XML の **`total`** を確認する。属性数（2026-11-05 時点）: **19**（`SpecterTick.Presentation.PlayModeTests`）。

### Unity Test Runner（GUI）

- **Window → General → Test Runner**
- EditMode: `SpecterTick.Core.Tests` と `SpecterTick.Presentation.Tests`
- PlayMode: `SpecterTick.Presentation.PlayModeTests`

## スタンドアロンビルド（Windows 64 ビット）

ビルドシーン（`EditorBuildSettings`）: `Assets/Scenes/SampleScene.unity`（有効シーン 1 件）。

```powershell
New-Item -ItemType Directory -Force -Path $Out | Out-Null
$p = Start-Process -FilePath $Unity -PassThru -Wait -ArgumentList @(
  "-batchmode","-nographics","-quit",
  "-projectPath",$Game,
  "-buildWindows64Player","$(Join-Path $Out 'SPECTER-TICK.exe')",
  "-logFile","$(Join-Path $Out 'build.log')"
)
$p.ExitCode
Test-Path (Join-Path $Out 'SPECTER-TICK.exe')
```

検証時の目安: `Library/` のないコピーでも約 **90 秒** 程度で完了、出力フォルダは約 **93 MB** 程度。Managed に `*Tests*.dll` が含まれないこと。

## 検証済みの範囲

- OS: Windows 64 ビット
- スクリプティング: Mono（本プロジェクトの Unity 既定）
- 本ドキュメントで未検証: 他プラットフォーム、IL2CPP のみの手順、Core を Git URL のみで入れる構成（ゲームは `file:` 前提）

## トラブルシュート

| 症状 | 想定原因 |
|------|----------|
| EditMode **0** 件 | `manifest.json` の `testables` に Core がない |
| PlayMode **0** 件 / XML なし | Core の兄弟パスが誤り |
| 件数が想定と違う | `-assemblyNames` または `-projectPath` の誤り |
| 本番リポジトリが書き換わった | `-projectPath` が作業用コピーではない |
| テストで `-quit` を付けた | `-runTests` では `-quit` を省略 |

## 関連ドキュメント

- [アーキテクチャ](ARCHITECTURE.md)
- [既知の制限](KNOWN_LIMITATIONS.md)
- [ステージガイド](STAGE_GUIDE.md)
- [README](../README.md)
