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
            var ids = new[] { Sa, Sb, Sc };
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
