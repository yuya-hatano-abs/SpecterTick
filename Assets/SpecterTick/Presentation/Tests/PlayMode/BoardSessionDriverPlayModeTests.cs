using System.Collections;
using NUnit.Framework;
using SpecterTick.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace SpecterTick.Presentation.PlayModeTests
{
    /// <summary>
    /// PlayMode regression for BoardSessionDriver input, mode flow, and UI text/counts.
    /// Does not guarantee visuals, window resize, built-player input, or focus behavior.
    /// </summary>
    public sealed class BoardSessionDriverPlayModeTests : InputTestFixture
    {
        private Camera _camera;
        private GameObject _board;
        private Vector3 _playerOrigin;
        private Vector3 _specterOrigin;

        public override void Setup()
        {
            base.Setup();
            _camera = PlayModeHarness.CreateMainCamera();
            _board = null;
        }

        public override void TearDown()
        {
            if (_board != null)
            {
                Object.DestroyImmediate(_board);
                _board = null;
            }

            if (_camera != null)
            {
                Object.DestroyImmediate(_camera.gameObject);
                _camera = null;
            }

            base.TearDown();
        }

        private IEnumerator Tap(KeyControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
            yield return null;
        }

        private IEnumerator TapRecording(string moves)
        {
            var keyboard = Keyboard.current;
            for (var i = 0; i < moves.Length; i++)
            {
                KeyControl key;
                switch (moves[i])
                {
                    case 'L':
                        key = keyboard.leftArrowKey;
                        break;
                    case 'R':
                        key = keyboard.rightArrowKey;
                        break;
                    case 'U':
                        key = keyboard.upArrowKey;
                        break;
                    case 'D':
                        key = keyboard.downArrowKey;
                        break;
                    default:
                        throw new System.InvalidOperationException("Unexpected recording character: " + moves[i]);
                }

                yield return Tap(key);
            }
        }

        private IEnumerator TapEchoSetup()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            var keyboard = Keyboard.current;
            yield return Tap(keyboard.nKey);
            RecordActorOrigins();
        }

        private void RecordActorOrigins()
        {
            var player = PlayModeHarness.FindActor(_board, "Player");
            var specter = PlayModeHarness.FindActor(_board, "Specter");
            Assert.That(player, Is.Not.Null);
            Assert.That(specter, Is.Not.Null);
            _playerOrigin = player.localPosition;
            _specterOrigin = specter.localPosition;
        }

        private void AssertPlayerDelta(float deltaX, float deltaY)
        {
            var player = PlayModeHarness.FindActor(_board, "Player");
            Assert.That(player, Is.Not.Null);
            Assert.That(player.localPosition.x, Is.EqualTo(_playerOrigin.x + deltaX).Within(0.0001f));
            Assert.That(player.localPosition.y, Is.EqualTo(_playerOrigin.y + deltaY).Within(0.0001f));
        }

        private void AssertSpecterDelta(float deltaX, float deltaY)
        {
            var specter = PlayModeHarness.FindActor(_board, "Specter");
            Assert.That(specter, Is.Not.Null);
            Assert.That(specter.localPosition.x, Is.EqualTo(_specterOrigin.x + deltaX).Within(0.0001f));
            Assert.That(specter.localPosition.y, Is.EqualTo(_specterOrigin.y + deltaY).Within(0.0001f));
        }

        private static string HudText(GameObject board)
        {
            return PlayModeHarness.CanvasText(board, 100);
        }

        private static string LabelText(GameObject board)
        {
            return PlayModeHarness.CanvasText(board, 95);
        }

        private static string ClearText(GameObject board)
        {
            return PlayModeHarness.CanvasText(board, 110);
        }

        private static void ExpectModeChangedLog(string modeName, int tick)
        {
            LogAssert.Expect(
                LogType.Log,
                "BoardSessionDriver: mode changed to "
                + modeName
                + " (tick="
                + tick
                + ").");
        }

        private static void ExpectRejectedLog(string intentName, string modeName)
        {
            LogAssert.Expect(
                LogType.Log,
                "BoardSessionDriver: input rejected by mode (intent="
                + intentName
                + ", mode="
                + modeName
                + ").");
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM1_Start_BuildsBoardAndUi()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;

            Assert.That(PlayModeHarness.ChildCount(_board, "Floor"), Is.EqualTo(12));
            Assert.That(PlayModeHarness.ChildCount(_board, "Walls"), Is.EqualTo(2));
            Assert.That(PlayModeHarness.ChildCount(_board, "Actors"), Is.EqualTo(2));
            Assert.That(PlayModeHarness.ChildCount(_board, "Elements"), Is.EqualTo(1));

            var hud = HudText(_board);
            Assert.That(hud, Does.Contain("MODE: RECORDING"));
            Assert.That(hud, Does.Contain("TICK: 0"));
            Assert.That(hud, Does.Contain("RECORDED: 0"));

            var label = LabelText(_board);
            Assert.That(label, Does.Contain("STAGE 1 / 5  WALK"));

            Assert.That(PlayModeHarness.IsClearScreenVisible(_board), Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM2_Walk_SolutionToCleared()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            RecordActorOrigins();

            var keyboard = Keyboard.current;
            yield return Tap(keyboard.upArrowKey);
            AssertPlayerDelta(0f, 1f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            Assert.That(HudText(_board), Does.Contain("MODE: RECORDING"));

            yield return Tap(keyboard.upArrowKey);
            AssertPlayerDelta(0f, 2f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 2"));

            yield return Tap(keyboard.rightArrowKey);
            AssertPlayerDelta(1f, 2f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 3"));

            yield return Tap(keyboard.rightArrowKey);
            AssertPlayerDelta(2f, 2f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 4"));

            yield return Tap(keyboard.rightArrowKey);
            AssertPlayerDelta(3f, 2f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 5"));

            ExpectModeChangedLog("Cleared", 0);
            yield return Tap(keyboard.enterKey);
            Assert.That(HudText(_board), Does.Contain("MODE: CLEARED"));
            AssertPlayerDelta(3f, 2f);
            Assert.That(PlayModeHarness.IsClearScreenVisible(_board), Is.True);
            var clear = ClearText(_board);
            Assert.That(clear, Does.Contain("CLEARED!"));
            Assert.That(clear, Does.Contain("TICK: 0  RECORDED: 5"));
            Assert.That(clear, Does.Contain("PRESS R TO RETRY"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM3_Echo_RecordConfirmPlaybackClear()
        {
            yield return TapEchoSetup();
            Assert.That(LabelText(_board), Does.Contain("STAGE 2 / 5  ECHO"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));

            var keyboard = Keyboard.current;
            yield return Tap(keyboard.rightArrowKey);
            AssertPlayerDelta(1f, 0f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));

            yield return Tap(keyboard.rightArrowKey);
            AssertPlayerDelta(2f, 0f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 2"));

            yield return Tap(keyboard.rightArrowKey);
            AssertPlayerDelta(3f, 0f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 3"));

            yield return Tap(keyboard.rightArrowKey);
            AssertPlayerDelta(4f, 0f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 4"));

            ExpectModeChangedLog("Playback", 0);
            yield return Tap(keyboard.enterKey);
            Assert.That(HudText(_board), Does.Contain("MODE: PLAYBACK"));
            Assert.That(HudText(_board), Does.Contain("PLAYBACK: 0 / 4"));
            AssertPlayerDelta(4f, 0f);
            AssertSpecterDelta(0f, 0f);

            yield return Tap(keyboard.spaceKey);
            AssertSpecterDelta(1f, 0f);
            Assert.That(HudText(_board), Does.Contain("PLAYBACK: 1 / 4"));
            Assert.That(HudText(_board), Does.Contain("TICK: 1"));

            yield return Tap(keyboard.spaceKey);
            AssertSpecterDelta(2f, 0f);
            Assert.That(HudText(_board), Does.Contain("PLAYBACK: 2 / 4"));

            ExpectModeChangedLog("Cleared", 3);
            yield return Tap(keyboard.spaceKey);
            Assert.That(HudText(_board), Does.Contain("MODE: CLEARED"));
            Assert.That(PlayModeHarness.IsClearScreenVisible(_board), Is.True);
            Assert.That(ClearText(_board), Does.Contain("TICK: 3  RECORDED: 4"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM4_Retry_FromAllModes()
        {
            yield return TapEchoSetup();
            var keyboard = Keyboard.current;

            yield return Tap(keyboard.rightArrowKey);
            AssertPlayerDelta(1f, 0f);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            yield return Tap(keyboard.rKey);
            AssertPlayerDelta(0f, 0f);
            Assert.That(HudText(_board), Does.Contain("MODE: RECORDING"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));
            Assert.That(HudText(_board), Does.Contain("TICK: 0"));

            for (var i = 0; i < 4; i++)
            {
                yield return Tap(keyboard.rightArrowKey);
            }

            ExpectModeChangedLog("Playback", 0);
            yield return Tap(keyboard.enterKey);
            yield return Tap(keyboard.spaceKey);
            AssertSpecterDelta(1f, 0f);
            Assert.That(HudText(_board), Does.Contain("PLAYBACK: 1 / 4"));
            ExpectModeChangedLog("Recording", 0);
            yield return Tap(keyboard.rKey);
            AssertSpecterDelta(0f, 0f);
            AssertPlayerDelta(0f, 0f);
            Assert.That(HudText(_board), Does.Contain("MODE: RECORDING"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));

            for (var i = 0; i < 4; i++)
            {
                yield return Tap(keyboard.rightArrowKey);
            }

            ExpectModeChangedLog("Playback", 0);
            yield return Tap(keyboard.enterKey);
            yield return Tap(keyboard.spaceKey);
            yield return Tap(keyboard.spaceKey);
            ExpectModeChangedLog("Cleared", 3);
            yield return Tap(keyboard.spaceKey);
            Assert.That(PlayModeHarness.IsClearScreenVisible(_board), Is.True);
            Assert.That(HudText(_board), Does.Contain("MODE: CLEARED"));
            ExpectModeChangedLog("Recording", 0);
            yield return Tap(keyboard.rKey);
            Assert.That(PlayModeHarness.IsClearScreenVisible(_board), Is.False);
            Assert.That(HudText(_board), Does.Contain("MODE: RECORDING"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));
            AssertPlayerDelta(0f, 0f);
            AssertSpecterDelta(0f, 0f);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM5_ModeGuard_RejectedInputsKeepState()
        {
            yield return TapEchoSetup();
            using (var logs = new PlayModeHarness.LogCapture())
            {
                var keyboard = Keyboard.current;
                ExpectRejectedLog("StepForward", "Recording");
                yield return Tap(keyboard.spaceKey);
                yield return Tap(keyboard.rightArrowKey);
                yield return Tap(keyboard.rightArrowKey);
                yield return Tap(keyboard.rightArrowKey);
                yield return Tap(keyboard.rightArrowKey);
                AssertPlayerDelta(4f, 0f);
                ExpectModeChangedLog("Playback", 0);
                yield return Tap(keyboard.enterKey);
                var hudBeforeReject = HudText(_board);
                ExpectRejectedLog("Move", "Playback");
                yield return Tap(keyboard.rightArrowKey);
                AssertPlayerDelta(4f, 0f);
                Assert.That(HudText(_board), Is.EqualTo(hudBeforeReject));
                Assert.That(HudText(_board), Does.Contain("PLAYBACK: 0 / 4"));
                ExpectRejectedLog("ConfirmRecording", "Playback");
                yield return Tap(keyboard.enterKey);
                Assert.That(HudText(_board), Is.EqualTo(hudBeforeReject));
                yield return Tap(keyboard.spaceKey);
                yield return Tap(keyboard.spaceKey);
                ExpectModeChangedLog("Cleared", 3);
                yield return Tap(keyboard.spaceKey);
                var hudCleared = HudText(_board);
                Assert.That(hudCleared, Does.Contain("MODE: CLEARED"));
                ExpectRejectedLog("StepForward", "Cleared");
                yield return Tap(keyboard.spaceKey);
                ExpectRejectedLog("Move", "Cleared");
                yield return Tap(keyboard.rightArrowKey);
                ExpectRejectedLog("ConfirmRecording", "Cleared");
                yield return Tap(keyboard.enterKey);
                Assert.That(HudText(_board), Is.EqualTo(hudCleared));

                Assert.That(
                    logs.CountExact(
                        "BoardSessionDriver: input rejected by mode (intent=StepForward, mode=Recording)."),
                    Is.EqualTo(1));
                Assert.That(
                    logs.CountExact(
                        "BoardSessionDriver: input rejected by mode (intent=Move, mode=Playback)."),
                    Is.EqualTo(1));
                Assert.That(
                    logs.CountExact(
                        "BoardSessionDriver: input rejected by mode (intent=ConfirmRecording, mode=Playback)."),
                    Is.EqualTo(1));
                Assert.That(
                    logs.CountExact(
                        "BoardSessionDriver: input rejected by mode (intent=StepForward, mode=Cleared)."),
                    Is.EqualTo(1));
                Assert.That(
                    logs.CountExact(
                        "BoardSessionDriver: input rejected by mode (intent=Move, mode=Cleared)."),
                    Is.EqualTo(1));
                Assert.That(
                    logs.CountExact(
                        "BoardSessionDriver: input rejected by mode (intent=ConfirmRecording, mode=Cleared)."),
                    Is.EqualTo(1));
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM6_Navigation_ClampAndKeepProgress()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            RecordActorOrigins();
            var keyboard = Keyboard.current;

            yield return Tap(keyboard.rightArrowKey);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            var playerTransform = PlayModeHarness.FindActor(_board, "Player");
            yield return Tap(keyboard.pKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 1 / 5  WALK"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            Assert.That(playerTransform == null, Is.False);

            yield return Tap(keyboard.nKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 2 / 5  ECHO"));
            Assert.That(PlayModeHarness.ChildCount(_board, "Floor"), Is.EqualTo(15));
            Assert.That(PlayModeHarness.ChildCount(_board, "Elements"), Is.EqualTo(2));

            yield return Tap(keyboard.nKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 3 / 5  GATE"));
            Assert.That(PlayModeHarness.ChildCount(_board, "Floor"), Is.EqualTo(24));
            Assert.That(PlayModeHarness.ChildCount(_board, "Elements"), Is.EqualTo(3));

            yield return Tap(keyboard.nKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 4 / 5  TWIN"));
            Assert.That(PlayModeHarness.ChildCount(_board, "Floor"), Is.EqualTo(24));
            Assert.That(PlayModeHarness.ChildCount(_board, "Elements"), Is.EqualTo(5));

            yield return Tap(keyboard.nKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 5 / 5  TANGLE"));
            Assert.That(PlayModeHarness.ChildCount(_board, "Floor"), Is.EqualTo(24));
            Assert.That(PlayModeHarness.ChildCount(_board, "Elements"), Is.EqualTo(5));

            playerTransform = PlayModeHarness.FindActor(_board, "Player");
            yield return Tap(keyboard.rightArrowKey);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            var playerAtEnd = PlayModeHarness.FindActor(_board, "Player");
            yield return Tap(keyboard.nKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 5 / 5  TANGLE"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            Assert.That(playerAtEnd == null, Is.False);

            yield return Tap(keyboard.pKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 4 / 5  TWIN"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM7_StageSwitch_ResetsProgressAndClearScreen()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            RecordActorOrigins();
            var initialPlayerPosition = PlayModeHarness.FindActor(_board, "Player").localPosition;
            var keyboard = Keyboard.current;

            yield return Tap(keyboard.upArrowKey);
            yield return Tap(keyboard.upArrowKey);
            yield return Tap(keyboard.rightArrowKey);
            yield return Tap(keyboard.rightArrowKey);
            yield return Tap(keyboard.rightArrowKey);
            ExpectModeChangedLog("Cleared", 0);
            yield return Tap(keyboard.enterKey);
            Assert.That(HudText(_board), Does.Contain("MODE: CLEARED"));
            Assert.That(PlayModeHarness.IsClearScreenVisible(_board), Is.True);

            yield return Tap(keyboard.nKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 2 / 5  ECHO"));
            Assert.That(HudText(_board), Does.Contain("MODE: RECORDING"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));
            Assert.That(PlayModeHarness.IsClearScreenVisible(_board), Is.False);

            yield return Tap(keyboard.rightArrowKey);
            yield return Tap(keyboard.pKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 1 / 5  WALK"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));
            var player = PlayModeHarness.FindActor(_board, "Player");
            Assert.That(player.localPosition.x, Is.EqualTo(initialPlayerPosition.x).Within(0.0001f));
            Assert.That(player.localPosition.y, Is.EqualTo(initialPlayerPosition.y).Within(0.0001f));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM8_NavigationFrame_ConsumesOtherKeys()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            var keyboard = Keyboard.current;
            Press(keyboard.nKey);
            Press(keyboard.rightArrowKey);
            yield return null;
            Release(keyboard.nKey);
            Release(keyboard.rightArrowKey);
            yield return null;
            yield return null;

            Assert.That(LabelText(_board), Does.Contain("STAGE 2 / 5  ECHO"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));
            Assert.That(HudText(_board), Does.Contain("MODE: RECORDING"));

            yield return Tap(keyboard.rightArrowKey);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM9_NoKeyboard_WarnsOnce()
        {
            Assert.That(Keyboard.current == null, Is.True);
            LogAssert.Expect(
                LogType.Warning,
                "BoardSessionDriver: Keyboard.current is null; input is ignored.");
            using (var logs = new PlayModeHarness.LogCapture())
            {
                _board = PlayModeHarness.CreateBoard();
                for (var i = 0; i < 5; i++)
                {
                    yield return null;
                }

                Assert.That(
                    logs.CountExact("BoardSessionDriver: Keyboard.current is null; input is ignored."),
                    Is.EqualTo(1));
                Assert.That(HudText(_board), Does.Contain("MODE: RECORDING"));
            }

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM10_Destroy_ReleasesSprites()
        {
            yield return null;
            yield return null;
            var n0 = Resources.FindObjectsOfTypeAll<Sprite>().Length;

            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            var n1 = Resources.FindObjectsOfTypeAll<Sprite>().Length;
            Assert.That(n1, Is.GreaterThan(n0));

            Object.Destroy(_board);
            _board = null;
            yield return null;
            yield return null;
            var n2 = Resources.FindObjectsOfTypeAll<Sprite>().Length;
            Assert.That(n2, Is.EqualTo(n0));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM11_FitCamera_FitsInsetBandAcrossStages()
        {
            // Viewport band checks use BoardScreenInsets.Compute at the actual Screen size (Editor Game view or batchmode).
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            var keyboard = Keyboard.current;

            AssertFitCameraFitsInsetBand("WALK", 1.5f);

            yield return Tap(keyboard.nKey);
            AssertFitCameraFitsInsetBand("ECHO", 2.0f);

            yield return Tap(keyboard.nKey);
            yield return Tap(keyboard.nKey);
            yield return Tap(keyboard.nKey);
            AssertFitCameraFitsInsetBand("TANGLE", 2.5f);

            yield return Tap(keyboard.pKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 4 / 5  TWIN"));
            AssertFitCameraFitsInsetBand("TWIN", 2.5f);
            LogAssert.NoUnexpectedReceived();
        }

        private void AssertFitCameraFitsInsetBand(string stageLabel, float expectedCameraX)
        {
            var screenWidth = Mathf.Max(1, Screen.width);
            var screenHeight = Mathf.Max(1, Screen.height);
            var insets = BoardScreenInsets.Compute(screenWidth, screenHeight);
            var top = insets.Top;
            var bottom = insets.Bottom;
            var bandCenterY = bottom + (1f - top - bottom) / 2f;
            var viewportTopLimit = 1f - top;

            Assert.That(_camera.transform.position.x, Is.EqualTo(expectedCameraX).Within(0.0001f));
            Assert.That(_camera.transform.position.z, Is.EqualTo(-10f).Within(0.0001f));

            var prefix = stageLabel
                + " Screen="
                + screenWidth
                + "x"
                + screenHeight
                + " Top="
                + top.ToString("F6")
                + " Bottom="
                + bottom.ToString("F6")
                + " bandCenterY="
                + bandCenterY.ToString("F6");

            Assert.That(
                PlayModeHarness.TryGetFloorViewportBounds(_board, _camera, out var floor),
                Is.True,
                prefix + " floor viewport bounds missing");

            Assert.That(
                floor.MinY,
                Is.GreaterThanOrEqualTo(bottom - 0.001f),
                prefix
                    + " floorMinY="
                    + floor.MinY.ToString("F6")
                    + " expected>="
                    + bottom.ToString("F6"));

            Assert.That(
                floor.MaxY,
                Is.LessThanOrEqualTo(viewportTopLimit + 0.001f),
                prefix
                    + " floorMaxY="
                    + floor.MaxY.ToString("F6")
                    + " expected<="
                    + viewportTopLimit.ToString("F6"));

            Assert.That(
                floor.CenterY,
                Is.EqualTo(bandCenterY).Within(0.001f),
                prefix
                    + " floorCenterY="
                    + floor.CenterY.ToString("F6"));

            Assert.That(
                floor.CenterX,
                Is.EqualTo(0.5f).Within(0.001f),
                prefix
                    + " floorCenterX="
                    + floor.CenterX.ToString("F6"));
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PM12_Panels_DoNotOverlapBoard()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            var keyboard = Keyboard.current;

            AssertNoPanelOverlap("WALK");

            yield return Tap(keyboard.nKey);
            AssertNoPanelOverlap("ECHO");

            yield return Tap(keyboard.nKey);
            AssertNoPanelOverlap("GATE");

            yield return Tap(keyboard.nKey);
            AssertNoPanelOverlap("TWIN");

            yield return Tap(keyboard.nKey);
            AssertNoPanelOverlap("TANGLE");

            yield return Tap(keyboard.pKey);
            yield return Tap(keyboard.pKey);
            yield return Tap(keyboard.pKey);
            yield return Tap(keyboard.pKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 1 / 5  WALK"));

            yield return Tap(keyboard.upArrowKey);
            yield return Tap(keyboard.upArrowKey);
            yield return Tap(keyboard.rightArrowKey);
            yield return Tap(keyboard.rightArrowKey);
            yield return Tap(keyboard.rightArrowKey);
            yield return Tap(keyboard.rightArrowKey);
            ExpectModeChangedLog("Cleared", 0);
            yield return Tap(keyboard.enterKey);
            yield return null;
            yield return null;

            AssertNoPanelOverlap("WALK cleared", includeClearPanel: true);

            for (var stageIndex = 0; stageIndex < 4; stageIndex++)
            {
                yield return Tap(keyboard.nKey);
            }

            Assert.That(LabelText(_board), Does.Contain("STAGE 5 / 5  TANGLE"));
            yield return TapRecording("LLURURRLUURR");
            ExpectModeChangedLog("Playback", 0);
            yield return Tap(keyboard.enterKey);
            for (var step = 0; step < 11; step++)
            {
                yield return Tap(keyboard.spaceKey);
            }

            ExpectModeChangedLog("Cleared", 12);
            yield return Tap(keyboard.spaceKey);
            yield return null;
            yield return null;

            AssertNoPanelOverlap("TANGLE cleared", includeClearPanel: true);
            LogAssert.NoUnexpectedReceived();
        }

        private void AssertNoPanelOverlap(string stageLabel, bool includeClearPanel = false)
        {
            Canvas.ForceUpdateCanvases();
            Assert.That(
                PlayModeHarness.TryGetFloorScreenBounds(_board, _camera, out var floor),
                Is.True,
                stageLabel + ": floor bounds missing");

            Assert.That(
                PlayModeHarness.TryGetPanelScreenRect(_board, 100, out var hud),
                Is.True,
                stageLabel + ": HUD panel missing");
            Assert.That(
                PlayModeHarness.ScreenRectsOverlap(floor, hud),
                Is.False,
                stageLabel
                    + ": floor overlaps HUD "
                    + PlayModeHarness.FormatScreenRect(floor)
                    + " vs "
                    + PlayModeHarness.FormatScreenRect(hud));

            Assert.That(
                PlayModeHarness.TryGetPanelScreenRect(_board, 95, out var label),
                Is.True,
                stageLabel + ": stage label missing");
            Assert.That(
                PlayModeHarness.ScreenRectsOverlap(floor, label),
                Is.False,
                stageLabel
                    + ": floor overlaps label "
                    + PlayModeHarness.FormatScreenRect(floor)
                    + " vs "
                    + PlayModeHarness.FormatScreenRect(label));

            if (!includeClearPanel)
            {
                return;
            }

            Assert.That(
                PlayModeHarness.TryGetPanelScreenRect(_board, 110, out var clear),
                Is.True,
                stageLabel + ": clear panel missing");
            Assert.That(
                PlayModeHarness.ScreenRectsOverlap(floor, clear),
                Is.False,
                stageLabel
                    + ": floor overlaps clear "
                    + PlayModeHarness.FormatScreenRect(floor)
                    + " vs "
                    + PlayModeHarness.FormatScreenRect(clear));
        }
    }
}
