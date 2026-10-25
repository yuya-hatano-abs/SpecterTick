using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class BoardElementsModelBuilderTests
    {
        [Test]
        public void E1_Demo_Initial()
        {
            var stage = SampleStageFactory.CreateDemo();
            var session = new GridPuzzleEngineSession(stage);
            var model = BoardElementsModelBuilder.Build(session);

            Assert.That(model.Elements.Count, Is.EqualTo(5));
            Assert.That(model.Elements[0].Kind, Is.EqualTo(BoardElementKind.Exit));
            Assert.That(model.Elements[0].Position.X, Is.EqualTo(5));
            Assert.That(model.Elements[0].Position.Y, Is.EqualTo(0));
            Assert.That(model.Elements[0].IsActive, Is.False);
            Assert.That(model.Elements[1].Kind, Is.EqualTo(BoardElementKind.Switch));
            Assert.That(model.Elements[1].Position.X, Is.EqualTo(1));
            Assert.That(model.Elements[1].Position.Y, Is.EqualTo(2));
            Assert.That(model.Elements[1].IsActive, Is.False);
            Assert.That(model.Elements[2].Kind, Is.EqualTo(BoardElementKind.Switch));
            Assert.That(model.Elements[2].Position.X, Is.EqualTo(4));
            Assert.That(model.Elements[2].Position.Y, Is.EqualTo(2));
            Assert.That(model.Elements[2].IsActive, Is.False);
            Assert.That(model.Elements[3].Kind, Is.EqualTo(BoardElementKind.Door));
            Assert.That(model.Elements[3].Position.X, Is.EqualTo(3));
            Assert.That(model.Elements[3].Position.Y, Is.EqualTo(2));
            Assert.That(model.Elements[3].IsActive, Is.False);
            Assert.That(model.Elements[4].Kind, Is.EqualTo(BoardElementKind.Door));
            Assert.That(model.Elements[4].Position.X, Is.EqualTo(5));
            Assert.That(model.Elements[4].Position.Y, Is.EqualTo(1));
            Assert.That(model.Elements[4].IsActive, Is.False);
        }

        [Test]
        public void E2_SG_ExitOpenWithoutSwitches()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            var model = BoardElementsModelBuilder.Build(session);

            Assert.That(model.Elements.Count, Is.EqualTo(1));
            Assert.That(model.Elements[0].Kind, Is.EqualTo(BoardElementKind.Exit));
            Assert.That(model.Elements[0].Position.X, Is.EqualTo(4));
            Assert.That(model.Elements[0].Position.Y, Is.EqualTo(3));
            Assert.That(model.Elements[0].IsActive, Is.True);
        }

        [Test]
        public void E3_Demo_Recording_PlayerLatchesSwitch()
        {
            var stage = SampleStageFactory.CreateDemo();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 2)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            var model = BoardElementsModelBuilder.Build(session);
            Assert.That(model.Elements.Count, Is.EqualTo(5));
            Assert.That(model.Elements[0].IsActive, Is.False);
            Assert.That(model.Elements[1].IsActive, Is.True);
            Assert.That(model.Elements[2].IsActive, Is.False);
            Assert.That(model.Elements[3].IsActive, Is.True);
            Assert.That(model.Elements[4].IsActive, Is.False);
        }

        [Test]
        public void E4_Demo_Confirm_ResetsLatch()
        {
            var stage = SampleStageFactory.CreateDemo();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Down)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            var modelBeforeConfirm = BoardElementsModelBuilder.Build(session);
            Assert.That(modelBeforeConfirm.Elements[1].IsActive, Is.True);
            Assert.That(modelBeforeConfirm.Elements[3].IsActive, Is.True);
            Assert.That(modelBeforeConfirm.Elements[0].IsActive, Is.False);
            Assert.That(modelBeforeConfirm.Elements[2].IsActive, Is.False);
            Assert.That(modelBeforeConfirm.Elements[4].IsActive, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));

            var modelAfterConfirm = BoardElementsModelBuilder.Build(session);
            Assert.That(modelAfterConfirm.Elements[0].IsActive, Is.False);
            Assert.That(modelAfterConfirm.Elements[1].IsActive, Is.False);
            Assert.That(modelAfterConfirm.Elements[2].IsActive, Is.False);
            Assert.That(modelAfterConfirm.Elements[3].IsActive, Is.False);
            Assert.That(modelAfterConfirm.Elements[4].IsActive, Is.False);
        }

        [Test]
        public void E5_Demo_Playback_ToCleared()
        {
            var stage = SampleStageFactory.CreateDemo();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(3, 0)));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(5));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(5, 0)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(5, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));

            var modelAfterConfirm = BoardElementsModelBuilder.Build(session);
            Assert.That(modelAfterConfirm.Elements[0].IsActive, Is.False);
            Assert.That(modelAfterConfirm.Elements[1].IsActive, Is.False);
            Assert.That(modelAfterConfirm.Elements[2].IsActive, Is.False);
            Assert.That(modelAfterConfirm.Elements[3].IsActive, Is.False);
            Assert.That(modelAfterConfirm.Elements[4].IsActive, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Playback));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(1, 2)));

            var modelTick1 = BoardElementsModelBuilder.Build(session);
            Assert.That(modelTick1.Elements[0].IsActive, Is.False);
            Assert.That(modelTick1.Elements[1].IsActive, Is.True);
            Assert.That(modelTick1.Elements[2].IsActive, Is.False);
            Assert.That(modelTick1.Elements[3].IsActive, Is.True);
            Assert.That(modelTick1.Elements[4].IsActive, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(2));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(2, 2)));

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(3));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(3, 2)));

            var modelTick3 = BoardElementsModelBuilder.Build(session);
            Assert.That(modelTick3.Elements[0].IsActive, Is.False);
            Assert.That(modelTick3.Elements[1].IsActive, Is.True);
            Assert.That(modelTick3.Elements[2].IsActive, Is.False);
            Assert.That(modelTick3.Elements[3].IsActive, Is.True);
            Assert.That(modelTick3.Elements[4].IsActive, Is.False);

            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(4));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(4, 2)));

            var modelCleared = BoardElementsModelBuilder.Build(session);
            Assert.That(modelCleared.Elements[0].IsActive, Is.True);
            Assert.That(modelCleared.Elements[1].IsActive, Is.True);
            Assert.That(modelCleared.Elements[2].IsActive, Is.True);
            Assert.That(modelCleared.Elements[3].IsActive, Is.True);
            Assert.That(modelCleared.Elements[4].IsActive, Is.True);
        }

        [Test]
        public void E6_Demo_Retry_AfterCleared_ResetsAll()
        {
            var stage = SampleStageFactory.CreateDemo();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            Assert.That(session.State.Tick, Is.EqualTo(4));

            var modelCleared = BoardElementsModelBuilder.Build(session);
            Assert.That(modelCleared.Elements[0].IsActive, Is.True);
            Assert.That(modelCleared.Elements[1].IsActive, Is.True);
            Assert.That(modelCleared.Elements[2].IsActive, Is.True);
            Assert.That(modelCleared.Elements[3].IsActive, Is.True);
            Assert.That(modelCleared.Elements[4].IsActive, Is.True);

            Assert.That(dispatcher.Dispatch(InputIntent.Retry), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlaybackIndex, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));

            var modelRetry = BoardElementsModelBuilder.Build(session);
            Assert.That(modelRetry.Elements[0].IsActive, Is.False);
            Assert.That(modelRetry.Elements[1].IsActive, Is.False);
            Assert.That(modelRetry.Elements[2].IsActive, Is.False);
            Assert.That(modelRetry.Elements[3].IsActive, Is.False);
            Assert.That(modelRetry.Elements[4].IsActive, Is.False);
        }

        [Test]
        public void E7_Build_NullSession()
        {
            Assert.Throws<ArgumentNullException>(() => BoardElementsModelBuilder.Build(null));
        }

        [Test]
        public void E8_Elements_IsReadOnly()
        {
            var stage = SampleStageFactory.CreateDemo();
            var session = new GridPuzzleEngineSession(stage);
            var model = BoardElementsModelBuilder.Build(session);

            var list = model.Elements as IList<BoardElement>;
            Assert.That(list, Is.Not.Null);
            Assert.Throws<NotSupportedException>(() => list[0] = default(BoardElement));
            Assert.That(model.Elements.Count, Is.EqualTo(5));
            Assert.That(model.Elements[0].Kind, Is.EqualTo(BoardElementKind.Exit));
            Assert.That(model.Elements[0].Position, Is.EqualTo(new GridPosition(5, 0)));
        }
    }
}
