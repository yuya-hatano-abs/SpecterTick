using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Tests.Stages;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class GridPuzzleEngineSessionRecordingPlaybackTests
    {
        private static readonly GridPosition SpecterF2 = new GridPosition(4, 4);
        private static readonly SwitchId Sa = new SwitchId("SA");
        private static readonly SwitchId Sb = new SwitchId("SB");
        private static readonly DoorId Da = new DoorId("DA");
        private static readonly DoorId Dab = new DoorId("DAB");

        [Test]
        public void RecordingPlayback_F2_G8_SpecterActivatesSwitches_PlayerClearsAtExit()
        {
            var session = CreateF2Session();
            var recording = new[]
            {
                MoveDirection.Down, MoveDirection.Down, MoveDirection.Down,
                MoveDirection.Left, MoveDirection.Left, MoveDirection.Left, MoveDirection.Left,
                MoveDirection.Right, MoveDirection.Right, MoveDirection.Right, MoveDirection.Right
            };
            RunRecording(session, recording);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(11));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(11));
            AssertSaSb(session, false, false);

            session.ConfirmRecording();
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            AssertSaSb(session, false, false);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.IsPlaybackFinished, Is.False);

            session.StepPlayback();
            AssertPlaybackTick(session, 1, new GridPosition(4, 0), new GridPosition(4, 3), 1, false, false, false, false, GameMode.Playback);

            session.StepPlayback();
            AssertPlaybackTick(session, 2, new GridPosition(4, 0), new GridPosition(4, 2), 2, false, false, false, false, GameMode.Playback);

            session.StepPlayback();
            AssertPlaybackTick(session, 3, new GridPosition(4, 0), new GridPosition(4, 1), 3, false, false, false, false, GameMode.Playback);

            session.StepPlayback();
            AssertPlaybackTick(session, 4, new GridPosition(4, 0), new GridPosition(3, 1), 4, false, false, false, false, GameMode.Playback);

            session.StepPlayback();
            AssertPlaybackTick(session, 5, new GridPosition(4, 0), new GridPosition(2, 1), 5, false, true, false, false, GameMode.Playback);

            session.StepPlayback();
            AssertPlaybackTick(session, 6, new GridPosition(4, 0), new GridPosition(1, 1), 6, false, true, false, false, GameMode.Playback);

            session.StepPlayback();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(7));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(7));
            AssertSaSb(session, true, true);
            Assert.That(session.IsDoorOpen(Da), Is.True);
            Assert.That(session.IsDoorOpen(Dab), Is.True);
            Assert.That(session.IsExitOpen, Is.True);
            Assert.That(session.IsPlaybackFinished, Is.False);

            var commandsBefore = session.RecordedCommands;
            var stateBefore = session.State;
            Assert.Throws<InvalidOperationException>(() => session.StepPlayback());
            Assert.That(session.State, Is.SameAs(stateBefore));
            Assert.That(session.RecordedCommands, Is.SameAs(commandsBefore));
        }

        [Test]
        public void RecordingPlayback_F2_G9a_ConfirmResetsActivation_PlaybackFinishTickOnly()
        {
            var session = CreateF2Session();
            RunRecording(session, new[]
            {
                MoveDirection.Up, MoveDirection.Right, MoveDirection.Right, MoveDirection.Down
            });
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 0)));
            AssertSaSb(session, true, true);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(4));
            Assert.That(session.State.Tick, Is.EqualTo(4));

            session.ConfirmRecording();
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            AssertSaSb(session, false, false);
            Assert.That(session.IsExitOpen, Is.False);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.Tick, Is.EqualTo(0));

            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(1));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            AssertSaSb(session, false, false);

            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(2));
            Assert.That(session.State.Tick, Is.EqualTo(2));

            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(3));
            Assert.That(session.State.Tick, Is.EqualTo(3));

            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.IsPlaybackFinished, Is.True);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));

            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.Tick, Is.EqualTo(5));
            AssertSaSb(session, false, false);
        }

        [Test]
        public void RecordingPlayback_F2_G9b_ConfirmEvaluatesStationaryPlayer()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(1));

            session.ConfirmRecording();
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(session.IsDoorOpen(Da), Is.True);
            Assert.That(session.IsExitOpen, Is.False);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.Tick, Is.EqualTo(0));
        }

        [Test]
        public void RecordingPlayback_F2_G10a_SpecterBlockedByClosedDoor()
        {
            var session = CreateF2Session();
            RunRecording(session, new[]
            {
                MoveDirection.Left, MoveDirection.Left, MoveDirection.Left, MoveDirection.Left,
                MoveDirection.Down, MoveDirection.Down, MoveDirection.Down
            });
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            session.ConfirmRecording();
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            AssertSaSb(session, false, false);

            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 4)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(1));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(2, 4)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(2));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 4)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(3));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 4)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(5));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(6));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(7));
            Assert.That(session.IsPlaybackFinished, Is.True);
            AssertSaSb(session, false, false);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
        }

        [Test]
        public void RecordingPlayback_F2_G10b_SpecterPassesOpenDoor()
        {
            var session = CreateF2Session();
            RunRecording(session, new[]
            {
                MoveDirection.Left, MoveDirection.Left, MoveDirection.Left, MoveDirection.Left,
                MoveDirection.Down, MoveDirection.Down, MoveDirection.Down, MoveDirection.Up
            });
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            session.ConfirmRecording();
            Assert.That(session.IsDoorOpen(Da), Is.True);

            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 4)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(1));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(2, 4)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 4)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 4)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 3)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 1)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(8));
            Assert.That(session.IsPlaybackFinished, Is.True);
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
        }

        [Test]
        public void Recording_F2_G11_NoClearWhileRecording()
        {
            var session = CreateF2Session();
            RunRecording(session, new[]
            {
                MoveDirection.Up, MoveDirection.Right, MoveDirection.Right,
                MoveDirection.Right, MoveDirection.Right, MoveDirection.Down
            });
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            AssertSaSb(session, true, true);
            Assert.That(session.IsExitOpen, Is.True);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(6));

            session.ConfirmRecording();
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            AssertSaSb(session, false, false);
            Assert.That(session.IsExitOpen, Is.False);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.Tick, Is.EqualTo(0));
        }

        [Test]
        public void RecordingPlayback_F2N_G12a_SpecterOnExitDoesNotClear()
        {
            var session = new GridPuzzleEngineSession(F2NStageDefinitionBuilder.CreateValidatedStage());
            RunRecording(session, new[] { MoveDirection.Down, MoveDirection.Down, MoveDirection.Down, MoveDirection.Down });
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            session.ConfirmRecording();
            Assert.That(session.IsExitOpen, Is.True);

            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 3)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 2)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 1)));
            session.StepPlayback();
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.IsPlaybackFinished, Is.True);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
        }

        [Test]
        public void RecordingPlayback_F2N_G12b_ConfirmClearsWhenExitOpen()
        {
            var session = new GridPuzzleEngineSession(F2NStageDefinitionBuilder.CreateValidatedStage());
            RunRecording(session, new[]
            {
                MoveDirection.Right, MoveDirection.Right, MoveDirection.Right, MoveDirection.Right
            });
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            session.ConfirmRecording();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));

            var commands = session.RecordedCommands;
            Assert.Throws<InvalidOperationException>(() => session.StepPlayback());
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void Step_Recording_RejectedMove_StillAppendsCommand()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Down);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(1));
            Assert.That(session.RecordedCommands[0].Direction, Is.EqualTo(MoveDirection.Down));
        }

        [Test]
        public void ConfirmRecording_EmptySequence_AllowsPlaybackFinished()
        {
            var session = CreateF2Session();
            session.ConfirmRecording();
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0));
            Assert.That(session.IsPlaybackFinished, Is.True);
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
        }

        [Test]
        public void IsPlaybackFinished_NewRecordingSession_IsFalse()
        {
            var session = CreateF2Session();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.IsPlaybackFinished, Is.False);
        }

        [Test]
        public void Step_PlaybackMode_ThrowsInvalidOperationException()
        {
            var session = CreateF2Session();
            session.ConfirmRecording();
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<InvalidOperationException>(() => session.Step(MoveDirection.Up));
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void StepPlayback_RecordingMode_ThrowsInvalidOperationException()
        {
            var session = CreateF2Session();
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<InvalidOperationException>(() => session.StepPlayback());
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void ConfirmRecording_Twice_ThrowsInvalidOperationException()
        {
            var session = CreateF2Session();
            session.ConfirmRecording();
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<InvalidOperationException>(() => session.ConfirmRecording());
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void RecordedCommands_InstanceUnchangedAfterSubsequentSteps()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Up);
            var snapshot = session.RecordedCommands;
            session.Step(MoveDirection.Right);
            Assert.That(session.RecordedCommands, Is.Not.SameAs(snapshot));
            Assert.That(snapshot.Count, Is.EqualTo(1));
            Assert.That(snapshot[0].Direction, Is.EqualTo(MoveDirection.Up));
        }

        [Test]
        public void Step_AfterConfirm_DoesNotChangeRecordedCommands()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Up);
            session.ConfirmRecording();
            var before = session.RecordedCommands;
            Assert.Throws<InvalidOperationException>(() => session.Step(MoveDirection.Down));
            Assert.That(session.RecordedCommands, Is.SameAs(before));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(1));
        }

        [Test]
        public void Recording_SpecterRemainsAtInitialPosition()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Down);
            session.Step(MoveDirection.Left);
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
        }

        [Test]
        public void TwoSessions_DoNotShareStateOrRecording()
        {
            var a = CreateF2Session();
            var b = CreateF2Session();
            a.Step(MoveDirection.Up);
            Assert.That(b.State.Tick, Is.EqualTo(0));
            Assert.That(b.RecordedCommands.Count, Is.EqualTo(0));
            Assert.That(b.State.IsSwitchActivated(Sa), Is.False);
        }

        [Test]
        public void Step_UndefinedDirection_InRecording_LeavesStateAndRecordingUnchanged()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Up);
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Step((MoveDirection)0));
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void Step_UndefinedDirection_InPlayback_ThrowsArgumentOutOfRangeException()
        {
            var session = CreateF2Session();
            session.ConfirmRecording();
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Step((MoveDirection)0));
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void StepPlayback_InClearedMode_ThrowsInvalidOperationException()
        {
            var session = CreateClearedF2NSession();
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<InvalidOperationException>(() => session.StepPlayback());
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void Step_InClearedMode_ThrowsInvalidOperationException()
        {
            var session = CreateClearedF2NSession();
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<InvalidOperationException>(() => session.Step(MoveDirection.Down));
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void ConfirmRecording_InClearedMode_ThrowsInvalidOperationException()
        {
            var session = CreateClearedF2NSession();
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<InvalidOperationException>(() => session.ConfirmRecording());
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void Step_UndefinedDirection_InClearedMode_ThrowsArgumentOutOfRangeException()
        {
            var session = CreateClearedF2NSession();
            var state = session.State;
            var commands = session.RecordedCommands;
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Step((MoveDirection)0));
            Assert.That(session.State, Is.SameAs(state));
            Assert.That(session.RecordedCommands, Is.SameAs(commands));
        }

        [Test]
        public void TwoSessions_SameRecordingAndPlayback_ProduceEqualStateAndCommands()
        {
            var directions = new[] { MoveDirection.Up, MoveDirection.Right, MoveDirection.Right, MoveDirection.Down };
            const int playbackSteps = 2;

            var a = CreateF2Session();
            RunRecording(a, directions);
            a.ConfirmRecording();
            for (var i = 0; i < playbackSteps; i++)
            {
                a.StepPlayback();
            }

            var b = CreateF2Session();
            RunRecording(b, directions);
            b.ConfirmRecording();
            for (var i = 0; i < playbackSteps; i++)
            {
                b.StepPlayback();
            }

            Assert.That(b.State, Is.EqualTo(a.State));
            Assert.That(b.State.GetHashCode(), Is.EqualTo(a.State.GetHashCode()));
            Assert.That(b.RecordedCommands, Is.EqualTo(a.RecordedCommands));
            Assert.That(b.RecordedCommands.GetHashCode(), Is.EqualTo(a.RecordedCommands.GetHashCode()));
        }

        [Test]
        public void ConfirmRecording_UsesSnapshotExitPosition_NotLiveStageExit()
        {
            var inner = F2NStageDefinitionBuilder.CreateValidatedStage();
            var mutable = new MutableExitPositionStage(inner, inner.ExitPosition);
            var session = new GridPuzzleEngineSession(mutable);
            mutable.SetExitPosition(new GridPosition(0, 0));

            RunRecording(session, new[]
            {
                MoveDirection.Right, MoveDirection.Right, MoveDirection.Right, MoveDirection.Right
            });
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));

            session.ConfirmRecording();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
        }

        private static GridPuzzleEngineSession CreateF2Session()
        {
            return new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
        }

        private static GridPuzzleEngineSession CreateClearedF2NSession()
        {
            var session = new GridPuzzleEngineSession(F2NStageDefinitionBuilder.CreateValidatedStage());
            RunRecording(session, new[]
            {
                MoveDirection.Right, MoveDirection.Right, MoveDirection.Right, MoveDirection.Right
            });
            session.ConfirmRecording();
            return session;
        }

        private sealed class MutableExitPositionStage : IStage
        {
            private readonly IStage _inner;
            private GridPosition _exitPosition;

            public MutableExitPositionStage(IStage inner, GridPosition exitPosition)
            {
                _inner = inner;
                _exitPosition = exitPosition;
            }

            public int Width => _inner.Width;
            public int Height => _inner.Height;
            public GridPosition PlayerPosition => _inner.PlayerPosition;
            public GridPosition SpecterPosition => _inner.SpecterPosition;
            public GridPosition ExitPosition => _exitPosition;
            public IReadOnlyList<GridPosition> Walls => _inner.Walls;
            public IReadOnlyList<StageSwitch> Switches => _inner.Switches;
            public IReadOnlyList<StageDoor> Doors => _inner.Doors;

            public void SetExitPosition(GridPosition exitPosition)
            {
                _exitPosition = exitPosition;
            }
        }

        private static void RunRecording(GridPuzzleEngineSession session, MoveDirection[] directions)
        {
            for (var i = 0; i < directions.Length; i++)
            {
                session.Step(directions[i]);
            }
        }

        private static void AssertSaSb(GridPuzzleEngineSession session, bool sa, bool sb)
        {
            Assert.That(session.State.IsSwitchActivated(Sa), Is.EqualTo(sa));
            Assert.That(session.State.IsSwitchActivated(Sb), Is.EqualTo(sb));
        }

        private static void AssertPlaybackTick(
            GridPuzzleEngineSession session,
            int tick,
            GridPosition player,
            GridPosition specter,
            int index,
            bool sa,
            bool sb,
            bool exitOpen,
            bool all,
            GameMode mode)
        {
            Assert.That(session.State.Tick, Is.EqualTo(tick));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(player));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(specter));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(index));
            AssertSaSb(session, sa, sb);
            Assert.That(session.IsExitOpen, Is.EqualTo(exitOpen));
            Assert.That(session.AllSwitchesActivated, Is.EqualTo(all));
            Assert.That(session.State.Mode, Is.EqualTo(mode));
        }
    }
}
