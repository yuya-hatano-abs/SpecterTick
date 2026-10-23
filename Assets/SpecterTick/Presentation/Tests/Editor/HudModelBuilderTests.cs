using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class HudModelBuilderTests
    {
        [Test]
        public void H1_Recording_Empty()
        {
            var model = HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>());

            Assert.That(model.ModeLine, Is.EqualTo("MODE: RECORDING"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 0"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 0"));
            Assert.That(model.KeysLine, Is.EqualTo("MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: -"));
        }

        [Test]
        public void H2_Recording_ThreeCommands()
        {
            var directions = new[] { MoveDirection.Up, MoveDirection.Right, MoveDirection.Right };
            var model = HudModelBuilder.Build(GameMode.Recording, 3, 0, directions);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: RECORDING"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 3"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 3"));
            Assert.That(model.KeysLine, Is.EqualTo("MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U R R"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("MODE: RECORDING\nTICK: 3\nRECORDED: 3\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: U R R"));
        }

        [Test]
        public void H3_Recording_ExactlyEight()
        {
            var directions = new[]
            {
                MoveDirection.Up,
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Down,
                MoveDirection.Down,
                MoveDirection.Left,
                MoveDirection.Left
            };
            var model = HudModelBuilder.Build(GameMode.Recording, 8, 0, directions);

            Assert.That(model.TickLine, Is.EqualTo("TICK: 8"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 8"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U U R R D D L L"));
        }

        [Test]
        public void H4_Recording_NineCommands()
        {
            var directions = new[]
            {
                MoveDirection.Up,
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Down,
                MoveDirection.Down,
                MoveDirection.Left,
                MoveDirection.Left,
                MoveDirection.Up
            };
            var model = HudModelBuilder.Build(GameMode.Recording, 9, 0, directions);

            Assert.That(model.TickLine, Is.EqualTo("TICK: 9"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 9"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: ... U R R D D L L U"));
        }

        [Test]
        public void H5_Playback_Start()
        {
            var directions = new[]
            {
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Down,
                MoveDirection.Left
            };
            var model = HudModelBuilder.Build(GameMode.Playback, 0, 0, directions);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: PLAYBACK"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 0"));
            Assert.That(model.ProgressLine, Is.EqualTo("PLAYBACK: 0 / 5"));
            Assert.That(model.KeysLine, Is.EqualTo("STEP: SPACE  RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U R R D L"));
        }

        [Test]
        public void H6_Playback_Midway()
        {
            var directions = new[]
            {
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Down,
                MoveDirection.Left
            };
            var model = HudModelBuilder.Build(GameMode.Playback, 2, 2, directions);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: PLAYBACK"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 2"));
            Assert.That(model.ProgressLine, Is.EqualTo("PLAYBACK: 2 / 5"));
            Assert.That(model.KeysLine, Is.EqualTo("STEP: SPACE  RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U R R D L"));
        }

        [Test]
        public void H7_Playback_FinishedTickKeepsRunning()
        {
            var directions = new[]
            {
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Down,
                MoveDirection.Left
            };
            var model = HudModelBuilder.Build(GameMode.Playback, 7, 5, directions);

            Assert.That(model.TickLine, Is.EqualTo("TICK: 7"));
            Assert.That(model.ProgressLine, Is.EqualTo("PLAYBACK: 5 / 5"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U R R D L"));
        }

        [Test]
        public void H8_Cleared()
        {
            var directions = new[]
            {
                MoveDirection.Up,
                MoveDirection.Up,
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Right
            };
            var model = HudModelBuilder.Build(GameMode.Cleared, 0, 0, directions);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: CLEARED"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 0"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 7"));
            Assert.That(model.KeysLine, Is.EqualTo("RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U U U R R R R"));
        }

        [Test]
        public void H9_InvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, 0, (IReadOnlyList<MoveDirection>)null));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build((GameMode)99, 0, 0, new[] { MoveDirection.Up }));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build(GameMode.Recording, -1, 0, new[] { MoveDirection.Up }));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, -1, new[] { MoveDirection.Up }));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                HudModelBuilder.Build(GameMode.Recording, 0, 0, new[] { (MoveDirection)99 }));

            Assert.Throws<ArgumentNullException>(() => HudModelBuilder.Build((GridPuzzleEngineSession)null));
        }

        [Test]
        public void A1_FromSession_Recording()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));

            var moveUp = dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up));
            Assert.That(moveUp, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            var moveRight = dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right));
            Assert.That(moveRight, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            var model = HudModelBuilder.Build(session);
            Assert.That(model.ModeLine, Is.EqualTo("MODE: RECORDING"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 2"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 2"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U R"));
        }

        [Test]
        public void A2_FromSession_Playback()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            var moveUp = dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up));
            Assert.That(moveUp, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));

            var confirm = dispatcher.Dispatch(InputIntent.ConfirmRecording);
            Assert.That(confirm, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));

            var modelAfterConfirm = HudModelBuilder.Build(session);
            Assert.That(modelAfterConfirm.ModeLine, Is.EqualTo("MODE: PLAYBACK"));
            Assert.That(modelAfterConfirm.TickLine, Is.EqualTo("TICK: 0"));
            Assert.That(modelAfterConfirm.ProgressLine, Is.EqualTo("PLAYBACK: 0 / 1"));
            Assert.That(modelAfterConfirm.CommandsLine, Is.EqualTo("COMMANDS: U"));

            var step = dispatcher.Dispatch(InputIntent.StepForward);
            Assert.That(step, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));

            var modelAfterStep = HudModelBuilder.Build(session);
            Assert.That(modelAfterStep.TickLine, Is.EqualTo("TICK: 1"));
            Assert.That(modelAfterStep.ProgressLine, Is.EqualTo("PLAYBACK: 1 / 1"));
            Assert.That(modelAfterStep.CommandsLine, Is.EqualTo("COMMANDS: U"));
        }
    }
}
