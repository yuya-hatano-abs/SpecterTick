using System.Collections.Generic;
using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorCoordinateBoundaryTests
    {
        private const int W = 4;
        private const int H = 4;

        public enum BoundaryElementKind
        {
            Player,
            Specter,
            Exit,
            Wall,
            Switch,
            Door
        }

        public enum BoundaryCoordinateKind
        {
            Origin,
            MaxCorner,
            XBelow,
            YBelow,
            XAtWidth,
            YAtHeight
        }

        private static IEnumerable<TestCaseData> BoundaryCases()
        {
            foreach (BoundaryElementKind element in System.Enum.GetValues(typeof(BoundaryElementKind)))
            {
                yield return new TestCaseData(element, BoundaryCoordinateKind.Origin, true)
                    .SetName($"{element}_AtOrigin_0_0_IsValid");
                yield return new TestCaseData(element, BoundaryCoordinateKind.MaxCorner, true)
                    .SetName($"{element}_AtMaxCorner_Wm1_Hm1_IsValid");
                yield return new TestCaseData(element, BoundaryCoordinateKind.XBelow, false)
                    .SetName($"{element}_AtXMinus1_IsOutOfRange");
                yield return new TestCaseData(element, BoundaryCoordinateKind.YBelow, false)
                    .SetName($"{element}_AtYMinus1_IsOutOfRange");
                yield return new TestCaseData(element, BoundaryCoordinateKind.XAtWidth, false)
                    .SetName($"{element}_AtXEqualsWidth_IsOutOfRange");
                yield return new TestCaseData(element, BoundaryCoordinateKind.YAtHeight, false)
                    .SetName($"{element}_AtYEqualsHeight_IsOutOfRange");
            }
        }

        [TestCaseSource(nameof(BoundaryCases))]
        public void CoordinateBoundary_ForElement(
            BoundaryElementKind element,
            BoundaryCoordinateKind coordinateKind,
            bool shouldSucceed)
        {
            var position = ResolvePosition(coordinateKind);
            var definition = CreateBaselineDefinition(element, position);
            var result = StageValidator.Validate(definition);

            if (shouldSucceed)
            {
                Assert.That(result.IsSuccess, Is.True, $"Expected success for {element} at {position}");
                return;
            }

            Assert.That(result.IsSuccess, Is.False);
            var expectedCode = GetOutOfRangeCode(element);
            Assert.That(result.Errors.Select(e => e.Code), Contains.Item(expectedCode));
        }

        private static GridPosition ResolvePosition(BoundaryCoordinateKind kind)
        {
            switch (kind)
            {
                case BoundaryCoordinateKind.Origin:
                    return new GridPosition(0, 0);
                case BoundaryCoordinateKind.MaxCorner:
                    return new GridPosition(W - 1, H - 1);
                case BoundaryCoordinateKind.XBelow:
                    return new GridPosition(-1, 0);
                case BoundaryCoordinateKind.YBelow:
                    return new GridPosition(0, -1);
                case BoundaryCoordinateKind.XAtWidth:
                    return new GridPosition(W, 0);
                case BoundaryCoordinateKind.YAtHeight:
                    return new GridPosition(0, H);
                default:
                    return new GridPosition(0, 0);
            }
        }

        private static StageValidationErrorCode GetOutOfRangeCode(BoundaryElementKind element)
        {
            switch (element)
            {
                case BoundaryElementKind.Player:
                    return StageValidationErrorCode.PlayerPositionOutOfRange;
                case BoundaryElementKind.Specter:
                    return StageValidationErrorCode.SpecterPositionOutOfRange;
                case BoundaryElementKind.Exit:
                    return StageValidationErrorCode.ExitPositionOutOfRange;
                case BoundaryElementKind.Wall:
                    return StageValidationErrorCode.WallPositionOutOfRange;
                case BoundaryElementKind.Switch:
                    return StageValidationErrorCode.SwitchPositionOutOfRange;
                case BoundaryElementKind.Door:
                    return StageValidationErrorCode.DoorPositionOutOfRange;
                default:
                    return StageValidationErrorCode.PlayerPositionOutOfRange;
            }
        }

        private static StageDefinition CreateBaselineDefinition(BoundaryElementKind element, GridPosition target)
        {
            var definition = new StageDefinition
            {
                Width = W,
                Height = H,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(2, 0),
                Walls = new List<GridPosition>(),
                Switches = new List<SwitchPlacement>(),
                Doors = new List<DoorPlacement>()
            };

            definition.Switches.Add(new SwitchPlacement(new SwitchId("s_ref"), new GridPosition(3, 1)));

            switch (element)
            {
                case BoundaryElementKind.Player:
                    definition.PlayerPosition = target;
                    break;
                case BoundaryElementKind.Specter:
                    definition.SpecterPosition = target;
                    break;
                case BoundaryElementKind.Exit:
                    definition.ExitPosition = target;
                    break;
                case BoundaryElementKind.Wall:
                    definition.Walls.Add(target);
                    break;
                case BoundaryElementKind.Switch:
                    definition.Switches.Add(new SwitchPlacement(new SwitchId("s_test"), target));
                    break;
                case BoundaryElementKind.Door:
                    definition.Doors.Add(new DoorPlacement(
                        new DoorId("d_test"),
                        target,
                        new List<SwitchId> { new SwitchId("s_ref") }));
                    break;
            }

            if (IsInRange(target))
            {
                SeparateSingletons(definition, element, target);
            }

            return definition;
        }

        private static bool IsInRange(GridPosition position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < W && position.Y < H;
        }

        private static void SeparateSingletons(StageDefinition definition, BoundaryElementKind element, GridPosition target)
        {
            var reserved = new HashSet<GridPosition> { target };
            if (element != BoundaryElementKind.Player)
            {
                definition.PlayerPosition = AllocatePosition(reserved);
            }
            else
            {
                reserved.Add(definition.PlayerPosition.Value);
            }

            if (element != BoundaryElementKind.Specter)
            {
                definition.SpecterPosition = AllocatePosition(reserved);
            }
            else
            {
                reserved.Add(definition.SpecterPosition.Value);
            }

            if (element != BoundaryElementKind.Exit)
            {
                definition.ExitPosition = AllocatePosition(reserved);
            }
        }

        private static GridPosition AllocatePosition(HashSet<GridPosition> reserved)
        {
            var candidates = new[]
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(2, 0),
                new GridPosition(3, 0),
                new GridPosition(0, 1),
                new GridPosition(1, 1),
                new GridPosition(2, 1),
                new GridPosition(3, 1),
                new GridPosition(0, 2),
                new GridPosition(1, 2),
                new GridPosition(2, 2),
                new GridPosition(3, 2),
                new GridPosition(0, 3),
                new GridPosition(1, 3),
                new GridPosition(2, 3),
                new GridPosition(3, 3)
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                if (!reserved.Contains(candidates[i]))
                {
                    reserved.Add(candidates[i]);
                    return candidates[i];
                }
            }

            return new GridPosition(3, 3);
        }
    }
}
