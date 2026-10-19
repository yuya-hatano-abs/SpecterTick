using System.Collections;
using System.Linq;
using SpecterTick.Core.Stages;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class PlacementConflictPairsImmutabilityTests
    {
        [Test]
        public void CrossKindPairsInOrder_IListMutation_DoesNotChangeContent()
        {
            var originalCount = PlacementConflictPairs.CrossKindPairsInOrder.Count;
            var originalFirst = PlacementConflictPairs.CrossKindPairsInOrder[0];
            var list = PlacementConflictPairs.CrossKindPairsInOrder as IList;
            if (list != null)
            {
                TryMutateList(list);
            }

            Assert.That(PlacementConflictPairs.CrossKindPairsInOrder.Count, Is.EqualTo(originalCount));
            Assert.That(PlacementConflictPairs.CrossKindPairsInOrder[0], Is.EqualTo(originalFirst));
        }

        [Test]
        public void SameKindOverlapKinds_IListMutation_DoesNotChangeContent()
        {
            var original = PlacementConflictPairs.SameKindOverlapKinds.ToList();
            var list = PlacementConflictPairs.SameKindOverlapKinds as IList;
            if (list != null)
            {
                TryMutateList(list);
            }

            Assert.That(PlacementConflictPairs.SameKindOverlapKinds.ToList(), Is.EqualTo(original));
        }

        private static void TryMutateList(IList list)
        {
            try
            {
                list.Add(list.Count > 0 ? list[0] : null);
            }
            catch (System.NotSupportedException)
            {
            }

            try
            {
                list.Clear();
            }
            catch (System.NotSupportedException)
            {
            }
        }
    }
}
