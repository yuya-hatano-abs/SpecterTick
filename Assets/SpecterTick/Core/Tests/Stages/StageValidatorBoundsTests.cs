using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorBoundsTests
    {
        [Test]
        public void PlayerCornerPositions_AreValidOn4x4()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(4, 4);
            definition.PlayerPosition = new GridPosition(0, 0);
            definition.SpecterPosition = new GridPosition(1, 0);
            definition.ExitPosition = new GridPosition(3, 3);
            Assert.That(StageValidator.Validate(definition).IsSuccess, Is.True);

            definition.PlayerPosition = new GridPosition(3, 3);
            definition.ExitPosition = new GridPosition(0, 0);
            Assert.That(StageValidator.Validate(definition).IsSuccess, Is.True);
        }

        [TestCase(-1, 0, StageValidationErrorCode.PlayerPositionOutOfRange)]
        [TestCase(0, -1, StageValidationErrorCode.PlayerPositionOutOfRange)]
        [TestCase(4, 0, StageValidationErrorCode.PlayerPositionOutOfRange)]
        [TestCase(0, 4, StageValidationErrorCode.PlayerPositionOutOfRange)]
        public void PlayerOutOfRange_ReportsDedicatedCode(int x, int y, StageValidationErrorCode expected)
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(4, 4);
            definition.PlayerPosition = new GridPosition(x, y);
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(expected));
        }

        [Test]
        public void WallOutOfRange_ReportsWallCode()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(4, 4);
            definition.Walls.Add(new GridPosition(4, 0));
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.WallPositionOutOfRange));
        }
    }
}
