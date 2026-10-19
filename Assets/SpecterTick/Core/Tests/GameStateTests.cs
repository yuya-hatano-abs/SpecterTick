using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class GameStateTests
    {
        private static readonly GridPosition Player = new GridPosition(1, 1);
        private static readonly GridPosition Specter = new GridPosition(3, 3);

        [Test]
        public void Equals_SameValues_AreEqual_HashCodesMatch()
        {
            var a = new GameState(2, Player, Specter);
            var b = new GameState(2, Player, Specter);

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentTick_IsNotEqual()
        {
            var a = new GameState(1, Player, Specter);
            var b = new GameState(2, Player, Specter);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void Equals_DifferentPlayerPosition_IsNotEqual()
        {
            var a = new GameState(1, Player, Specter);
            var b = new GameState(1, new GridPosition(2, 1), Specter);
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void Equals_DifferentSpecterPosition_IsNotEqual()
        {
            var a = new GameState(1, Player, Specter);
            var b = new GameState(1, Player, new GridPosition(2, 2));
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void Equals_NullAndWrongObjectType_ReturnFalse()
        {
            var state = new GameState(0, Player, Specter);
            Assert.That(state.Equals(null), Is.False);
            Assert.That(state.Equals("not a state"), Is.False);
            Assert.That(state == null, Is.False);
            Assert.That(null == state, Is.False);
        }
    }
}
