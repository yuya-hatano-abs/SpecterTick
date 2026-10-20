using System;
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
            var a = new GameState(2, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            var b = new GameState(2, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentTick_IsNotEqual()
        {
            var a = new GameState(1, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            var b = new GameState(2, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void Equals_DifferentPlayerPosition_IsNotEqual()
        {
            var a = new GameState(1, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            var b = new GameState(1, new GridPosition(2, 1), Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void Equals_DifferentSpecterPosition_IsNotEqual()
        {
            var a = new GameState(1, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            var b = new GameState(1, Player, new GridPosition(2, 2), ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void Equals_NullAndWrongObjectType_ReturnFalse()
        {
            var state = new GameState(0, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            Assert.That(state.Equals(null), Is.False);
            Assert.That(state.Equals("not a state"), Is.False);
            Assert.That(state == null, Is.False);
            Assert.That(null == state, Is.False);
        }

        [Test]
        public void Equals_DifferentActivatedSwitchSet_IsNotEqual()
        {
            var empty = ActivatedSwitchSet.Empty;
            var withS1 = new ActivatedSwitchSet(new[] { new SwitchId("S1") });
            var a = new GameState(1, Player, Specter, empty, GameMode.Recording, 0);
            var b = new GameState(1, Player, Specter, withS1, GameMode.Recording, 0);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void Equals_SameCountDifferentActivatedSwitchIds_IsNotEqual()
        {
            var s1 = new ActivatedSwitchSet(new[] { new SwitchId("S1") });
            var s2 = new ActivatedSwitchSet(new[] { new SwitchId("S2") });
            var a = new GameState(1, Player, Specter, s1, GameMode.Recording, 0);
            var b = new GameState(1, Player, Specter, s2, GameMode.Recording, 0);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void Constructor_NullActivatedSwitches_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GameState(0, Player, Specter, null, GameMode.Recording, 0));
            Assert.That(ex.ParamName, Is.EqualTo("activatedSwitches"));
        }

        [Test]
        public void IsSwitchActivated_InvalidId_ThrowsArgumentException()
        {
            var state = new GameState(0, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            var ex = Assert.Throws<ArgumentException>(() => state.IsSwitchActivated(default(SwitchId)));
            Assert.That(ex.ParamName, Is.EqualTo("switchId"));
        }

        [Test]
        public void IsSwitchActivated_ValidIdNotOnStage_ReturnsFalse()
        {
            var state = new GameState(0, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            Assert.That(state.IsSwitchActivated(new SwitchId("missing")), Is.False);
        }

        [Test]
        public void Constructor_UndefinedModeZero_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameState(0, Player, Specter, ActivatedSwitchSet.Empty, (GameMode)0, 0));
            Assert.That(ex.ParamName, Is.EqualTo("mode"));
        }

        [Test]
        public void Constructor_UndefinedModeAboveRange_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameState(0, Player, Specter, ActivatedSwitchSet.Empty, (GameMode)4, 0));
            Assert.That(ex.ParamName, Is.EqualTo("mode"));
        }

        [Test]
        public void Constructor_NegativePlaybackIndex_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new GameState(0, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, -1));
            Assert.That(ex.ParamName, Is.EqualTo("playbackIndex"));
        }

        [Test]
        public void Equals_DifferentMode_IsNotEqual()
        {
            var a = new GameState(1, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Recording, 0);
            var b = new GameState(1, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Playback, 0);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void Equals_DifferentPlaybackIndex_IsNotEqual()
        {
            var a = new GameState(1, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Playback, 0);
            var b = new GameState(1, Player, Specter, ActivatedSwitchSet.Empty, GameMode.Playback, 1);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(a != b, Is.True);
        }
    }
}
