using System;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Tests.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class StageLayoutSnapshotLatchTests
    {
        [Test]
        public void ApplyLatch_NonMatchingAdjacentAndMissingPositions_OnlyExactCellActivates()
        {
            var layout = StageLayoutSnapshot.FromStage(F2StageDefinitionBuilder.CreateValidatedStage());
            var sa = new SwitchId("SA");
            var next = layout.ApplyLatch(ActivatedSwitchSet.Empty, new[]
            {
                new GridPosition(0, 0),
                new GridPosition(0, 2),
                new GridPosition(1, 1)
            });
            Assert.That(next.Contains(sa), Is.False);
        }

        [Test]
        public void ApplyLatch_OnSwitchCell_ActivatesSwitch()
        {
            var layout = StageLayoutSnapshot.FromStage(F2StageDefinitionBuilder.CreateValidatedStage());
            var sa = new SwitchId("SA");
            var next = layout.ApplyLatch(ActivatedSwitchSet.Empty, new[] { new GridPosition(0, 1) });
            Assert.That(next.Contains(sa), Is.True);
        }

        [Test]
        public void ApplyLatch_TwoDistinctSwitches_DuplicatePosition_EmptyInput()
        {
            var layout = StageLayoutSnapshot.FromStage(F2StageDefinitionBuilder.CreateValidatedStage());
            var sa = new SwitchId("SA");
            var sb = new SwitchId("SB");
            var both = layout.ApplyLatch(ActivatedSwitchSet.Empty, new[]
            {
                new GridPosition(0, 1),
                new GridPosition(2, 1)
            });
            Assert.That(both.Contains(sa), Is.True);
            Assert.That(both.Contains(sb), Is.True);

            var duplicate = layout.ApplyLatch(both, new[]
            {
                new GridPosition(0, 1),
                new GridPosition(0, 1)
            });
            Assert.That(duplicate.Equals(both), Is.True);

            var unchanged = layout.ApplyLatch(both, Array.Empty<GridPosition>());
            Assert.That(unchanged.Equals(both), Is.True);
        }

        [Test]
        public void ApplyLatch_PreservesPriorActivations_WhenAddingNewPosition()
        {
            var layout = StageLayoutSnapshot.FromStage(F2StageDefinitionBuilder.CreateValidatedStage());
            var sa = new SwitchId("SA");
            var sb = new SwitchId("SB");
            var withSa = layout.ApplyLatch(ActivatedSwitchSet.Empty, new[] { new GridPosition(0, 1) });
            var withBoth = layout.ApplyLatch(withSa, new[] { new GridPosition(2, 1) });
            Assert.That(withBoth.Contains(sa), Is.True);
            Assert.That(withBoth.Contains(sb), Is.True);
        }

        [Test]
        public void ApplyLatch_NullOccupantPositions_ThrowsArgumentNullException()
        {
            var layout = StageLayoutSnapshot.FromStage(StageValidator.Validate(StageDefinitionTestBuilder.CreateMinimalValid()).Stage);
            var ex = Assert.Throws<ArgumentNullException>(() => layout.ApplyLatch(ActivatedSwitchSet.Empty, null));
            Assert.That(ex.ParamName, Is.EqualTo("occupantPositions"));
        }

        [Test]
        public void ApplyLatch_NullCurrent_ThrowsArgumentNullException()
        {
            var layout = StageLayoutSnapshot.FromStage(StageValidator.Validate(StageDefinitionTestBuilder.CreateMinimalValid()).Stage);
            var ex = Assert.Throws<ArgumentNullException>(() =>
                layout.ApplyLatch(null, new[] { new GridPosition(0, 0) }));
            Assert.That(ex.ParamName, Is.EqualTo("current"));
        }
    }
}
