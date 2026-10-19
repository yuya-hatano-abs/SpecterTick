using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class DoorIdTests
    {
        [Test]
        public void Default_IsInvalid()
        {
            Assert.That(default(DoorId).IsValid, Is.False);
        }

        [Test]
        public void NullConstructor_MatchesDefault()
        {
            Assert.That(new DoorId(null), Is.EqualTo(default(DoorId)));
        }

        [Test]
        public void EmptyAndWhitespace_AreInvalid()
        {
            Assert.That(new DoorId(string.Empty).IsValid, Is.False);
            Assert.That(new DoorId("   ").IsValid, Is.False);
        }

        [Test]
        public void OrdinalEquality_IsCaseAndWhitespaceSensitive()
        {
            Assert.That(new DoorId("Abc"), Is.Not.EqualTo(new DoorId("abc")));
            Assert.That(new DoorId(" a"), Is.Not.EqualTo(new DoorId("a")));
            Assert.That(new DoorId("a"), Is.EqualTo(new DoorId("a")));
            Assert.That(new DoorId("a").GetHashCode(), Is.EqualTo(new DoorId("a").GetHashCode()));
        }
    }
}
