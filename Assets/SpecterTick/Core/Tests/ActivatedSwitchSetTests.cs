using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SpecterTick.Core.Tests
{
    [TestFixture]
    public sealed class ActivatedSwitchSetTests
    {
        private static readonly SwitchId Sa = new SwitchId("SA");
        private static readonly SwitchId Sb = new SwitchId("SB");
        private static readonly SwitchId Sc = new SwitchId("SC");

        [Test]
        public void Constructor_NullEnumerable_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ActivatedSwitchSet(null));
            Assert.That(ex.ParamName, Is.EqualTo("switchIds"));
        }

        [Test]
        public void Equals_DifferentSingleElementSets_ReturnFalse()
        {
            var a = new ActivatedSwitchSet(new[] { Sa });
            var b = new ActivatedSwitchSet(new[] { Sb });
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void Equals_SameCountDifferentElements_ReturnFalse()
        {
            var ab = new ActivatedSwitchSet(new[] { Sa, Sb });
            var ac = new ActivatedSwitchSet(new[] { Sa, Sc });
            Assert.That(ab.Equals(ac), Is.False);
        }

        [Test]
        public void Constructor_DuplicateInput_DeduplicatesToSingleElement()
        {
            var once = new ActivatedSwitchSet(new[] { Sa });
            var twice = new ActivatedSwitchSet(new[] { Sa, Sa });
            Assert.That(twice.Equals(once), Is.True);
            Assert.That(twice.GetHashCode(), Is.EqualTo(once.GetHashCode()));
            Assert.That(twice.Count, Is.EqualTo(1));
        }

        [Test]
        public void Add_AlreadyContainedElement_ReturnsEqualSet()
        {
            var set = new ActivatedSwitchSet(new[] { Sa });
            var again = set.Add(Sa);
            Assert.That(again.Equals(set), Is.True);
        }

        [Test]
        public void Empty_EqualsNewEmptySet_SameHashCode()
        {
            var empty = ActivatedSwitchSet.Empty;
            var other = new ActivatedSwitchSet(Array.Empty<SwitchId>());
            Assert.That(empty.Equals(other), Is.True);
            Assert.That(empty.GetHashCode(), Is.EqualTo(other.GetHashCode()));
        }

        [Test]
        public void Equals_NullAndWrongObjectType_ReturnFalse()
        {
            var set = new ActivatedSwitchSet(new[] { Sa });
            Assert.That(set.Equals(null), Is.False);
            Assert.That(set.Equals("not a set"), Is.False);
        }

        [Test]
        public void Add_DoesNotMutateOriginalSet()
        {
            var original = new ActivatedSwitchSet(new[] { Sa });
            var added = original.Add(Sb);
            Assert.That(original.Contains(Sa), Is.True);
            Assert.That(original.Contains(Sb), Is.False);
            Assert.That(added.Contains(Sb), Is.True);
        }

        [Test]
        public void ThreeElements_AllPermutations_AreEqualAndSameHashCode()
        {
            var permutations = new List<SwitchId[]>
            {
                new[] { Sa, Sb, Sc },
                new[] { Sa, Sc, Sb },
                new[] { Sb, Sa, Sc },
                new[] { Sb, Sc, Sa },
                new[] { Sc, Sa, Sb },
                new[] { Sc, Sb, Sa }
            };

            var reference = new ActivatedSwitchSet(permutations[0]);
            var referenceHash = reference.GetHashCode();
            for (var i = 0; i < permutations.Count; i++)
            {
                var set = new ActivatedSwitchSet(permutations[i]);
                Assert.That(set.Equals(reference), Is.True, "Permutation index " + i);
                Assert.That(set.GetHashCode(), Is.EqualTo(referenceHash), "Hash permutation index " + i);
            }
        }
    }
}
