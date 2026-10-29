using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies input guide strings, key assignments, and mode guards against dispatcher behavior.
    /// Does not guarantee on-screen HUD layout or live key rebinding.
    /// </summary>
    [TestFixture]
    public sealed class InputGuideTests
    {
        [Test]
        public void IG1_KeysLineFor_Modes()
        {
            Assert.That(
                InputGuide.KeysLineFor(GameMode.Recording),
                Is.EqualTo("MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R"));
            Assert.That(
                InputGuide.KeysLineFor(GameMode.Playback),
                Is.EqualTo("STEP: SPACE  RESET: R"));
            Assert.That(
                InputGuide.KeysLineFor(GameMode.Cleared),
                Is.EqualTo("RESET: R"));
        }

        [Test]
        public void IG2_KeysLineFor_InvalidMode()
        {
            var invalid99 = Assert.Throws<ArgumentOutOfRangeException>(() => InputGuide.KeysLineFor((GameMode)99));
            Assert.That(invalid99.ParamName, Is.EqualTo("mode"));

            var invalid0 = Assert.Throws<ArgumentOutOfRangeException>(() => InputGuide.KeysLineFor((GameMode)0));
            Assert.That(invalid0.ParamName, Is.EqualTo("mode"));
        }

        [Test]
        public void IG3_IsAllowed_Matrix()
        {
            Assert.That(InputGuide.IsAllowed(InputIntentKind.Move, GameMode.Recording), Is.True);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.Move, GameMode.Playback), Is.False);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.Move, GameMode.Cleared), Is.False);

            Assert.That(InputGuide.IsAllowed(InputIntentKind.ConfirmRecording, GameMode.Recording), Is.True);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.ConfirmRecording, GameMode.Playback), Is.False);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.ConfirmRecording, GameMode.Cleared), Is.False);

            Assert.That(InputGuide.IsAllowed(InputIntentKind.StepForward, GameMode.Recording), Is.False);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.StepForward, GameMode.Playback), Is.True);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.StepForward, GameMode.Cleared), Is.False);

            Assert.That(InputGuide.IsAllowed(InputIntentKind.Retry, GameMode.Recording), Is.True);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.Retry, GameMode.Playback), Is.True);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.Retry, GameMode.Cleared), Is.True);

            Assert.That(InputGuide.IsAllowed(InputIntentKind.None, GameMode.Recording), Is.False);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.None, GameMode.Playback), Is.False);
            Assert.That(InputGuide.IsAllowed(InputIntentKind.None, GameMode.Cleared), Is.False);

            var kindException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputGuide.IsAllowed((InputIntentKind)99, GameMode.Recording));
            Assert.That(kindException.ParamName, Is.EqualTo("kind"));

            var modeException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputGuide.IsAllowed(InputIntentKind.Move, (GameMode)99));
            Assert.That(modeException.ParamName, Is.EqualTo("mode"));

            var orderException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                InputGuide.IsAllowed((InputIntentKind)99, (GameMode)99));
            Assert.That(orderException.ParamName, Is.EqualTo("kind"));
        }

        [Test]
        public void IG4_Guard_MatchesDispatcher()
        {
            AssertGuardCell(GameMode.Recording, InputIntent.Move(MoveDirection.Up), InputResult.Executed);
            AssertGuardCell(GameMode.Recording, InputIntent.ConfirmRecording, InputResult.Executed);
            AssertGuardCell(GameMode.Recording, InputIntent.StepForward, InputResult.RejectedByMode);
            AssertGuardCell(GameMode.Recording, InputIntent.Retry, InputResult.Executed);

            AssertGuardCell(GameMode.Playback, InputIntent.Move(MoveDirection.Up), InputResult.RejectedByMode);
            AssertGuardCell(GameMode.Playback, InputIntent.ConfirmRecording, InputResult.RejectedByMode);
            AssertGuardCell(GameMode.Playback, InputIntent.StepForward, InputResult.Executed);
            AssertGuardCell(GameMode.Playback, InputIntent.Retry, InputResult.Executed);

            AssertGuardCell(GameMode.Cleared, InputIntent.Move(MoveDirection.Up), InputResult.RejectedByMode);
            AssertGuardCell(GameMode.Cleared, InputIntent.ConfirmRecording, InputResult.RejectedByMode);
            AssertGuardCell(GameMode.Cleared, InputIntent.StepForward, InputResult.RejectedByMode);
            AssertGuardCell(GameMode.Cleared, InputIntent.Retry, InputResult.Executed);
        }

        [Test]
        public void IG5_Keys_ResolveToIntent()
        {
            Assert.That(
                InputGuide.GetKeys(InputIntentKind.Move),
                Is.EqualTo(new[]
                {
                    InputKeys.ArrowUp,
                    InputKeys.ArrowDown,
                    InputKeys.ArrowLeft,
                    InputKeys.ArrowRight,
                    InputKeys.KeyW,
                    InputKeys.KeyA,
                    InputKeys.KeyS,
                    InputKeys.KeyD
                }));

            Assert.That(InputGuide.GetKeys(InputIntentKind.ConfirmRecording), Is.EqualTo(new[] { InputKeys.Enter }));
            Assert.That(InputGuide.GetKeys(InputIntentKind.StepForward), Is.EqualTo(new[] { InputKeys.Space }));
            Assert.That(InputGuide.GetKeys(InputIntentKind.Retry), Is.EqualTo(new[] { InputKeys.KeyR }));
            Assert.That(InputGuide.GetKeys(InputIntentKind.None), Is.EqualTo(Array.Empty<InputKeys>()));

            var kindException = Assert.Throws<ArgumentOutOfRangeException>(() => InputGuide.GetKeys((InputIntentKind)99));
            Assert.That(kindException.ParamName, Is.EqualTo("kind"));

            foreach (var key in InputGuide.GetKeys(InputIntentKind.ConfirmRecording))
            {
                Assert.That(InputIntentResolver.Resolve(key).Kind, Is.EqualTo(InputIntentKind.ConfirmRecording));
            }

            foreach (var key in InputGuide.GetKeys(InputIntentKind.StepForward))
            {
                Assert.That(InputIntentResolver.Resolve(key).Kind, Is.EqualTo(InputIntentKind.StepForward));
            }

            foreach (var key in InputGuide.GetKeys(InputIntentKind.Retry))
            {
                Assert.That(InputIntentResolver.Resolve(key).Kind, Is.EqualTo(InputIntentKind.Retry));
            }

            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowUp).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowUp).Direction, Is.EqualTo(MoveDirection.Up));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyW).Kind, Is.EqualTo(InputIntentKind.Move));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyW).Direction, Is.EqualTo(MoveDirection.Up));

            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowDown).Direction, Is.EqualTo(MoveDirection.Down));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyS).Direction, Is.EqualTo(MoveDirection.Down));
            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowLeft).Direction, Is.EqualTo(MoveDirection.Left));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyA).Direction, Is.EqualTo(MoveDirection.Left));
            Assert.That(InputIntentResolver.Resolve(InputKeys.ArrowRight).Direction, Is.EqualTo(MoveDirection.Right));
            Assert.That(InputIntentResolver.Resolve(InputKeys.KeyD).Direction, Is.EqualTo(MoveDirection.Right));

            var moveKeys = InputGuide.GetKeys(InputIntentKind.Move);
            moveKeys[0] = InputKeys.Enter;
            var freshMoveKeys = InputGuide.GetKeys(InputIntentKind.Move);
            Assert.That(freshMoveKeys[0], Is.EqualTo(InputKeys.ArrowUp));
        }

        [Test]
        public void IG6_HudModelBuilder_UsesGuide()
        {
            const string recordingLine = "MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R";
            const string playbackLine = "STEP: SPACE  RESET: R";
            const string clearedLine = "RESET: R";

            var recordingFourArg = HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>());
            Assert.That(recordingFourArg.KeysLine, Is.EqualTo(recordingLine));
            Assert.That(recordingFourArg.KeysLine, Is.EqualTo(InputGuide.KeysLineFor(GameMode.Recording)));

            var playbackFourArg = HudModelBuilder.Build(
                GameMode.Playback,
                0,
                0,
                new[] { MoveDirection.Up });
            Assert.That(playbackFourArg.KeysLine, Is.EqualTo(playbackLine));
            Assert.That(playbackFourArg.KeysLine, Is.EqualTo(InputGuide.KeysLineFor(GameMode.Playback)));

            var clearedFourArg = HudModelBuilder.Build(GameMode.Cleared, 0, 0, Array.Empty<MoveDirection>());
            Assert.That(clearedFourArg.KeysLine, Is.EqualTo(clearedLine));
            Assert.That(clearedFourArg.KeysLine, Is.EqualTo(InputGuide.KeysLineFor(GameMode.Cleared)));

            var session = new GridPuzzleEngineSession(SampleStageFactory.Create());
            var recordingSevenArg = HudModelBuilder.Build(session);
            Assert.That(recordingSevenArg.KeysLine, Is.EqualTo(recordingLine));
            Assert.That(recordingSevenArg.KeysLine, Is.EqualTo(InputGuide.KeysLineFor(GameMode.Recording)));
        }

        private static void AssertGuardCell(GameMode expectedMode, InputIntent intent, InputResult expectedResult)
        {
            var session = CreateSessionForMode(expectedMode);
            Assert.That(session.State.Mode, Is.EqualTo(expectedMode));

            var dispatcher = new SessionInputDispatcher(session);
            Assert.That(dispatcher.Dispatch(intent), Is.EqualTo(expectedResult));

            var allowed = expectedResult == InputResult.Executed;
            Assert.That(InputGuide.IsAllowed(intent.Kind, expectedMode), Is.EqualTo(allowed));
        }

        private static GridPuzzleEngineSession CreateSessionForMode(GameMode mode)
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            switch (mode)
            {
                case GameMode.Recording:
                    return session;

                case GameMode.Playback:
                    Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
                    Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
                    Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
                    return session;

                case GameMode.Cleared:
                    Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
                    Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
                    Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
                    Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
                    Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
                    Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
                    Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
                    Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
                    Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
                    return session;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "The game mode is not defined.");
            }
        }
    }
}
