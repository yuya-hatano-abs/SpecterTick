using System;
using System.Reflection;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class RecordedCommandTests
    {
        [Test]
        public void Constructor_ValidDirection_StoresDirection()
        {
            var command = new RecordedCommand(MoveDirection.Up);

            Assert.That(command.Direction, Is.EqualTo(MoveDirection.Up));
        }

        [Test]
        public void Direction_IsNotMutableFromOutside()
        {
            var property = typeof(RecordedCommand).GetProperty(
                nameof(RecordedCommand.Direction),
                BindingFlags.Public | BindingFlags.Instance);

            Assert.That(property, Is.Not.Null);
            Assert.That(property!.CanWrite, Is.False);
        }

        [Test]
        public void Constructor_InvalidDirection_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new RecordedCommand((MoveDirection)0));

            Assert.That(ex!.ParamName, Is.EqualTo("direction"));
        }

        [Test]
        public void Equality_SameDirection_IsEqual()
        {
            var a = new RecordedCommand(MoveDirection.Left);
            var b = new RecordedCommand(MoveDirection.Left);

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a == b, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equality_DifferentDirection_IsNotEqual()
        {
            var a = new RecordedCommand(MoveDirection.Left);
            var b = new RecordedCommand(MoveDirection.Right);

            Assert.That(a, Is.Not.EqualTo(b));
            Assert.That(a != b, Is.True);
        }
    }
}
