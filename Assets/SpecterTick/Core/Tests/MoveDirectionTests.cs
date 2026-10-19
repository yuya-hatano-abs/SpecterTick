using System;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class MoveDirectionTests
    {
        [Test]
        public void ToOffset_Up_IsZeroOne()
        {
            Assert.That(MoveDirectionUtility.ToOffset(MoveDirection.Up), Is.EqualTo(new GridPosition(0, 1)));
        }

        [Test]
        public void ToOffset_Down_IsZeroMinusOne()
        {
            Assert.That(MoveDirectionUtility.ToOffset(MoveDirection.Down), Is.EqualTo(new GridPosition(0, -1)));
        }

        [Test]
        public void ToOffset_Left_IsMinusOneZero()
        {
            Assert.That(MoveDirectionUtility.ToOffset(MoveDirection.Left), Is.EqualTo(new GridPosition(-1, 0)));
        }

        [Test]
        public void ToOffset_Right_IsOneZero()
        {
            Assert.That(MoveDirectionUtility.ToOffset(MoveDirection.Right), Is.EqualTo(new GridPosition(1, 0)));
        }

        [Test]
        public void ToOffset_UndefinedValue_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                MoveDirectionUtility.ToOffset((MoveDirection)99));

            Assert.That(ex!.ParamName, Is.EqualTo("direction"));
        }

        [Test]
        public void ToOffset_DoesNotDependOnDeclarationOrder()
        {
            Assert.That((int)MoveDirection.Up, Is.Not.EqualTo(0));
            Assert.That((int)MoveDirection.Down, Is.Not.EqualTo(0));
            Assert.That(MoveDirectionUtility.ToOffset(MoveDirection.Right), Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(MoveDirectionUtility.ToOffset(MoveDirection.Left), Is.EqualTo(new GridPosition(-1, 0)));
        }

        [Test]
        public void DefaultDirection_IsRejected()
        {
            var defaultDirection = default(MoveDirection);

            Assert.That(MoveDirectionUtility.IsDefined(defaultDirection), Is.False);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                MoveDirectionUtility.ToOffset(defaultDirection));

            Assert.That(ex!.ParamName, Is.EqualTo("direction"));
        }
    }
}
