using System;
using NUnit.Framework;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class StageLabelModelTests
    {
        [Test]
        public void LM1_Build_FirstStage()
        {
            var model = StageLabelModelBuilder.Build(0, 5, "WALK");
            Assert.That(model.TitleLine, Is.EqualTo("STAGE 1 / 5  WALK"));
            Assert.That(model.KeysLine, Is.EqualTo("PREV: P  NEXT: N"));
            Assert.That(model.ToDisplayText(), Is.EqualTo("STAGE 1 / 5  WALK\nPREV: P  NEXT: N"));
        }

        [Test]
        public void LM2_Build_LastStage()
        {
            var model = StageLabelModelBuilder.Build(4, 5, "TANGLE");
            Assert.That(model.TitleLine, Is.EqualTo("STAGE 5 / 5  TANGLE"));
            Assert.That(model.KeysLine, Is.EqualTo("PREV: P  NEXT: N"));
            Assert.That(model.ToDisplayText(), Is.EqualTo("STAGE 5 / 5  TANGLE\nPREV: P  NEXT: N"));
        }

        [Test]
        public void LM3_InvalidArguments()
        {
            var countEx = Assert.Throws<ArgumentOutOfRangeException>(() => StageLabelModelBuilder.Build(0, 0, "X"));
            Assert.That(countEx.ParamName, Is.EqualTo("count"));

            countEx = Assert.Throws<ArgumentOutOfRangeException>(() => StageLabelModelBuilder.Build(-1, 0, "X"));
            Assert.That(countEx.ParamName, Is.EqualTo("count"));

            var indexEx = Assert.Throws<ArgumentOutOfRangeException>(() => StageLabelModelBuilder.Build(-1, 5, "X"));
            Assert.That(indexEx.ParamName, Is.EqualTo("index"));

            indexEx = Assert.Throws<ArgumentOutOfRangeException>(() => StageLabelModelBuilder.Build(5, 5, "X"));
            Assert.That(indexEx.ParamName, Is.EqualTo("index"));

            var nullEx = Assert.Throws<ArgumentNullException>(() => StageLabelModelBuilder.Build(0, 5, null));
            Assert.That(nullEx.ParamName, Is.EqualTo("name"));

            Assert.That(StageLabelModelBuilder.Build(4, 5, "X").TitleLine, Is.EqualTo("STAGE 5 / 5  X"));
            Assert.That(StageLabelModelBuilder.Build(0, 1, "X").TitleLine, Is.EqualTo("STAGE 1 / 1  X"));
        }
    }
}
