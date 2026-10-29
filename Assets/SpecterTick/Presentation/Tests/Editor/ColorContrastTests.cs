using System;
using NUnit.Framework;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class ColorContrastTests
    {
        private const float Tolerance = 1e-5f;

        [Test]
        public void CC1_RelativeLuminance_Primaries()
        {
            Assert.That(ColorContrast.RelativeLuminance(0f, 0f, 0f), Is.EqualTo(0f).Within(Tolerance));
            Assert.That(ColorContrast.RelativeLuminance(1f, 1f, 1f), Is.EqualTo(1f).Within(Tolerance));
            Assert.That(ColorContrast.RelativeLuminance(1f, 0f, 0f), Is.EqualTo(0.2126f).Within(Tolerance));
            Assert.That(ColorContrast.RelativeLuminance(0f, 1f, 0f), Is.EqualTo(0.7152f).Within(Tolerance));
            Assert.That(ColorContrast.RelativeLuminance(0f, 0f, 1f), Is.EqualTo(0.0722f).Within(Tolerance));
        }

        [Test]
        public void CC2_RelativeLuminance_Gamma()
        {
            Assert.That(ColorContrast.RelativeLuminance(0.5f, 0.5f, 0.5f), Is.EqualTo(0.2140411f).Within(Tolerance));
            Assert.That(ColorContrast.RelativeLuminance(0.03f, 0.03f, 0.03f), Is.EqualTo(0.0023220f).Within(Tolerance));
        }

        [Test]
        public void CC3_ContrastRatio_Values()
        {
            Assert.That(ColorContrast.ContrastRatio(1f, 0f), Is.EqualTo(21f).Within(Tolerance));
            Assert.That(ColorContrast.ContrastRatio(0f, 1f), Is.EqualTo(21f).Within(Tolerance));
            Assert.That(ColorContrast.ContrastRatio(0.5f, 0.5f), Is.EqualTo(1f).Within(Tolerance));
            Assert.That(ColorContrast.ContrastRatio(0.2f, 0.1f), Is.EqualTo(1.666667f).Within(Tolerance));
            Assert.That(ColorContrast.ContrastRatio(0f, 0f), Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void CC4_Blend_Values()
        {
            Assert.That(ColorContrast.Blend(1f, 0.5f, 0f), Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(ColorContrast.Blend(0.2f, 0f, 0.7f), Is.EqualTo(0.7f).Within(Tolerance));
            Assert.That(ColorContrast.Blend(0.2f, 1f, 0.7f), Is.EqualTo(0.2f).Within(Tolerance));
            Assert.That(ColorContrast.Blend(0f, 0.6f, 1f), Is.EqualTo(0.4f).Within(Tolerance));
        }

        [Test]
        public void CC5_InvalidArguments()
        {
            var rException = Assert.Throws<ArgumentOutOfRangeException>(() => ColorContrast.RelativeLuminance(-0.1f, 0f, 0f));
            Assert.That(rException.ParamName, Is.EqualTo("r"));

            var gException = Assert.Throws<ArgumentOutOfRangeException>(() => ColorContrast.RelativeLuminance(0f, 1.1f, 0f));
            Assert.That(gException.ParamName, Is.EqualTo("g"));

            var bException = Assert.Throws<ArgumentOutOfRangeException>(() => ColorContrast.RelativeLuminance(0f, 0f, -0.1f));
            Assert.That(bException.ParamName, Is.EqualTo("b"));

            var orderException = Assert.Throws<ArgumentOutOfRangeException>(() => ColorContrast.RelativeLuminance(-0.1f, 1.1f, 0f));
            Assert.That(orderException.ParamName, Is.EqualTo("r"));

            var luminanceAException = Assert.Throws<ArgumentOutOfRangeException>(() => ColorContrast.ContrastRatio(-0.1f, 0f));
            Assert.That(luminanceAException.ParamName, Is.EqualTo("luminanceA"));

            var luminanceBException = Assert.Throws<ArgumentOutOfRangeException>(() => ColorContrast.ContrastRatio(0f, 1.1f));
            Assert.That(luminanceBException.ParamName, Is.EqualTo("luminanceB"));

            var alphaLowException = Assert.Throws<ArgumentOutOfRangeException>(() => ColorContrast.Blend(0f, -0.1f, 0f));
            Assert.That(alphaLowException.ParamName, Is.EqualTo("alpha"));

            var alphaHighException = Assert.Throws<ArgumentOutOfRangeException>(() => ColorContrast.Blend(0f, 1.1f, 0f));
            Assert.That(alphaHighException.ParamName, Is.EqualTo("alpha"));

            Assert.DoesNotThrow(() => ColorContrast.RelativeLuminance(0f, 0f, 0f));
            Assert.DoesNotThrow(() => ColorContrast.RelativeLuminance(1f, 1f, 1f));
            Assert.DoesNotThrow(() => ColorContrast.ContrastRatio(0f, 1f));
            Assert.DoesNotThrow(() => ColorContrast.Blend(0f, 0f, 0f));
            Assert.DoesNotThrow(() => ColorContrast.Blend(0f, 1f, 0f));
        }
    }
}
