using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies mode guards and Core dispatch only — not keyboard polling or on-screen movement.
    /// </summary>
    [TestFixture]
    public sealed class SessionInputDispatcherTests
    {
        [Test]
        public void M1_Recording_MoveExecutes_StepForwardRejected()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));

            var moveResult = dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up));
            Assert.That(moveResult, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            var stateBeforeReject = session.State;
            var rejectResult = dispatcher.Dispatch(InputIntent.StepForward);
            Assert.That(rejectResult, Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State, Is.SameAs(stateBeforeReject));
            Assert.That(session.State.Tick, Is.EqualTo(1));
        }

        [Test]
        public void M2_Playback_Guards_And_StepForward()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            var moveResult = dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up));
            Assert.That(moveResult, Is.EqualTo(InputResult.Executed));

            var confirmResult = dispatcher.Dispatch(InputIntent.ConfirmRecording);
            Assert.That(confirmResult, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));

            var stateBeforeMove = session.State;
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State, Is.SameAs(stateBeforeMove));

            var stateBeforeConfirm = session.State;
            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State, Is.SameAs(stateBeforeConfirm));

            var stepResult = dispatcher.Dispatch(InputIntent.StepForward);
            Assert.That(stepResult, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(1));
            Assert.That(session.IsPlaybackFinished, Is.True);

            var secondStep = dispatcher.Dispatch(InputIntent.StepForward);
            Assert.That(secondStep, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
        }

        [Test]
        public void M3_Retry_FromEveryMode()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            var moveResultA = dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up));
            Assert.That(moveResultA, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            var retryRecording = dispatcher.Dispatch(InputIntent.Retry);
            Assert.That(retryRecording, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            var moveResultB = dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up));
            Assert.That(moveResultB, Is.EqualTo(InputResult.Executed));
            var confirmResultB = dispatcher.Dispatch(InputIntent.ConfirmRecording);
            Assert.That(confirmResultB, Is.EqualTo(InputResult.Executed));
            var stepResultB = dispatcher.Dispatch(InputIntent.StepForward);
            Assert.That(stepResultB, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 1)));
            var retryPlayback = dispatcher.Dispatch(InputIntent.Retry);
            Assert.That(retryPlayback, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));

            var clearedSession = CreateClearedSession();
            var clearedDispatcher = new SessionInputDispatcher(clearedSession);
            var retryCleared = clearedDispatcher.Dispatch(InputIntent.Retry);
            Assert.That(retryCleared, Is.EqualTo(InputResult.Executed));
            Assert.That(clearedSession.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(clearedSession.State.Tick, Is.EqualTo(0));
            Assert.That(clearedSession.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(clearedSession.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));

            var moveAfterRetry = clearedDispatcher.Dispatch(InputIntent.Move(MoveDirection.Up));
            Assert.That(moveAfterRetry, Is.EqualTo(InputResult.Executed));
            Assert.That(clearedSession.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
        }

        [Test]
        public void M4_Cleared_RejectsAllButRetry()
        {
            var session = CreateSessionAtExitRecording();
            var dispatcher = new SessionInputDispatcher(session);

            var confirmResult = dispatcher.Dispatch(InputIntent.ConfirmRecording);
            Assert.That(confirmResult, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 3)));

            AssertReject(dispatcher, InputIntent.Move(MoveDirection.Up));
            AssertReject(dispatcher, InputIntent.Move(MoveDirection.Right));
            AssertReject(dispatcher, InputIntent.ConfirmRecording);
            AssertReject(dispatcher, InputIntent.StepForward);
        }

        [Test]
        public void M5_None_DoesNothing()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            var stateBefore = session.State;
            Assert.That(dispatcher.Dispatch(InputIntent.None), Is.EqualTo(InputResult.None));
            Assert.That(session.State, Is.SameAs(stateBefore));

            var moveResultC = dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up));
            Assert.That(moveResultC, Is.EqualTo(InputResult.Executed));
            var confirmResultC = dispatcher.Dispatch(InputIntent.ConfirmRecording);
            Assert.That(confirmResultC, Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));

            var stateBeforePlayback = session.State;
            Assert.That(dispatcher.Dispatch(InputIntent.None), Is.EqualTo(InputResult.None));
            Assert.That(session.State, Is.SameAs(stateBeforePlayback));
        }

        [Test]
        public void M6_InvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new SessionInputDispatcher(null));

            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);
            var stateBefore = session.State;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                dispatcher.Dispatch(InputIntent.Move((MoveDirection)99)));
            Assert.That(session.State, Is.SameAs(stateBefore));
        }

        private static void AssertReject(SessionInputDispatcher dispatcher, InputIntent intent)
        {
            var stateBefore = dispatcher.Session.State;
            Assert.That(dispatcher.Dispatch(intent), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(dispatcher.Session.State, Is.SameAs(stateBefore));
        }

        private static GridPuzzleEngineSession CreateSessionAtExitRecording()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.Create());
            session.Step(MoveDirection.Up);
            session.Step(MoveDirection.Up);
            session.Step(MoveDirection.Up);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(session.State.Tick, Is.EqualTo(7));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            return session;
        }

        private static GridPuzzleEngineSession CreateClearedSession()
        {
            var session = CreateSessionAtExitRecording();
            session.ConfirmRecording();
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            return session;
        }
    }
}
