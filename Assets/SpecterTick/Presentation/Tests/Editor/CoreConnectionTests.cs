using System;
using System.Collections.Generic;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class CoreConnectionTests
    {
        private static readonly SwitchId Sa = new SwitchId("SA");
        private static readonly DoorId Da = new DoorId("DA");

        [Test]
        public void T1_RecordConfirmClearThenRetry_OnS1()
        {
            var session = new GridPuzzleEngineSession(CreateValidatedStageS1());

            session.Step(MoveDirection.Right);
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(1));

            session.Step(MoveDirection.Right);
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(2));

            session.ConfirmRecording();
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.IsPlaybackFinished, Is.False);
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(2));

            session.Retry();
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0));
        }

        [Test]
        public void T2_ValidationFailure_WidthZero_OnS1Positions()
        {
            var definition = new StageDefinition
            {
                Width = 0,
                Height = 1,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(2, 0)
            };

            var result = StageValidator.Validate(definition);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0].Code, Is.EqualTo(StageValidationErrorCode.WidthInvalid));

            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = result.Stage;
            });
        }

        [Test]
        public void T3_PublicApiReadings_OnS2()
        {
            var session = new GridPuzzleEngineSession(CreateValidatedStageS2());

            Assert.That(session.State.IsSwitchActivated(Sa), Is.False);
            Assert.That(session.IsDoorOpen(Da), Is.False);
            Assert.That(session.IsExitOpen, Is.False);
            Assert.That(session.AllSwitchesActivated, Is.False);

            session.Step(MoveDirection.Right);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(session.IsDoorOpen(Da), Is.True);
            Assert.That(session.IsExitOpen, Is.True);
            Assert.That(session.AllSwitchesActivated, Is.True);
        }

        [Test]
        public void T4_InvalidModeOperations_OnS2()
        {
            var session = new GridPuzzleEngineSession(CreateValidatedStageS2());

            session.ConfirmRecording();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 1)));

            var stateBefore = session.State;
            Assert.Throws<InvalidOperationException>(() => session.Step(MoveDirection.Up));
            Assert.That(session.State, Is.SameAs(stateBefore));

            Assert.Throws<InvalidOperationException>(() => session.ConfirmRecording());
            Assert.That(session.State, Is.SameAs(stateBefore));

            session.StepPlayback();
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.IsPlaybackFinished, Is.True);
        }

        private static IStage CreateValidatedStageS1()
        {
            var definition = new StageDefinition
            {
                Width = 3,
                Height = 1,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(2, 0)
            };

            var result = StageValidator.Validate(definition);
            Assert.That(result.IsSuccess, Is.True);
            return result.Stage;
        }

        private static IStage CreateValidatedStageS2()
        {
            var definition = new StageDefinition
            {
                Width = 4,
                Height = 2,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(0, 1),
                ExitPosition = new GridPosition(3, 0)
            };
            definition.Switches.Add(new SwitchPlacement(Sa, new GridPosition(1, 0)));
            definition.Doors.Add(new DoorPlacement(Da, new GridPosition(2, 0), new List<SwitchId> { Sa }));

            var result = StageValidator.Validate(definition);
            Assert.That(result.IsSuccess, Is.True);
            return result.Stage;
        }
    }
}
