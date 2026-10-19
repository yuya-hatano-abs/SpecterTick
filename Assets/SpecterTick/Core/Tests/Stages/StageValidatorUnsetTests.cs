using System.Collections.Generic;
using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorUnsetTests
    {
        [Test]
        public void UnsetSpecter_ReportsDedicatedError()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.SpecterPosition = null;
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.SpecterPositionNotSet));
        }

        [Test]
        public void UnsetExit_ReportsDedicatedError()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.ExitPosition = null;
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.ExitPositionNotSet));
        }

        [Test]
        public void NullSwitchesCollection_ReportsDedicatedError()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Switches = null;
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.SwitchesCollectionNull));
        }

        [Test]
        public void NullDoorsCollection_ReportsDedicatedError()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Doors = null;
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.DoorsCollectionNull));
        }

        [Test]
        public void UnsetPlayer_IsNotTreatedAsOrigin()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.PlayerPosition = null;
            definition.SpecterPosition = new GridPosition(0, 0);

            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.PlayerPositionNotSet));
            Assert.That(codes.Contains(StageValidationErrorCode.PlayerPositionOutOfRange), Is.False);
        }

        [Test]
        public void Origin_IsValid_WhenExplicitlySet()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.PlayerPosition = new GridPosition(0, 0);
            definition.SpecterPosition = new GridPosition(1, 0);
            definition.ExitPosition = new GridPosition(0, 1);

            Assert.That(StageValidator.Validate(definition).IsSuccess, Is.True);
        }

        [Test]
        public void NullWallsCollection_ReportsDedicatedError()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Walls = null;
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.WallsCollectionNull));
            Assert.That(StageValidator.Validate(definition).IsSuccess, Is.False);
        }

        [Test]
        public void DoorSwitchReferences_NullAndEmpty_UseSameErrorCode()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Doors.Add(new DoorPlacement(new DoorId("d1"), new GridPosition(2, 2), null));
            var nullCodes = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.DoorSwitchReferencesEmpty)
                .ToList();
            Assert.That(nullCodes.Count, Is.EqualTo(1));

            definition.Doors[0] = new DoorPlacement(
                new DoorId("d1"),
                new GridPosition(2, 2),
                new List<SwitchId>());
            var emptyCodes = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.DoorSwitchReferencesEmpty)
                .ToList();
            Assert.That(emptyCodes.Count, Is.EqualTo(1));
        }
    }
}
