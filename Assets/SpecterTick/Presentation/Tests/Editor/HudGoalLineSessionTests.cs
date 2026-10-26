using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class HudGoalLineSessionTests
    {
        private const string KeysRecording = "MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R";

        [Test]
        public void GS1_Demo_Initial()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var model = HudModelBuilder.Build(session);

            Assert.That(model.ModeLine, Is.EqualTo("MODE: RECORDING"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 0"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 0"));
            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 0 / 2  EXIT: CLOSED"));
            Assert.That(model.KeysLine, Is.EqualTo(KeysRecording));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: -"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("MODE: RECORDING\nTICK: 0\nRECORDED: 0\nSWITCHES: 0 / 2  EXIT: CLOSED\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: -"));
        }

        [Test]
        public void GS2_Demo_Recording_Partial()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(1));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(2));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(2));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            var model = HudModelBuilder.Build(session);
            Assert.That(model.TickLine, Is.EqualTo("TICK: 3"));
            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 1 / 2  EXIT: CLOSED"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: U U R"));
        }

        [Test]
        public void GS3_Demo_Confirm_ResetsCount()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(1));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(2));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(2));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 1 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Down)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(1));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 1 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));

            var model = HudModelBuilder.Build(session);
            Assert.That(model.ProgressLine, Is.EqualTo("PLAYBACK: 0 / 4"));
            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 0 / 2  EXIT: CLOSED"));
        }

        [Test]
        public void GS4_Demo_Playback_ToCleared()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 0 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 1 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(2));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(2));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 1 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(3));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(3));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 1 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));

            var model = HudModelBuilder.Build(session);
            Assert.That(model.ModeLine, Is.EqualTo("MODE: CLEARED"));
            Assert.That(model.TickLine, Is.EqualTo("TICK: 4"));
            Assert.That(model.ProgressLine, Is.EqualTo("RECORDED: 5"));
            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 2 / 2  EXIT: OPEN"));
            Assert.That(model.KeysLine, Is.EqualTo("RESET: R"));
            Assert.That(model.CommandsLine, Is.EqualTo("COMMANDS: R R R R R"));
        }

        [Test]
        public void GS5_SG_ExitOpenWithoutSwitches()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.Create());
            var model = HudModelBuilder.Build(session);

            Assert.That(model.GoalLine, Is.EqualTo("SWITCHES: 0 / 0  EXIT: OPEN"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("MODE: RECORDING\nTICK: 0\nRECORDED: 0\nSWITCHES: 0 / 0  EXIT: OPEN\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: -"));
        }

        [Test]
        public void GS6_Cleared_RejectsInput_ThenRetryResets()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var dispatcher = new SessionInputDispatcher(session);
            AdvanceDemoToCleared(dispatcher, session);

            const string clearedText =
                "MODE: CLEARED\nTICK: 4\nRECORDED: 5\nSWITCHES: 2 / 2  EXIT: OPEN\nRESET: R\nCOMMANDS: R R R R R";
            Assert.That(HudModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(clearedText));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Left)), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(HudModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(clearedText));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(HudModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(clearedText));

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(HudModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(clearedText));

            Assert.That(dispatcher.Dispatch(InputIntent.Retry), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(
                HudModelBuilder.Build(session).ToDisplayText(),
                Is.EqualTo("MODE: RECORDING\nTICK: 0\nRECORDED: 0\nSWITCHES: 0 / 2  EXIT: CLOSED\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: -"));
        }

        [Test]
        public void GS7_CustomStage_SwitchesNotEqualDoors()
        {
            var definition = new StageDefinition
            {
                Width = 5,
                Height = 4,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(3, 3)
            };
            definition.Switches.Add(new SwitchPlacement(new SwitchId("A"), new GridPosition(1, 1)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("B"), new GridPosition(2, 1)));

            var validation = StageValidator.Validate(definition);
            Assert.That(validation.IsSuccess, Is.True);

            var session = new GridPuzzleEngineSession(validation.Stage);
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 0 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(1));
            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 0 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(1));
            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 1 / 2  EXIT: CLOSED"));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(1));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(HudModelBuilder.Build(session).GoalLine, Is.EqualTo("SWITCHES: 2 / 2  EXIT: OPEN"));
        }

        private static void AdvanceDemoToCleared(SessionInputDispatcher dispatcher, GridPuzzleEngineSession session)
        {
            for (var i = 0; i < 5; i++)
            {
                Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            }

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));

            for (var i = 0; i < 4; i++)
            {
                Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            }

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
        }
    }
}
