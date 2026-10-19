using System.Collections.Generic;
using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorPlacementTests
    {
        private static readonly GridPosition Conflict = new GridPosition(2, 2);
        private static readonly GridPosition Away = new GridPosition(0, 0);

        [Test]
        public void ThreeWallsAtSamePosition_ProducesOneSameKindError()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Walls.Add(Conflict);
            definition.Walls.Add(Conflict);
            definition.Walls.Add(Conflict);

            var errors = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.SameKindPositionOverlap)
                .ToList();
            Assert.That(errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void WallSwitchDoorAtSamePosition_ProducesThreeCrossKindErrors()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.PlayerPosition = Away;
            definition.SpecterPosition = new GridPosition(1, 0);
            definition.ExitPosition = new GridPosition(3, 3);
            definition.Walls.Add(Conflict);
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), Conflict));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                Conflict,
                new List<SwitchId> { new SwitchId("s1") }));

            var errors = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.CrossKindPositionConflict)
                .ToList();
            Assert.That(errors.Count, Is.EqualTo(3));
        }

        [Test]
        public void OutOfRangeAndOverlap_BothReported()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(3, 3);
            definition.Walls.Add(new GridPosition(9, 9));
            definition.Walls.Add(new GridPosition(9, 9));

            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.WallPositionOutOfRange));
            Assert.That(codes, Contains.Item(StageValidationErrorCode.SameKindPositionOverlap));
        }

        [Test]
        public void SameKindDoorOverlap_ProducesOneSameKindError()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(5, 5);
            var overlap = new GridPosition(2, 2);
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), new GridPosition(1, 1)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                overlap,
                new List<SwitchId> { new SwitchId("s1") }));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d2"),
                overlap,
                new List<SwitchId> { new SwitchId("s1") }));

            var errors = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.SameKindPositionOverlap
                            && e.Subject.Kind == StageElementKind.Door)
                .ToList();
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0].Subject.Position, Is.EqualTo(overlap));
        }

        [Test]
        public void SameKindSwitchOverlap_UsesSameKindCode()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Switches.Add(new SwitchPlacement(new SwitchId("a"), Conflict));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("b"), Conflict));

            var errors = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.SameKindPositionOverlap)
                .ToList();
            Assert.That(errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateErrorKey_IsMerged()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.PlayerPosition = Conflict;
            definition.SpecterPosition = Conflict;

            var errors = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.CrossKindPositionConflict
                            && e.Subject.Kind == StageElementKind.Player
                            && e.Subject.OtherKind == StageElementKind.Specter)
                .ToList();
            Assert.That(errors.Count, Is.EqualTo(1));
        }

        private static StageDefinition CreateIsolatedConflict(StageElementKind a, StageElementKind b)
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(5, 5);
            definition.PlayerPosition = Away;
            definition.SpecterPosition = new GridPosition(1, 0);
            definition.ExitPosition = new GridPosition(3, 3);

            Place(definition, a, Conflict);
            Place(definition, b, Conflict);
            return definition;
        }

        private static void Place(StageDefinition definition, StageElementKind kind, GridPosition position)
        {
            switch (kind)
            {
                case StageElementKind.Player:
                    definition.PlayerPosition = position;
                    break;
                case StageElementKind.Specter:
                    definition.SpecterPosition = position;
                    break;
                case StageElementKind.Exit:
                    definition.ExitPosition = position;
                    break;
                case StageElementKind.Wall:
                    definition.Walls.Add(position);
                    break;
                case StageElementKind.Switch:
                    definition.Switches.Add(new SwitchPlacement(new SwitchId("s_" + definition.Switches.Count), position));
                    break;
                case StageElementKind.Door:
                    SwitchId doorSwitchId;
                    if (definition.Switches.Count == 0)
                    {
                        doorSwitchId = new SwitchId("s_door");
                        definition.Switches.Add(new SwitchPlacement(doorSwitchId, new GridPosition(4, 4)));
                    }
                    else
                    {
                        doorSwitchId = definition.Switches[definition.Switches.Count - 1].Id;
                    }

                    definition.Doors.Add(new DoorPlacement(
                        new DoorId("d_" + definition.Doors.Count),
                        position,
                        new List<SwitchId> { doorSwitchId }));
                    break;
            }
        }
    }
}
