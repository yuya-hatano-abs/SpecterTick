using System.Collections.Generic;
using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorCompositeTests
    {
        [Test]
        public void CaseA_CollectsExpectedCodes_WithoutOutOfRange()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Width = 0;
            definition.Height = -1;
            definition.PlayerPosition = new GridPosition(9, 9);
            definition.Walls.Add(new GridPosition(8, 8));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("dup"), new GridPosition(1, 1)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("dup"), new GridPosition(2, 2)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                new GridPosition(3, 2),
                new List<SwitchId> { new SwitchId("missing") }));

            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            var expected = new[]
            {
                StageValidationErrorCode.WidthInvalid,
                StageValidationErrorCode.HeightInvalid,
                StageValidationErrorCode.DuplicateSwitchId,
                StageValidationErrorCode.DoorSwitchReferenceNotFound
            };

            Assert.That(codes, Is.EqualTo(expected));
            Assert.That(codes.Count, Is.EqualTo(4));
        }

        [Test]
        public void CaseB_CollectsEachExpectedIssue()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(6, 6);
            var overlap = new GridPosition(2, 2);
            definition.PlayerPosition = overlap;
            definition.SpecterPosition = overlap;
            definition.Walls.Add(overlap);
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), overlap));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s2"), overlap));
            definition.Doors.Add(new DoorPlacement(new DoorId("d1"), new GridPosition(4, 4), new List<SwitchId> { new SwitchId("s1") }));
            definition.Doors.Add(new DoorPlacement(new DoorId("d1"), new GridPosition(5, 5), new List<SwitchId> { new SwitchId("s2") }));

            var errors = StageValidator.Validate(definition).Errors;
            var codes = errors.Select(e => e.Code).ToList();
            var expectedCodes = new List<StageValidationErrorCode>
            {
                StageValidationErrorCode.DuplicateDoorId,
                StageValidationErrorCode.SameKindPositionOverlap,
                StageValidationErrorCode.CrossKindPositionConflict,
                StageValidationErrorCode.CrossKindPositionConflict,
                StageValidationErrorCode.CrossKindPositionConflict,
                StageValidationErrorCode.CrossKindPositionConflict,
                StageValidationErrorCode.CrossKindPositionConflict,
                StageValidationErrorCode.CrossKindPositionConflict
            };

            Assert.That(codes, Is.EqualTo(expectedCodes));
            Assert.That(codes.Count, Is.EqualTo(8));

            var cross = errors.Where(e => e.Code == StageValidationErrorCode.CrossKindPositionConflict).ToList();
            var expectedPairs = new[]
            {
                (StageElementKind.Player, StageElementKind.Specter),
                (StageElementKind.Player, StageElementKind.Wall),
                (StageElementKind.Specter, StageElementKind.Wall),
                (StageElementKind.Player, StageElementKind.Switch),
                (StageElementKind.Specter, StageElementKind.Switch),
                (StageElementKind.Wall, StageElementKind.Switch)
            };

            Assert.That(cross.Count, Is.EqualTo(6));
            for (var i = 0; i < expectedPairs.Length; i++)
            {
                PlacementConflictPairs.NormalizeKinds(
                    expectedPairs[i].Item1,
                    expectedPairs[i].Item2,
                    out var kindA,
                    out var kindB);
                Assert.That(cross[i].Subject.Kind, Is.EqualTo(kindA));
                Assert.That(cross[i].Subject.OtherKind, Is.EqualTo(kindB));
                Assert.That(cross[i].Subject.Position, Is.EqualTo(overlap));
            }
        }

        [Test]
        public void SameInput_ProducesDeterministicErrorOrder()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Width = 0;
            definition.PlayerPosition = null;

            var first = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            var second = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(second, Is.EqualTo(first));
        }
    }
}
