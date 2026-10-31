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
        public IEnumerator PM11_FitCamera_FollowsStage()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;
            var keyboard = Keyboard.current;

            Assert.That(_camera.transform.position.x, Is.EqualTo(1.5f).Within(0.0001f));
            Assert.That(_camera.transform.position.y, Is.EqualTo(1.0f).Within(0.0001f));
            Assert.That(_camera.transform.position.z, Is.EqualTo(-10f).Within(0.0001f));

            yield return Tap(keyboard.nKey);
            Assert.That(_camera.transform.position.x, Is.EqualTo(2.0f).Within(0.0001f));
            Assert.That(_camera.transform.position.y, Is.EqualTo(1.0f).Within(0.0001f));

            yield return Tap(keyboard.nKey);
            yield return Tap(keyboard.nKey);
            yield return Tap(keyboard.nKey);
            Assert.That(_camera.transform.position.x, Is.EqualTo(2.5f).Within(0.0001f));
            Assert.That(_camera.transform.position.y, Is.EqualTo(1.5f).Within(0.0001f));

            yield return Tap(keyboard.pKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 4 / 5  TWIN"));
            Assert.That(_camera.transform.position.x, Is.EqualTo(2.5f).Within(0.0001f));
            Assert.That(_camera.transform.position.y, Is.EqualTo(1.5f).Within(0.0001f));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
