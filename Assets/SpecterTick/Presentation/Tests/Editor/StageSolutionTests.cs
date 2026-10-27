using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Presentation;
using MoveDirection = SpecterTick.Core.MoveDirection;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies Core execution results for catalog stages. Does not guarantee puzzle quality or visuals.
    /// TWIN and TANGLE have no exhaustive proof of absence of shorter solutions (not verified here).
    /// </summary>
    [TestFixture]
    public sealed class StageSolutionTests
    {
        private static readonly SwitchId S1 = new SwitchId("S1");
        private static readonly SwitchId S2 = new SwitchId("S2");

        [Test]
        public void SS1_Walk_Solution()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(0));
            var dispatcher = new SessionInputDispatcher(session);

            AssertExecutedMove(dispatcher, MoveDirection.Up, 1, new GridPosition(0, 1));
            AssertExecutedMove(dispatcher, MoveDirection.Up, 2, new GridPosition(0, 2));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 3, new GridPosition(1, 2));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 4, new GridPosition(2, 2));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 5, new GridPosition(3, 2));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 0)));
        }

        [Test]
        public void SS2_Echo_Solution()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(1));
            var dispatcher = new SessionInputDispatcher(session);

            AssertExecutedMove(dispatcher, MoveDirection.Right, 1, new GridPosition(1, 0));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 2, new GridPosition(2, 0));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 3, new GridPosition(3, 0));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 4, new GridPosition(4, 0));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.False);

            AssertExecutedStepForward(dispatcher, 1, 1, new GridPosition(1, 2), false, false);
            AssertExecutedStepForward(dispatcher, 2, 2, new GridPosition(2, 2), false, false);
            AssertExecutedStepForward(dispatcher, 3, 3, new GridPosition(3, 2), true, false);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(3));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(4));
        }

        [Test]
        public void SS3_Gate_Solution()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(2));
            var dispatcher = new SessionInputDispatcher(session);

            AssertExecutedMove(dispatcher, MoveDirection.Up, 1, new GridPosition(2, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Up, 2, new GridPosition(2, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Left, 3, new GridPosition(2, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Left, 4, new GridPosition(2, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Down, 5, new GridPosition(2, 2));
            AssertExecutedMove(dispatcher, MoveDirection.Left, 6, new GridPosition(1, 2));
            AssertExecutedMove(dispatcher, MoveDirection.Down, 7, new GridPosition(1, 1));
            AssertExecutedMove(dispatcher, MoveDirection.Down, 8, new GridPosition(1, 0));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 0)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.False);

            AssertExecutedStepForward(dispatcher, 1, 1, new GridPosition(3, 1), false, false);
            AssertExecutedStepForward(dispatcher, 2, 2, new GridPosition(3, 2), false, false);
            AssertExecutedStepForward(dispatcher, 3, 3, new GridPosition(2, 2), false, false);
            AssertExecutedStepForward(dispatcher, 4, 4, new GridPosition(1, 2), false, false);
            AssertExecutedStepForward(dispatcher, 5, 5, new GridPosition(1, 1), false, false);
            AssertExecutedStepForward(dispatcher, 6, 6, new GridPosition(0, 1), true, false);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(6));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(6));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(8));
        }

        [Test]
        public void SS4_Twin_Solution()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(3));
            var dispatcher = new SessionInputDispatcher(session);

            for (var i = 0; i < 4; i++)
            {
                AssertExecutedMove(dispatcher, MoveDirection.Right, i + 1, new GridPosition(1, 3));
            }

            for (var i = 0; i < 3; i++)
            {
                AssertExecutedMove(dispatcher, MoveDirection.Up, 5 + i, new GridPosition(1, 3));
            }

            AssertExecutedMove(dispatcher, MoveDirection.Down, 8, new GridPosition(1, 2));
            AssertExecutedMove(dispatcher, MoveDirection.Down, 9, new GridPosition(1, 1));
            AssertExecutedMove(dispatcher, MoveDirection.Down, 10, new GridPosition(1, 0));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.False);
            Assert.That(session.State.IsSwitchActivated(S2), Is.False);

            AssertExecutedStepForward(dispatcher, 1, 1, new GridPosition(1, 0), false, false);
            AssertExecutedStepForward(dispatcher, 2, 2, new GridPosition(2, 0), false, false);
            AssertExecutedStepForward(dispatcher, 3, 3, new GridPosition(3, 0), false, false);
            AssertExecutedStepForward(dispatcher, 4, 4, new GridPosition(4, 0), false, false);
            AssertExecutedStepForward(dispatcher, 5, 5, new GridPosition(4, 1), false, false);
            AssertExecutedStepForward(dispatcher, 6, 6, new GridPosition(4, 2), true, false);
            AssertExecutedStepForward(dispatcher, 7, 7, new GridPosition(4, 3), true, true);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(7));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(7));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(10));
        }

        [Test]
        public void SS5_Tangle_Solution()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(4));
            var dispatcher = new SessionInputDispatcher(session);

            AssertExecutedMove(dispatcher, MoveDirection.Left, 1, new GridPosition(1, 2));
            AssertExecutedMove(dispatcher, MoveDirection.Left, 2, new GridPosition(0, 2));
            AssertExecutedMove(dispatcher, MoveDirection.Up, 3, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 4, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Up, 5, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 6, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 7, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Left, 8, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Up, 9, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Up, 10, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 11, new GridPosition(0, 3));
            AssertExecutedMove(dispatcher, MoveDirection.Right, 12, new GridPosition(0, 3));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.False);
            Assert.That(session.State.IsSwitchActivated(S2), Is.False);

            AssertExecutedStepForward(dispatcher, 1, 1, new GridPosition(0, 0), false, false);
            AssertExecutedStepForward(dispatcher, 2, 2, new GridPosition(0, 0), false, false);
            AssertExecutedStepForward(dispatcher, 3, 3, new GridPosition(0, 0), false, false);
            AssertExecutedStepForward(dispatcher, 4, 4, new GridPosition(1, 0), false, false);
            AssertExecutedStepForward(dispatcher, 5, 5, new GridPosition(1, 1), false, false);
            AssertExecutedStepForward(dispatcher, 6, 6, new GridPosition(2, 1), false, false);
            AssertExecutedStepForward(dispatcher, 7, 7, new GridPosition(3, 1), true, false);
            AssertExecutedStepForward(dispatcher, 8, 8, new GridPosition(2, 1), true, false);
            AssertExecutedStepForward(dispatcher, 9, 9, new GridPosition(2, 2), true, false);
            AssertExecutedStepForward(dispatcher, 10, 10, new GridPosition(2, 3), true, false);
            AssertExecutedStepForward(dispatcher, 11, 11, new GridPosition(3, 3), true, false);
            AssertExecutedStepForward(dispatcher, 12, 12, new GridPosition(4, 3), true, true);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(12));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(12));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(12));
        }

        [Test]
        public void SN1_Walk_Prefix_And_Empty()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(0));
            var dispatcher = new SessionInputDispatcher(session);
            RecordMoves(dispatcher, "UURR");
            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            PlaybackToFinish(dispatcher, session);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 2)));

            Assert.That(dispatcher.Dispatch(InputIntent.Retry), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 0)));
            Assert.That(session.IsPlaybackFinished, Is.True);
        }

        [Test]
        public void SN2_Echo_Prefix_And_Empty()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(1));
            var dispatcher = new SessionInputDispatcher(session);
            RecordMoves(dispatcher, "RRR");
            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            PlaybackToFinish(dispatcher, session);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(3, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.True);

            Assert.That(dispatcher.Dispatch(InputIntent.Retry), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.False);
            Assert.That(session.IsPlaybackFinished, Is.True);
        }

        [Test]
        public void SN3_Gate_Prefix_And_Empty()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(2));
            var dispatcher = new SessionInputDispatcher(session);
            RecordMoves(dispatcher, "UULLDLD");
            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            PlaybackToFinish(dispatcher, session);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(7));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(7));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.True);

            Assert.That(dispatcher.Dispatch(InputIntent.Retry), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 0)));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 0)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.False);
            Assert.That(session.IsPlaybackFinished, Is.True);
        }

        [Test]
        public void SN4_Twin_Prefix_And_Empty()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(3));
            var dispatcher = new SessionInputDispatcher(session);
            RecordMoves(dispatcher, "RRRRUUUDD");
            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            PlaybackToFinish(dispatcher, session);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(9));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(9));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 1)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.True);
            Assert.That(session.State.IsSwitchActivated(S2), Is.True);

            Assert.That(dispatcher.Dispatch(InputIntent.Retry), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.False);
            Assert.That(session.State.IsSwitchActivated(S2), Is.False);
            Assert.That(session.IsPlaybackFinished, Is.True);
        }

        [Test]
        public void SN5_Tangle_Prefix_And_Empty()
        {
            var session = new GridPuzzleEngineSession(StageCatalog.CreateStage(4));
            var dispatcher = new SessionInputDispatcher(session);
            RecordMoves(dispatcher, "LLURURRLUUR");
            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            PlaybackToFinish(dispatcher, session);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(11));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(11));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 3)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.True);

            Assert.That(dispatcher.Dispatch(InputIntent.Retry), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.RecordedCommands.Count, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.IsSwitchActivated(S1), Is.False);
            Assert.That(session.State.IsSwitchActivated(S2), Is.False);
            Assert.That(session.IsPlaybackFinished, Is.True);
        }

        [Test]
        public void SX1_Walk_ExhaustiveShortSolutions()
        {
            var stage = StageCatalog.CreateStage(0);
            var session = new GridPuzzleEngineSession(stage);
            var counts = new int[6];
            var solutions = new List<string>();
            var total = 0;
            var sw = Stopwatch.StartNew();
            EnumerateSolutions(session, string.Empty, 5, counts, solutions, ref total);
            sw.Stop();
            TestContext.WriteLine("SX1_MS=" + sw.ElapsedMilliseconds);
            Assert.That(total, Is.EqualTo(1365));
            Assert.That(counts, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 2 }));
            Assert.That(solutions, Is.EqualTo(new[] { "RRRUU", "UURRR" }));
        }

        [Test]
        public void SX2_Echo_ExhaustiveShortSolutions()
        {
            var stage = StageCatalog.CreateStage(1);
            var session = new GridPuzzleEngineSession(stage);
            var counts = new int[5];
            var solutions = new List<string>();
            var total = 0;
            var sw = Stopwatch.StartNew();
            EnumerateSolutions(session, string.Empty, 4, counts, solutions, ref total);
            sw.Stop();
            TestContext.WriteLine("SX2_MS=" + sw.ElapsedMilliseconds);
            Assert.That(total, Is.EqualTo(341));
            Assert.That(counts, Is.EqualTo(new[] { 0, 0, 0, 0, 1 }));
            Assert.That(solutions, Is.EqualTo(new[] { "RRRR" }));
        }

        [Test]
        public void SX3_Gate_ExhaustiveShortSolutions()
        {
            var stage = StageCatalog.CreateStage(2);
            var session = new GridPuzzleEngineSession(stage);
            var counts = new int[9];
            var solutions = new List<string>();
            var total = 0;
            var sw = Stopwatch.StartNew();
            EnumerateSolutions(session, string.Empty, 8, counts, solutions, ref total);
            sw.Stop();
            TestContext.WriteLine("SX3_MS=" + sw.ElapsedMilliseconds);
            Assert.That(total, Is.EqualTo(87381));
            Assert.That(counts, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 0, 0, 1 }));
            Assert.That(solutions, Is.EqualTo(new[] { "UULLDLDD" }));
        }

        private static void AssertExecutedMove(
            SessionInputDispatcher dispatcher,
            MoveDirection direction,
            int expectedTick,
            GridPosition expectedPlayer)
        {
            Assert.That(dispatcher.Dispatch(InputIntent.Move(direction)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(dispatcher.Session.State.Tick, Is.EqualTo(expectedTick));
            Assert.That(dispatcher.Session.State.PlayerPosition, Is.EqualTo(expectedPlayer));
        }

        private static void AssertExecutedStepForward(
            SessionInputDispatcher dispatcher,
            int expectedTick,
            int expectedPlaybackIndex,
            GridPosition expectedSpecter,
            bool s1,
            bool s2)
        {
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Session.State.Tick, Is.EqualTo(expectedTick));
            Assert.That(dispatcher.Session.State.PlaybackIndex, Is.EqualTo(expectedPlaybackIndex));
            Assert.That(dispatcher.Session.State.SpecterPosition, Is.EqualTo(expectedSpecter));
            Assert.That(dispatcher.Session.State.IsSwitchActivated(S1), Is.EqualTo(s1));
            Assert.That(dispatcher.Session.State.IsSwitchActivated(S2), Is.EqualTo(s2));
        }

        private static void RecordMoves(SessionInputDispatcher dispatcher, string moves)
        {
            for (var i = 0; i < moves.Length; i++)
            {
                Assert.That(
                    dispatcher.Dispatch(InputIntent.Move(ToDirection(moves[i]))),
                    Is.EqualTo(InputResult.Executed));
            }
        }

        private static void PlaybackToFinish(SessionInputDispatcher dispatcher, GridPuzzleEngineSession session)
        {
            while (!session.IsPlaybackFinished)
            {
                Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            }
        }

        private static MoveDirection ToDirection(char c)
        {
            switch (c)
            {
                case 'U':
                    return MoveDirection.Up;
                case 'D':
                    return MoveDirection.Down;
                case 'L':
                    return MoveDirection.Left;
                case 'R':
                    return MoveDirection.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(c), c, "Unknown direction.");
            }
        }

        private static void EnumerateSolutions(
            GridPuzzleEngineSession session,
            string prefix,
            int maxLen,
            int[] counts,
            List<string> solutions,
            ref int total)
        {
            total++;
            session.Retry();
            for (var i = 0; i < prefix.Length; i++)
            {
                session.Step(ToDirection(prefix[i]));
            }

            session.ConfirmRecording();
            if (session.State.Mode == GameMode.Cleared)
            {
                counts[prefix.Length]++;
                if (prefix.Length == maxLen)
                {
                    solutions.Add(prefix);
                }

                return;
            }

            while (session.State.Mode == GameMode.Playback && !session.IsPlaybackFinished)
            {
                session.StepPlayback();
            }

            if (session.State.Mode == GameMode.Cleared)
            {
                counts[prefix.Length]++;
                if (prefix.Length == maxLen)
                {
                    solutions.Add(prefix);
                }

                return;
            }

            if (prefix.Length >= maxLen)
            {
                return;
            }

            var dirs = new[] { 'D', 'L', 'R', 'U' };
            for (var d = 0; d < dirs.Length; d++)
            {
                EnumerateSolutions(session, prefix + dirs[d], maxLen, counts, solutions, ref total);
            }
        }
    }
}
