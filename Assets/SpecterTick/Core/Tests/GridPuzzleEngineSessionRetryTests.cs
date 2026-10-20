using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Tests.Stages;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class GridPuzzleEngineSessionRetryTests
    {
        private static readonly GridPosition Origin = new GridPosition(0, 0);
        private static readonly GridPosition SpecterF2 = new GridPosition(4, 4);
        private static readonly GridPosition ExitF2 = new GridPosition(4, 0);
        private static readonly SwitchId Sa = new SwitchId("SA");
        private static readonly SwitchId Sb = new SwitchId("SB");
        private static readonly DoorId Da = new DoorId("DA");
        private static readonly DoorId Dab = new DoorId("DAB");

        private static readonly MoveDirection[] G13Recording =
        {
            MoveDirection.Down, MoveDirection.Down, MoveDirection.Down,
            MoveDirection.Left, MoveDirection.Left, MoveDirection.Left, MoveDirection.Left,
            MoveDirection.Right, MoveDirection.Right, MoveDirection.Right, MoveDirection.Right
        };

        [Test]
        public void G13_F2_RecordingTicks_MatchGoldenTable()
        {
            var session = CreateF2Session();
            for (var i = 0; i < 7; i++)
            {
                session.Step(G13Recording[i]);
                Assert.That(session.State.Tick, Is.EqualTo(i + 1), "Recording tick " + (i + 1));
                Assert.That(session.State.PlayerPosition, Is.EqualTo(Origin), "Recording player tick " + (i + 1));
                Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
                Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
                Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
                AssertSaSb(session, false, false);
                Assert.That(session.RecordedCommands.Count, Is.EqualTo(i + 1));
            }

            session.Step(G13Recording[7]);
            AssertRecordingTick(session, 8, new GridPosition(1, 0), 8);
            session.Step(G13Recording[8]);
            AssertRecordingTick(session, 9, new GridPosition(2, 0), 9);
            session.Step(G13Recording[9]);
            AssertRecordingTick(session, 10, new GridPosition(3, 0), 10);
            session.Step(G13Recording[10]);
            AssertRecordingTick(session, 11, ExitF2, 11);
        }

        [Test]
        public void G13_F2_ConfirmAndPlaybackTicks_MatchGoldenTable()
        {
            var session = CreateF2Session();
            RunRecording(session, G13Recording);
            session.ConfirmRecording();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(ExitF2));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            AssertSaSb(session, false, false);
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(11));

            session.StepPlayback();
            AssertPlaybackGolden(session, 1, ExitF2, new GridPosition(4, 3), 1, false, false, GameMode.Playback);
            session.StepPlayback();
            AssertPlaybackGolden(session, 2, ExitF2, new GridPosition(4, 2), 2, false, false, GameMode.Playback);
            session.StepPlayback();
            AssertPlaybackGolden(session, 3, ExitF2, new GridPosition(4, 1), 3, false, false, GameMode.Playback);
            session.StepPlayback();
            AssertPlaybackGolden(session, 4, ExitF2, new GridPosition(3, 1), 4, false, false, GameMode.Playback);
            session.StepPlayback();
            AssertPlaybackGolden(session, 5, ExitF2, new GridPosition(2, 1), 5, false, true, GameMode.Playback);
            session.StepPlayback();
            AssertPlaybackGolden(session, 6, ExitF2, new GridPosition(1, 1), 6, false, true, GameMode.Playback);
            session.StepPlayback();
            AssertPlaybackGolden(session, 7, ExitF2, new GridPosition(0, 1), 7, true, true, GameMode.Cleared);
        }

        [Test]
        public void G14_F2_RecordingBeforeRetry_MatchGoldenTable()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Right);
            AssertRecordingTick(session, 1, new GridPosition(1, 0), 1);
            session.Step(MoveDirection.Right);
            AssertRecordingTick(session, 2, new GridPosition(2, 0), 2);
            session.Step(MoveDirection.Up);
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            AssertSaSb(session, false, true);
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(3));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
        }

        [Test]
        public void R1_FromRecording_MatchesFreshSession()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Up);
            session.Retry();
            AssertMatchesFreshF2Session(session, "R1");
        }

        [Test]
        public void R2_FromPlaybackMid_MatchesFreshSession()
        {
            var session = CreateF2Session();
            RunRecording(session, G13Recording);
            session.ConfirmRecording();
            for (var i = 0; i < 5; i++)
            {
                session.StepPlayback();
            }

            Assert.That(session.State.Tick, Is.EqualTo(5));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(5));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.IsSwitchActivated(Sb), Is.True);
            Assert.That(session.State.IsSwitchActivated(Sa), Is.False);

            session.Retry();
            AssertMatchesFreshF2Session(session, "R2");
        }

        [Test]
        public void R3_FromCleared_MatchesFreshSession()
        {
            var session = CreateG13ClearedSession();
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(session.State.IsSwitchActivated(Sb), Is.True);
            session.Retry();
            AssertMatchesFreshF2Session(session, "R3");
        }

        [Test]
        public void R4_DoubleRetry_RemainsInitial()
        {
            var session = CreateG13ClearedSession();
            session.Retry();
            session.Retry();
            AssertMatchesFreshF2Session(session, "R4");
        }

        [Test]
        public void R5_FromEmptyPlaybackMid_MatchesFreshSession()
        {
            var session = CreateF2Session();
            session.ConfirmRecording();
            session.StepPlayback();
            session.StepPlayback();
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.IsPlaybackFinished, Is.True);

            session.Retry();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.IsPlaybackFinished, Is.False);
            Assert.That(session.State.Tick, Is.EqualTo(0));
            AssertMatchesFreshF2Session(session, "R5");
        }

        [Test]
        public void R3_AfterRetry_CanRecordStep()
        {
            var session = CreateG13ClearedSession();
            session.Retry();
            session.Step(MoveDirection.Up);
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
        }

        [Test]
        public void R3_AfterRetry_ReplayG13_MatchesFirstRun()
        {
            var session = CreateG13ClearedSession();
            session.Retry();
            RunRecording(session, G13Recording);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(ExitF2));
            Assert.That(session.State.Tick, Is.EqualTo(11));
            session.ConfirmRecording();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            for (var i = 0; i < 7; i++)
            {
                session.StepPlayback();
            }

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(7));
        }

        [Test]
        public void Retry_PreservesStageReference()
        {
            var session = CreateF2Session();
            var stage = session.Stage;
            RunRecording(session, G13Recording);
            session.ConfirmRecording();
            session.Retry();
            Assert.That(session.Stage, Is.SameAs(stage));
        }

        [Test]
        public void L1_PriorStateAndRecordedCommands_UnchangedAfterRetry()
        {
            var session = CreateF2Session();
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Up);
            var stateBefore = session.State;
            var commandsBefore = session.RecordedCommands;
            session.Retry();

            Assert.That(stateBefore.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(stateBefore.Tick, Is.EqualTo(3));
            Assert.That(stateBefore.PlayerPosition, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(stateBefore.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(stateBefore.PlaybackIndex, Is.EqualTo(0));
            Assert.That(stateBefore.IsSwitchActivated(Sa), Is.False);
            Assert.That(stateBefore.IsSwitchActivated(Sb), Is.True);
            Assert.That(commandsBefore.Count, Is.EqualTo(3));
            Assert.That(commandsBefore[0].Direction, Is.EqualTo(MoveDirection.Right));
            Assert.That(commandsBefore[1].Direction, Is.EqualTo(MoveDirection.Right));
            Assert.That(commandsBefore[2].Direction, Is.EqualTo(MoveDirection.Up));
        }

        [Test]
        public void L2_SessionB_UnchangedWhenSessionA_Retries()
        {
            var a = CreateF2Session();
            var b = CreateF2Session();
            RunRecording(a, G13Recording);
            a.Retry();
            Assert.That(b.State.Tick, Is.EqualTo(0));
            Assert.That(b.State.PlayerPosition, Is.EqualTo(Origin));
            Assert.That(b.RecordedCommands.Count, Is.EqualTo(0));
        }

        [Test]
        public void L2_SessionA_UnchangedWhenSessionB_Retries()
        {
            var a = CreateF2Session();
            var b = CreateF2Session();
            a.Step(MoveDirection.Up);
            b.Step(MoveDirection.Right);
            b.Retry();
            Assert.That(a.State.Tick, Is.EqualTo(1));
            Assert.That(a.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(a.RecordedCommands.Count, Is.EqualTo(1));
        }

        private static GridPuzzleEngineSession CreateF2Session()
        {
            return new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
        }

        private static GridPuzzleEngineSession CreateG13ClearedSession()
        {
            var session = CreateF2Session();
            RunRecording(session, G13Recording);
            session.ConfirmRecording();
            for (var i = 0; i < 7; i++)
            {
                session.StepPlayback();
            }

            return session;
        }

        private static void RunRecording(GridPuzzleEngineSession session, MoveDirection[] directions)
        {
            for (var i = 0; i < directions.Length; i++)
            {
                session.Step(directions[i]);
            }
        }

        private static void AssertMatchesFreshF2Session(GridPuzzleEngineSession session, string label)
        {
            var fresh = CreateF2Session();
            Assert.That(session.State.PlayerPosition, Is.EqualTo(Origin), label + " Player");
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2), label + " Specter");
            Assert.That(session.State.Tick, Is.EqualTo(0), label + " Tick");
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording), label + " Mode");
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0), label + " PlaybackIndex");
            Assert.That(session.State.IsSwitchActivated(Sa), Is.False, label + " SA");
            Assert.That(session.State.IsSwitchActivated(Sb), Is.False, label + " SB");
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0), label + " RecordedCommands.Count");
            Assert.That(session.IsPlaybackFinished, Is.False, label + " IsPlaybackFinished");
            Assert.That(session.State, Is.EqualTo(fresh.State), label + " State.Equals");
            Assert.That(session.State.GetHashCode(), Is.EqualTo(fresh.State.GetHashCode()), label + " State.Hash");
            Assert.That(session.RecordedCommands.Equals(RecordedCommandSequence.Empty), Is.True, label + " RecordedCommands.Equals");
        }

        private static void AssertRecordingTick(
            GridPuzzleEngineSession session,
            int tick,
            GridPosition player,
            int recordedCount)
        {
            Assert.That(session.State.Tick, Is.EqualTo(tick));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(player));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(recordedCount));
        }

        private static void AssertPlaybackGolden(
            GridPuzzleEngineSession session,
            int tick,
            GridPosition player,
            GridPosition specter,
            int index,
            bool sa,
            bool sb,
            GameMode mode)
        {
            Assert.That(session.State.Tick, Is.EqualTo(tick));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(player));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(specter));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(index));
            AssertSaSb(session, sa, sb);
            Assert.That(session.State.Mode, Is.EqualTo(mode));
        }

        private static void AssertSaSb(GridPuzzleEngineSession session, bool sa, bool sb)
        {
            Assert.That(session.State.IsSwitchActivated(Sa), Is.EqualTo(sa));
            Assert.That(session.State.IsSwitchActivated(Sb), Is.EqualTo(sb));
        }
    }
}
