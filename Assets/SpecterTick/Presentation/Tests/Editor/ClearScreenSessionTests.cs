using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class ClearScreenSessionTests
    {
        private const string Clr3 =
            "CLEARED!\nTICK: 4  RECORDED: 5\nPRESS R TO RETRY";

        [Test]
        public void CS1_Demo_Initial_Hidden()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var model = ClearScreenModelBuilder.Build(session);

            Assert.That(model.IsVisible, Is.False);
            Assert.That(model.ToDisplayText(), Is.EqualTo(""));
        }

        [Test]
        public void CS2_Demo_ToCleared()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var dispatcher = new SessionInputDispatcher(session);

            for (var i = 0; i < 5; i++)
            {
                Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
                Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
                Assert.That(session.State.Tick, Is.EqualTo(i + 1));
            }

            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(ClearScreenModelBuilder.Build(session).IsVisible, Is.False);
            Assert.That(ClearScreenModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(""));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(ClearScreenModelBuilder.Build(session).IsVisible, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(ClearScreenModelBuilder.Build(session).IsVisible, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(2));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(2));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(ClearScreenModelBuilder.Build(session).IsVisible, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(3));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(3));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(ClearScreenModelBuilder.Build(session).IsVisible, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));

            var model = ClearScreenModelBuilder.Build(session);
            Assert.That(model.IsVisible, Is.True);
            Assert.That(model.ToDisplayText(), Is.EqualTo(Clr3));
        }

        [Test]
        public void CS3_SG_ClearedByConfirm()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.Create());
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(3));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(7));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(3));
            Assert.That(ClearScreenModelBuilder.Build(session).IsVisible, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(3));

            var model = ClearScreenModelBuilder.Build(session);
            Assert.That(model.IsVisible, Is.True);
            Assert.That(model.ToDisplayText(), Is.EqualTo("CLEARED!\nTICK: 0  RECORDED: 7\nPRESS R TO RETRY"));
        }

        [Test]
        public void CS4_Cleared_RejectsInput_ThenRetryHides()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var dispatcher = new SessionInputDispatcher(session);
            AdvanceDemoToCleared(dispatcher, session);

            Assert.That(ClearScreenModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(Clr3));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Left)), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(ClearScreenModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(Clr3));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(ClearScreenModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(Clr3));

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.RejectedByMode));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(ClearScreenModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(Clr3));

            Assert.That(dispatcher.Dispatch(InputIntent.Retry), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.X, Is.EqualTo(0));
            Assert.That(session.State.SpecterPosition.Y, Is.EqualTo(2));
            Assert.That(ClearScreenModelBuilder.Build(session).IsVisible, Is.False);
            Assert.That(ClearScreenModelBuilder.Build(session).ToDisplayText(), Is.EqualTo(""));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition.X, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition.Y, Is.EqualTo(1));
            Assert.That(ClearScreenModelBuilder.Build(session).IsVisible, Is.False);
        }

        [Test]
        public void CS5_NullSession()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                ClearScreenModelBuilder.Build((GridPuzzleEngineSession)null));
            Assert.That(ex.ParamName, Is.EqualTo("session"));
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
