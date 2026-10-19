using System;
using System.Collections;
using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Tests.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class GridPuzzleEngineSessionTests
    {
        private static readonly GridPosition SpecterF1 = new GridPosition(3, 3);

        [Test]
        public void F1StageDefinition_PassesStageValidator()
        {
            var result = StageValidator.Validate(F1StageDefinitionBuilder.CreateDefinition());
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Session_StartsAtF1InitialState()
        {
            var session = CreateF1Session();
            Assert.That(session.State.Tick, Is.EqualTo(0));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1));
        }

        [Test]
        public void Session_Constructor_NullStage_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new GridPuzzleEngineSession(null));
            Assert.That(ex.ParamName, Is.EqualTo("stage"));
        }

        [Test]
        public void Step_RightFromStart_BlockedByWall_StaysAt11_Tick1()
        {
            var session = CreateF1Session();
            session.Step(MoveDirection.Right);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1));
        }

        [Test]
        public void Step_UpFromStart_BlockedByDoor_StaysAt11_Tick1()
        {
            var session = CreateF1Session();
            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
        }

        [Test]
        public void Step_LeftFromStart_MovesTo01_Tick1()
        {
            var session = CreateF1Session();
            session.Step(MoveDirection.Left);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
        }

        [Test]
        public void Step_F1_TwoLefts_SecondRejectedAtBoundary_Tick2()
        {
            var session = CreateF1Session();
            session.Step(MoveDirection.Left);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1));

            session.Step(MoveDirection.Left);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1));
        }

        [Test]
        public void Step_DownFromStart_MovesTo10_Tick1()
        {
            var session = CreateF1Session();
            session.Step(MoveDirection.Down);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
        }

        [Test]
        public void Step_GoldenSequence_AllSixSteps_PlayerTickSpecter()
        {
            var session = CreateF1Session();
            var directions = new[]
            {
                MoveDirection.Left,
                MoveDirection.Down,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Up,
                MoveDirection.Up
            };

            var expectedPlayers = new[]
            {
                new GridPosition(0, 1),
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0),
                new GridPosition(2, 0),
                new GridPosition(2, 0)
            };

            for (var i = 0; i < directions.Length; i++)
            {
                session.Step(directions[i]);
                Assert.That(session.State.Tick, Is.EqualTo(i + 1), "Tick after step " + (i + 1));
                Assert.That(session.State.PlayerPosition, Is.EqualTo(expectedPlayers[i]), "Player after step " + (i + 1));
                Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1), "Specter after step " + (i + 1));
            }
        }

        [Test]
        public void Step_F1_GoldenSequence_NewSessionReplay_ProducesEqualStates()
        {
            var directions = new[]
            {
                MoveDirection.Left,
                MoveDirection.Down,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Up,
                MoveDirection.Up
            };

            var recorded = new GameState[directions.Length];
            var sessionA = CreateF1Session();
            for (var i = 0; i < directions.Length; i++)
            {
                sessionA.Step(directions[i]);
                recorded[i] = sessionA.State;
            }

            var sessionB = CreateF1Session();
            for (var i = 0; i < directions.Length; i++)
            {
                sessionB.Step(directions[i]);
                Assert.That(sessionB.State, Is.EqualTo(recorded[i]), "State after step " + (i + 1));
            }
        }

        [Test]
        public void Step_PlayerCanEnterSpecterCell_NoSpecialHandling()
        {
            var stage = F1StageDefinitionBuilder.CreateValidatedStage(new GridPosition(2, 3));
            var session = new GridPuzzleEngineSession(stage);
            session.Step(MoveDirection.Right);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(SpecterF1));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1));
            Assert.That(session.State.Tick, Is.EqualTo(1));
        }

        [Test]
        public void Step_ExitTile_FromAdjacent_EntersExitCell_TickAdvances_SpecterUnchanged()
        {
            var stage = F1StageDefinitionBuilder.CreateValidatedStage(new GridPosition(4, 3));
            var session = new GridPuzzleEngineSession(stage);
            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 4)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1));
        }

        [Test]
        public void Step_InvalidDirectionZero_ThrowsAndLeavesStateUnchanged()
        {
            var session = CreateF1Session();
            var before = session.State;
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => session.Step((MoveDirection)0));
            Assert.That(ex.ParamName, Is.EqualTo("direction"));
            Assert.That(session.State, Is.EqualTo(before));
        }

        [Test]
        public void Step_InvalidDirectionFive_ThrowsAndLeavesStateUnchanged()
        {
            var session = CreateF1Session();
            var before = session.State;
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => session.Step((MoveDirection)5));
            Assert.That(ex.ParamName, Is.EqualTo("direction"));
            Assert.That(session.State, Is.EqualTo(before));
        }

        [Test]
        public void Step_AfterInvalidDirection_RecoveryStepStillWorks()
        {
            var session = CreateF1Session();
            Assert.Throws<ArgumentOutOfRangeException>(() => session.Step((MoveDirection)0));

            session.Step(MoveDirection.Left);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
        }

        [Test]
        public void Step_ReplacesStateWithNewInstance()
        {
            var session = CreateF1Session();
            var before = session.State;
            session.Step(MoveDirection.Left);
            Assert.That(session.State, Is.Not.SameAs(before));
        }

        [Test]
        public void TwoSessions_SameCommands_ProduceSameStates()
        {
            var a = CreateF1Session();
            var b = CreateF1Session();
            a.Step(MoveDirection.Left);
            b.Step(MoveDirection.Left);
            a.Step(MoveDirection.Down);
            b.Step(MoveDirection.Down);
            Assert.That(a.State, Is.EqualTo(b.State));
        }

        [Test]
        public void TwoSessions_OneSteps_OtherUnchanged()
        {
            var a = CreateF1Session();
            var b = CreateF1Session();
            a.Step(MoveDirection.Left);
            Assert.That(b.State.Tick, Is.EqualTo(0));
            Assert.That(b.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 1)));
        }

        [Test]
        public void Session_ImplementsIGameEngine_StageReferenceMatchesInput()
        {
            var stage = F1StageDefinitionBuilder.CreateValidatedStage();
            IGameEngine engine = new GridPuzzleEngineSession(stage);
            Assert.That(engine.Stage, Is.SameAs(stage));
        }

        [Test]
        public void Step_UsesLayoutSnapshot_NotLiveStageDimensions()
        {
            var inner = F1StageDefinitionBuilder.CreateValidatedStage(new GridPosition(3, 0));
            var mutable = new MutableWidthStage(inner, inner.Width);
            var session = new GridPuzzleEngineSession(mutable);
            mutable.SetWidth(1);

            session.Step(MoveDirection.Right);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
        }

        [Test]
        public void Step_UsesLayoutSnapshot_NotLiveStageHeight()
        {
            var inner = F1StageDefinitionBuilder.CreateValidatedStage(new GridPosition(0, 3));
            var mutable = new MutableWidthStage(inner, inner.Width);
            var session = new GridPuzzleEngineSession(mutable);
            mutable.SetHeight(1);

            session.Step(MoveDirection.Up);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 4)));
            Assert.That(session.State.Tick, Is.EqualTo(1));
        }

        [TestCaseSource(nameof(BoundaryCases))]
        public void Step_BoundaryMatrix(
            GridPosition start,
            MoveDirection direction,
            GridPosition expectedPlayer,
            int expectedTick)
        {
            var stage = F1StageDefinitionBuilder.CreateValidatedStage(start);
            var session = new GridPuzzleEngineSession(stage);
            session.Step(direction);
            Assert.That(session.State.PlayerPosition, Is.EqualTo(expectedPlayer));
            Assert.That(session.State.Tick, Is.EqualTo(expectedTick));
            Assert.That(session.State.SpecterPosition, Is.EqualTo(SpecterF1));
        }

        private static IEnumerable BoundaryCases()
        {
            var specter = SpecterF1;
            yield return Case(new GridPosition(3, 0), MoveDirection.Down, new GridPosition(3, 0), 1);
            yield return Case(new GridPosition(3, 0), MoveDirection.Up, new GridPosition(3, 1), 1);
            yield return Case(new GridPosition(3, 0), MoveDirection.Left, new GridPosition(2, 0), 1);
            yield return Case(new GridPosition(3, 0), MoveDirection.Right, new GridPosition(4, 0), 1);

            yield return Case(new GridPosition(3, 4), MoveDirection.Up, new GridPosition(3, 4), 1);
            yield return Case(new GridPosition(3, 4), MoveDirection.Down, specter, 1);
            yield return Case(new GridPosition(3, 4), MoveDirection.Left, new GridPosition(2, 4), 1);
            yield return Case(new GridPosition(3, 4), MoveDirection.Right, new GridPosition(4, 4), 1);

            yield return Case(new GridPosition(0, 3), MoveDirection.Left, new GridPosition(0, 3), 1);
            yield return Case(new GridPosition(0, 3), MoveDirection.Right, new GridPosition(1, 3), 1);
            yield return Case(new GridPosition(0, 3), MoveDirection.Up, new GridPosition(0, 4), 1);
            yield return Case(new GridPosition(0, 3), MoveDirection.Down, new GridPosition(0, 2), 1);

            yield return Case(new GridPosition(4, 3), MoveDirection.Right, new GridPosition(4, 3), 1);
            yield return Case(new GridPosition(4, 3), MoveDirection.Left, specter, 1);
            yield return Case(new GridPosition(4, 3), MoveDirection.Up, new GridPosition(4, 4), 1);
            yield return Case(new GridPosition(4, 3), MoveDirection.Down, new GridPosition(4, 2), 1);
        }

        private static TestCaseData Case(
            GridPosition start,
            MoveDirection direction,
            GridPosition expectedPlayer,
            int expectedTick)
        {
            return new TestCaseData(start, direction, expectedPlayer, expectedTick)
                .SetName($"Start_{start.X}_{start.Y}_Step_{direction}");
        }

        private static GridPuzzleEngineSession CreateF1Session()
        {
            return new GridPuzzleEngineSession(F1StageDefinitionBuilder.CreateValidatedStage());
        }

        private sealed class MutableWidthStage : IStage
        {
            private readonly IStage _inner;
            private int _width;
            private int _height;

            public MutableWidthStage(IStage inner, int width)
            {
                _inner = inner;
                _width = width;
                _height = inner.Height;
            }

            public int Width => _width;
            public int Height => _height;
            public GridPosition PlayerPosition => _inner.PlayerPosition;
            public GridPosition SpecterPosition => _inner.SpecterPosition;
            public GridPosition ExitPosition => _inner.ExitPosition;
            public IReadOnlyList<GridPosition> Walls => _inner.Walls;
            public IReadOnlyList<StageSwitch> Switches => _inner.Switches;
            public IReadOnlyList<StageDoor> Doors => _inner.Doors;

            public void SetWidth(int width)
            {
                _width = width;
            }

            public void SetHeight(int height)
            {
                _height = height;
            }
        }
    }
}
