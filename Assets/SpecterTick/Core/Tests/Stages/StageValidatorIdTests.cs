using System.Collections.Generic;
using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorIdTests
    {
        [Test]
        public void InvalidSwitchIds_DoNotProduceDuplicateSwitchId()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Switches.Add(new SwitchPlacement(default, new GridPosition(1, 1)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId(null), new GridPosition(2, 2)));

            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.InvalidSwitchId));
            Assert.That(codes.Contains(StageValidationErrorCode.DuplicateSwitchId), Is.False);
        }

        [Test]
        public void DuplicateValidSwitchId_ReportsOnce()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Switches.Add(new SwitchPlacement(new SwitchId("x"), new GridPosition(1, 1)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("x"), new GridPosition(2, 2)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("x"), new GridPosition(3, 3)));

            var duplicates = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.DuplicateSwitchId)
                .ToList();
            Assert.That(duplicates.Count, Is.EqualTo(1));
        }

        [Test]
        public void InvalidDoorSwitchReference_IsDistinctFromInvalidSwitchId()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), new GridPosition(2, 2)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                new GridPosition(3, 3),
                new List<SwitchId> { new SwitchId("   ") }));

            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.InvalidDoorSwitchReference));
            Assert.That(codes.Contains(StageValidationErrorCode.InvalidSwitchId), Is.False);
        }

        [Test]
        public void MissingSwitchReference_ReportsNotFound()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                new GridPosition(2, 2),
                new List<SwitchId> { new SwitchId("missing") }));

            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.DoorSwitchReferenceNotFound));
        }
    }
}
