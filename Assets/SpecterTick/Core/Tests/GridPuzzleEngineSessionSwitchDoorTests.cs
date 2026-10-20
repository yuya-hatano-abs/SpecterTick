using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Tests.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class GridPuzzleEngineSessionSwitchDoorTests
    {
        private static readonly GridPosition SpecterF1 = new GridPosition(3, 3);
        private static readonly GridPosition SpecterF2 = new GridPosition(4, 4);
        private static readonly SwitchId S1 = new SwitchId("S1");
        private static readonly DoorId D1 = new DoorId("D1");
        private static readonly SwitchId Sa = new SwitchId("SA");
        private static readonly SwitchId Sb = new SwitchId("SB");
        private static readonly DoorId Da = new DoorId("DA");
        private static readonly DoorId Dab = new DoorId("DAB");

        [Test]
        public void F2StageDefinition_PassesStageValidator()
        {
            Assert.That(StageValidator.Validate(F2StageDefinitionBuilder.CreateDefinition()).IsSuccess, Is.True);
        }

        [Test]
        public void Session_ZeroSwitches_AllSwitchesActivatedAndExitOpen_AreTrueAtTickZero()
        {
            var stage = StageValidator.Validate(StageDefinitionTestBuilder.CreateMinimalValid()).Stage;
            var session = new GridPuzzleEngineSession(stage);
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.AllSwitchesActivated, Is.True);
            Assert.That(session.IsExitOpen, Is.True);
        }

        [Test]
        public void Step_F1_GoldenSequenceG4_AllEightSteps_SwitchDoorExitQueries()
        {
            var session = new GridPuzzleEngineSession(F1StageDefinitionBuilder.CreateValidatedStage());
            AssertF1G4Tick(session, 0, new GridPosition(1, 1), false, false, false, false);

            session.Step(MoveDirection.Up);
            AssertF1G4Tick(session, 1, new GridPosition(1, 1), false, false, false, false);

            session.Step(MoveDirection.Left);
            AssertF1G4Tick(session, 2, new GridPosition(0, 1), false, false, false, false);

            session.Step(MoveDirection.Up);
            AssertF1G4Tick(session, 3, new GridPosition(0, 2), false, false, false, false);

            session.Step(MoveDirection.Up);
            AssertF1G4Tick(session, 4, new GridPosition(0, 3), false, false, false, false);

            session.Step(MoveDirection.Up);
            AssertF1G4Tick(session, 5, new GridPosition(0, 4), true, true, true, true);

            session.Step(MoveDirection.Down);
            AssertF1G4Tick(session, 6, new GridPosition(0, 3), true, true, true, true);

            session.Step(MoveDirection.Down);
            AssertF1G4Tick(session, 7, new GridPosition(0, 2), true, true, true, true);

            session.Step(MoveDirection.Right);
            AssertF1G4Tick(session, 8, new GridPosition(1, 2), true, true, true, true);
        }

        [Test]
        public void Step_F2_GoldenSequenceG5_SevenSteps_SwitchAndDoorStates()
        {
            var session = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            session.Step(MoveDirection.Right);
            AssertF2G5(session, 1, new GridPosition(1, 0), false, false, false, false, false);

            session.Step(MoveDirection.Right);
            AssertF2G5(session, 2, new GridPosition(2, 0), false, false, false, false, false);

            session.Step(MoveDirection.Up);
            AssertF2G5(session, 3, new GridPosition(2, 1), false, true, false, false, false);

            session.Step(MoveDirection.Up);
            AssertF2G5(session, 4, new GridPosition(2, 1), false, true, false, false, false);

            session.Step(MoveDirection.Left);
            AssertF2G5(session, 5, new GridPosition(1, 1), false, true, false, false, false);

            session.Step(MoveDirection.Left);
            AssertF2G5(session, 6, new GridPosition(0, 1), true, true, true, true, true);

            session.Step(MoveDirection.Up);
            AssertF2G5(session, 7, new GridPosition(0, 2), true, true, true, true, true);
        }

        [Test]
        public void Step_F2_GoldenSequenceG6_TwoSteps_OnlyDaOpens()
        {
            var session = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(session.State.IsSwitchActivated(Sb), Is.False);
            Assert.That(session.IsDoorOpen(Da), Is.True);
            Assert.That(session.IsDoorOpen(Dab), Is.False);
            Assert.That(session.AllSwitchesActivated, Is.False);

            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.IsDoorOpen(Da), Is.True);
            Assert.That(session.IsDoorOpen(Dab), Is.False);
        }

        [Test]
        public void Step_F2_GoldenSequenceG7_PathPAndQ_Tick7StatesEqual_HashEqual()
        {
            var pathP = new[]
            {
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Left,
                MoveDirection.Right,
                MoveDirection.Left,
                MoveDirection.Right
            };
            var pathQ = new[]
            {
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Up,
                MoveDirection.Left,
                MoveDirection.Left,
                MoveDirection.Right,
                MoveDirection.Right
            };

            var stateP = RunF2ToTick(pathP);
            var stateQ = RunF2ToTick(pathQ);
            Assert.That(stateP, Is.EqualTo(stateQ));
            Assert.That(stateP.GetHashCode(), Is.EqualTo(stateQ.GetHashCode()));
            Assert.That(stateP.PlayerPosition, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(stateP.IsSwitchActivated(Sa), Is.True);
            Assert.That(stateP.IsSwitchActivated(Sb), Is.True);
            Assert.That(stateP.SpecterPosition, Is.EqualTo(SpecterF2));
        }

        [Test]
        public void Step_F2_EnterExit_WhileSwitchesInactive_MovesSuccessfully()
        {
            var definition = F2StageDefinitionBuilder.CreateDefinition();
            definition.PlayerPosition = new GridPosition(3, 0);
            var stage = StageValidator.Validate(definition).Stage;
            var session = new GridPuzzleEngineSession(stage);
            session.Step(MoveDirection.Right);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.AllSwitchesActivated, Is.False);
            Assert.That(session.IsExitOpen, Is.False);
        }

        [Test]
        public void Step_F2_EnterExit_AllSwitchesActive_OnlyPlayerAndTickChange()
        {
            var session = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            RunF2GoldenG5(session);
            var activationBefore = session.State.ActivatedSwitches;
            var specterBefore = session.State.SpecterPosition;
            var tickBefore = session.State.Tick;
            session.Step(MoveDirection.Down);
            session.Step(MoveDirection.Down);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.Tick, Is.EqualTo(tickBefore + 6));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(specterBefore));
            Assert.That(session.State.ActivatedSwitches.Equals(activationBefore), Is.True);
        }

        [Test]
        public void Step_F2_RejectedMoveIntoClosedDoor_TickAdvances_ActivationUnchanged()
        {
            var session = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Up);
            var activationBefore = session.State.ActivatedSwitches;
            var playerBefore = session.State.PlayerPosition;
            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(playerBefore));
            Assert.That(session.State.Tick, Is.EqualTo(4));
            Assert.That(session.State.ActivatedSwitches.Equals(activationBefore), Is.True);
        }

        [Test]
        public void Step_AfterInvalidDirection_StateIncludingActivationUnchanged_SameReference()
        {
            var session = new GridPuzzleEngineSession(F1StageDefinitionBuilder.CreateValidatedStage());
            session.Step(MoveDirection.Left);
            session.Step(MoveDirection.Up);
            session.Step(MoveDirection.Up);
            session.Step(MoveDirection.Up);
            Assert.That(session.State.IsSwitchActivated(S1), Is.True);
            var before = session.State;
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Step((MoveDirection)0));
            Assert.That(session.State, Is.SameAs(before));
            Assert.That(session.State.IsSwitchActivated(S1), Is.True);
        }

        [Test]
        public void TwoSessions_OnlyOneActivatesSwitch_OtherUnchanged()
        {
            var a = new GridPuzzleEngineSession(F1StageDefinitionBuilder.CreateValidatedStage());
            var b = new GridPuzzleEngineSession(F1StageDefinitionBuilder.CreateValidatedStage());
            a.Step(MoveDirection.Left);
            a.Step(MoveDirection.Up);
            a.Step(MoveDirection.Up);
            a.Step(MoveDirection.Up);

            Assert.That(a.State.IsSwitchActivated(S1), Is.True);
            Assert.That(b.State.IsSwitchActivated(S1), Is.False);
            Assert.That(b.State.Tick, Is.EqualTo(0));
        }

        [Test]
        public void Step_AfterMutatingStageDefinition_SessionBehaviorUnchanged()
        {
            var definition = F1StageDefinitionBuilder.CreateDefinition();
            var stage = StageValidator.Validate(definition).Stage;
            var session = new GridPuzzleEngineSession(stage);
            definition.Switches[0] = new SwitchPlacement(new SwitchId("S1"), new GridPosition(4, 4));
            definition.Doors[0] = new DoorPlacement(
                new DoorId("D1"),
                new GridPosition(4, 3),
                new List<SwitchId> { new SwitchId("S1") });
            definition.ExitPosition = new GridPosition(0, 0);

            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.IsDoorOpen(D1), Is.False);
        }

        [Test]
        public void Step_UsesLayoutSnapshot_NotLiveStageSwitchPosition_UpActivatesSnapshotSwitch()
        {
            var inner = F2StageDefinitionBuilder.CreateValidatedStage();
            var mutable = new MutableStageLayoutSource(inner);
            var session = new GridPuzzleEngineSession(mutable);
            mutable.MoveSwitchSaTo(new GridPosition(4, 4));

            session.Step(MoveDirection.Up);
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
        }

        [Test]
        public void Step_UsesLayoutSnapshot_NotLiveStageDoorReferences_DabStaysClosedWithSnapshotAnd()
        {
            var inner = F2StageDefinitionBuilder.CreateValidatedStage();
            var mutable = new MutableStageLayoutSource(inner);
            var session = new GridPuzzleEngineSession(mutable);
            mutable.ReplaceDoorDabReferences(new[] { Sa });

            session.Step(MoveDirection.Up);
            Assert.That(session.State.IsSwitchActivated(Sa), Is.True);
            Assert.That(session.IsDoorOpen(Dab), Is.False);
        }

        [Test]
        public void Step_UsesLayoutSnapshot_NotLiveStageDoorPosition_ClosedDabBlocksUpFrom21()
        {
            var inner = F2StageDefinitionBuilder.CreateValidatedStage();
            var mutable = new MutableStageLayoutSource(inner);
            var session = new GridPuzzleEngineSession(mutable);
            mutable.MoveDoorDabTo(new GridPosition(4, 3));

            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(session.State.IsSwitchActivated(Sb), Is.True);

            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(4));
        }

        [Test]
        public void TwoSessions_SameInputs_AllTicksProduceEqualStates()
        {
            var directions = new[]
            {
                MoveDirection.Up,
                MoveDirection.Left,
                MoveDirection.Up,
                MoveDirection.Up,
                MoveDirection.Up
            };
            var recorded = new GameState[directions.Length];
            var a = new GridPuzzleEngineSession(F1StageDefinitionBuilder.CreateValidatedStage());
            for (var i = 0; i < directions.Length; i++)
            {
                a.Step(directions[i]);
                recorded[i] = a.State;
            }

            var b = new GridPuzzleEngineSession(F1StageDefinitionBuilder.CreateValidatedStage());
            for (var i = 0; i < directions.Length; i++)
            {
                b.Step(directions[i]);
                Assert.That(b.State, Is.EqualTo(recorded[i]));
            }
        }

        [Test]
        public void IsDoorOpen_InvalidDoorId_ThrowsArgumentException()
        {
            var session = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            var ex = Assert.Throws<ArgumentException>(() => session.IsDoorOpen(default(DoorId)));
            Assert.That(ex.ParamName, Is.EqualTo("doorId"));
        }

        [Test]
        public void IsDoorOpen_UnknownDoorId_ThrowsArgumentException()
        {
            var session = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            var ex = Assert.Throws<ArgumentException>(() => session.IsDoorOpen(new DoorId("missing")));
            Assert.That(ex.ParamName, Is.EqualTo("doorId"));
        }

        private static GameState RunF2ToTick(MoveDirection[] directions)
        {
            var session = new GridPuzzleEngineSession(F2StageDefinitionBuilder.CreateValidatedStage());
            for (var i = 0; i < directions.Length; i++)
            {
                session.Step(directions[i]);
            }

            return session.State;
        }

        private static void RunF2GoldenG5(GridPuzzleEngineSession session)
        {
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Right);
            session.Step(MoveDirection.Up);
            session.Step(MoveDirection.Up);
            session.Step(MoveDirection.Left);
            session.Step(MoveDirection.Left);
            session.Step(MoveDirection.Up);
        }

        private static void AssertF1G4Tick(
            GridPuzzleEngineSession session,
            int tick,
            GridPosition player,
            bool s1,
            bool d1Open,
            bool all,
            bool exitOpen)
        {
            Assert.That(session.State.Tick, Is.EqualTo(tick));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(player));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1));
            Assert.That(session.State.IsSwitchActivated(S1), Is.EqualTo(s1));
            Assert.That(session.IsDoorOpen(D1), Is.EqualTo(d1Open));
            Assert.That(session.AllSwitchesActivated, Is.EqualTo(all));
            Assert.That(session.IsExitOpen, Is.EqualTo(exitOpen));
        }

        private static void AssertF2G5(
            GridPuzzleEngineSession session,
            int tick,
            GridPosition player,
            bool sa,
            bool sb,
            bool da,
            bool dab,
            bool all)
        {
            Assert.That(session.State.Tick, Is.EqualTo(tick));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(player));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF2));
            Assert.That(session.State.IsSwitchActivated(Sa), Is.EqualTo(sa));
            Assert.That(session.State.IsSwitchActivated(Sb), Is.EqualTo(sb));
            Assert.That(session.IsDoorOpen(Da), Is.EqualTo(da));
            Assert.That(session.IsDoorOpen(Dab), Is.EqualTo(dab));
            Assert.That(session.AllSwitchesActivated, Is.EqualTo(all));
            Assert.That(session.IsExitOpen, Is.EqualTo(all));
        }

        private sealed class MutableStageLayoutSource : IStage
        {
            private readonly IStage _inner;
            private GridPosition _saPosition;
            private GridPosition _dabPosition;
            private IReadOnlyList<SwitchId> _dabReferences;

            public MutableStageLayoutSource(IStage inner)
            {
                _inner = inner;
                _saPosition = inner.Switches[0].Position;
                _dabPosition = inner.Doors[1].Position;
                _dabReferences = inner.Doors[1].SwitchReferences;
            }

            public int Width => _inner.Width;
            public int Height => _inner.Height;
            public GridPosition PlayerPosition => _inner.PlayerPosition;
            public GridPosition SpecterPosition => _inner.SpecterPosition;
            public GridPosition ExitPosition => _inner.ExitPosition;
            public IReadOnlyList<GridPosition> Walls => _inner.Walls;
            public IReadOnlyList<StageSwitch> Switches => new[]
            {
                new StageSwitch(new SwitchId("SA"), _saPosition),
                _inner.Switches[1]
            };
            public IReadOnlyList<StageDoor> Doors => new[]
            {
                _inner.Doors[0],
                new StageDoor(new DoorId("DAB"), _dabPosition, ToArray(_dabReferences))
            };

            public void MoveSwitchSaTo(GridPosition position)
            {
                _saPosition = position;
            }

            public void MoveDoorDabTo(GridPosition position)
            {
                _dabPosition = position;
            }

            public void ReplaceDoorDabReferences(SwitchId[] references)
            {
                _dabReferences = references;
            }

            private static SwitchId[] ToArray(IReadOnlyList<SwitchId> references)
            {
                var copy = new SwitchId[references.Count];
                for (var i = 0; i < references.Count; i++)
                {
                    copy[i] = references[i];
                }

                return copy;
            }
        }
    }
}
