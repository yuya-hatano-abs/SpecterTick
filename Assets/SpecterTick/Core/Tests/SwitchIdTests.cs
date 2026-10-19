using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class SwitchIdTests
    {
        [Test]
        public void Default_IsInvalid()
        {
            Assert.That(default(SwitchId).IsValid, Is.False);
        }

        [Test]
        public void NullConstructor_IsInvalid_AndMatchesDefault()
        {
            var fromNull = new SwitchId(null);
            Assert.That(fromNull.IsValid, Is.False);
            Assert.That(fromNull, Is.EqualTo(default(SwitchId)));
        }

        [Test]
        public void EmptyAndWhitespace_AreInvalid()
        {
            Assert.That(new SwitchId(string.Empty).IsValid, Is.False);
            Assert.That(new SwitchId("   ").IsValid, Is.False);
        }

        [Test]
        public void OrdinalEquality_IsCaseAndWhitespaceSensitive()
        {
            Assert.That(new SwitchId("Abc"), Is.Not.EqualTo(new SwitchId("abc")));
            Assert.That(new SwitchId(" a"), Is.Not.EqualTo(new SwitchId("a")));
            Assert.That(new SwitchId("a"), Is.EqualTo(new SwitchId("a")));
            Assert.That(new SwitchId("a").GetHashCode(), Is.EqualTo(new SwitchId("a").GetHashCode()));
        }

        [Test]
        public void Constructor_DoesNotThrow_ForInvalidValues()
        {
            Assert.DoesNotThrow(() => new SwitchId(null));
            Assert.DoesNotThrow(() => new SwitchId(" "));
        }
    }
}
