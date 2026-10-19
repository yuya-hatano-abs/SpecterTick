using System;
using System.Reflection;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class GridPositionTests
    {
        [Test]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            var a = new GridPosition(2, 3);
            var b = new GridPosition(2, 3);

            Assert.That(a.Equals(b), Is.True);
        }

        [Test]
        public void Equals_DifferentCoordinates_ReturnsFalse()
        {
            var a = new GridPosition(2, 3);
            var b = new GridPosition(2, 4);

            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void EqualityOperator_MatchesValueEquality()
        {
            var a = new GridPosition(1, -1);
            var b = new GridPosition(1, -1);
            var c = new GridPosition(0, -1);

            Assert.That(a == b, Is.True);
            Assert.That(a != c, Is.True);
        }

        [Test]
        public void ObjectEquals_MatchesEqualsAndOperators()
        {
            var a = new GridPosition(5, 7);
            var b = new GridPosition(5, 7);

            Assert.That(a.Equals((object)b), Is.True);
            Assert.That(a == b, Is.True);
        }

        [Test]
        public void GetHashCode_EqualInstances_Match()
        {
            var a = new GridPosition(10, 20);
            var b = new GridPosition(10, 20);

            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Addition_AddsComponents()
        {
            var result = new GridPosition(2, 3) + new GridPosition(1, 4);

            Assert.That(result, Is.EqualTo(new GridPosition(3, 7)));
        }

        [Test]
        public void Subtraction_SubtractsComponents()
        {
            var result = new GridPosition(5, 7) - new GridPosition(2, 3);

            Assert.That(result, Is.EqualTo(new GridPosition(3, 4)));
        }

        [Test]
        public void Arithmetic_WorksWithNegativeCoordinates()
        {
            var sum = new GridPosition(-2, -3) + new GridPosition(1, -1);
            var diff = new GridPosition(-2, -3) - new GridPosition(-1, 2);

            Assert.That(sum, Is.EqualTo(new GridPosition(-1, -4)));
            Assert.That(diff, Is.EqualTo(new GridPosition(-1, -5)));
        }

        [Test]
        public void Arithmetic_AtIntBoundaries_Succeeds()
        {
            var fromMax = new GridPosition(int.MaxValue, int.MaxValue) - new GridPosition(1, 1);
            var fromMin = new GridPosition(int.MinValue, int.MinValue) + new GridPosition(1, 1);

            Assert.That(fromMax, Is.EqualTo(new GridPosition(int.MaxValue - 1, int.MaxValue - 1)));
            Assert.That(fromMin, Is.EqualTo(new GridPosition(int.MinValue + 1, int.MinValue + 1)));
        }

        [Test]
        public void Addition_Overflow_ThrowsOverflowException()
        {
            Assert.Throws<OverflowException>(() =>
            {
                _ = new GridPosition(int.MaxValue, 0) + new GridPosition(1, 0);
            });
            Assert.Throws<OverflowException>(() =>
            {
                _ = new GridPosition(0, int.MaxValue) + new GridPosition(0, 1);
            });
        }

        [Test]
        public void Subtraction_Overflow_ThrowsOverflowException()
        {
            Assert.Throws<OverflowException>(() =>
            {
                _ = new GridPosition(int.MinValue, 0) - new GridPosition(1, 0);
            });
            Assert.Throws<OverflowException>(() =>
            {
                _ = new GridPosition(0, int.MinValue) - new GridPosition(0, 1);
            });
        }

        [Test]
        public void Coordinates_AreNotMutableFromOutside()
        {
            var xProperty = typeof(GridPosition).GetProperty(nameof(GridPosition.X), BindingFlags.Public | BindingFlags.Instance);
            var yProperty = typeof(GridPosition).GetProperty(nameof(GridPosition.Y), BindingFlags.Public | BindingFlags.Instance);

            Assert.That(xProperty, Is.Not.Null);
            Assert.That(yProperty, Is.Not.Null);
            Assert.That(xProperty!.CanWrite, Is.False);
            Assert.That(yProperty!.CanWrite, Is.False);
        }
    }
}
