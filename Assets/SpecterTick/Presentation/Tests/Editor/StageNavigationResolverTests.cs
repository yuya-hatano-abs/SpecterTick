using NUnit.Framework;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class StageNavigationResolverTests
    {
        [Test]
        public void NR1_Resolve_AllCombinations()
        {
            Assert.That(StageNavigationResolver.Resolve(false, false), Is.EqualTo(StageNavigation.None));
            Assert.That(StageNavigationResolver.Resolve(true, false), Is.EqualTo(StageNavigation.Next));
            Assert.That(StageNavigationResolver.Resolve(false, true), Is.EqualTo(StageNavigation.Previous));
            Assert.That(StageNavigationResolver.Resolve(true, true), Is.EqualTo(StageNavigation.Next));
        }
    }
}
