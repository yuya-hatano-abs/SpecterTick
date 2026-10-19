using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorDimensionTests
    {
        [Test]
        public void InvalidWidth_AndHeight_UseStageSubjectKind()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Width = 0;
            definition.Height = -1;
            var errors = StageValidator.Validate(definition).Errors;

            var width = errors.First(e => e.Code == StageValidationErrorCode.WidthInvalid);
            var height = errors.First(e => e.Code == StageValidationErrorCode.HeightInvalid);
            Assert.That(width.Subject.Kind, Is.EqualTo(StageElementKind.Stage));
            Assert.That(height.Subject.Kind, Is.EqualTo(StageElementKind.Stage));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void InvalidWidth_Fails(int width)
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Width = width;
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.WidthInvalid));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void InvalidHeight_Fails(int height)
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Height = height;
            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.HeightInvalid));
        }

        [Test]
        public void InvalidDimensions_DoNotProduceOutOfRangeErrors()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Width = 0;
            definition.Height = -1;
            definition.PlayerPosition = new GridPosition(99, 99);
            definition.Walls.Add(new GridPosition(50, 50));

            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes.Contains(StageValidationErrorCode.PlayerPositionOutOfRange), Is.False);
            Assert.That(codes.Contains(StageValidationErrorCode.WallPositionOutOfRange), Is.False);
        }

        [TestCase(1, 3)]
        [TestCase(3, 1)]
        public void ValidDimensionOfOne_Succeeds(int width, int height)
        {
            var definition = new StageDefinition
            {
                Width = width,
                Height = height,
                Walls = new System.Collections.Generic.List<GridPosition>(),
                Switches = new System.Collections.Generic.List<SwitchPlacement>(),
                Doors = new System.Collections.Generic.List<DoorPlacement>()
            };

            if (width == 1 && height == 3)
            {
                definition.PlayerPosition = new GridPosition(0, 0);
                definition.SpecterPosition = new GridPosition(0, 1);
                definition.ExitPosition = new GridPosition(0, 2);
            }
            else
            {
                definition.PlayerPosition = new GridPosition(0, 0);
                definition.SpecterPosition = new GridPosition(1, 0);
                definition.ExitPosition = new GridPosition(2, 0);
            }

            Assert.That(StageValidator.Validate(definition).IsSuccess, Is.True);
        }

        [Test]
        public void InvalidDimensions_StillCollectsIndependentErrors()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Width = 0;
            definition.Height = 0;
            definition.PlayerPosition = null;
            definition.Switches.Add(new SwitchPlacement(new SwitchId("a"), new GridPosition(0, 0)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("a"), new GridPosition(1, 1)));

            var codes = StageValidator.Validate(definition).Errors.Select(e => e.Code).ToList();
            Assert.That(codes, Contains.Item(StageValidationErrorCode.WidthInvalid));
            Assert.That(codes, Contains.Item(StageValidationErrorCode.HeightInvalid));
            Assert.That(codes, Contains.Item(StageValidationErrorCode.PlayerPositionNotSet));
            Assert.That(codes, Contains.Item(StageValidationErrorCode.DuplicateSwitchId));
        }
    }
}
