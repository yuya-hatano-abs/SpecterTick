using System;
using NUnit.Framework;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class StageFlowTests
    {
        [Test]
        public void SF1_Navigate_Clamps()
        {
            var flow = new StageFlow(3);
            Assert.That(flow.StageCount, Is.EqualTo(3));
            Assert.That(flow.CurrentIndex, Is.EqualTo(0));

            Assert.That(flow.Navigate(StageNavigation.None), Is.False);
            Assert.That(flow.CurrentIndex, Is.EqualTo(0));

            Assert.That(flow.Navigate(StageNavigation.Previous), Is.False);
            Assert.That(flow.CurrentIndex, Is.EqualTo(0));

            Assert.That(flow.Navigate(StageNavigation.Next), Is.True);
            Assert.That(flow.CurrentIndex, Is.EqualTo(1));

            Assert.That(flow.Navigate(StageNavigation.Next), Is.True);
            Assert.That(flow.CurrentIndex, Is.EqualTo(2));

            Assert.That(flow.Navigate(StageNavigation.Next), Is.False);
            Assert.That(flow.CurrentIndex, Is.EqualTo(2));

            Assert.That(flow.Navigate(StageNavigation.Previous), Is.True);
            Assert.That(flow.CurrentIndex, Is.EqualTo(1));

            Assert.That(flow.Navigate(StageNavigation.Previous), Is.True);
            Assert.That(flow.CurrentIndex, Is.EqualTo(0));

            Assert.That(flow.Navigate(StageNavigation.Previous), Is.False);
            Assert.That(flow.CurrentIndex, Is.EqualTo(0));
        }

        [Test]
        public void SF2_InvalidArguments()
        {
            var countEx = Assert.Throws<ArgumentOutOfRangeException>(() => new StageFlow(0));
            Assert.That(countEx.ParamName, Is.EqualTo("stageCount"));

            countEx = Assert.Throws<ArgumentOutOfRangeException>(() => new StageFlow(-1));
            Assert.That(countEx.ParamName, Is.EqualTo("stageCount"));

            var flow = new StageFlow(3);
            Assert.That(flow.Navigate(StageNavigation.Next), Is.True);
            Assert.That(flow.CurrentIndex, Is.EqualTo(1));

            var navEx = Assert.Throws<ArgumentOutOfRangeException>(() => flow.Navigate((StageNavigation)99));
            Assert.That(navEx.ParamName, Is.EqualTo("navigation"));
            Assert.That(flow.CurrentIndex, Is.EqualTo(1));
        }

        [Test]
        public void SF3_SingleStage_NeverMoves()
        {
            var flow = new StageFlow(1);
            Assert.That(flow.Navigate(StageNavigation.Next), Is.False);
            Assert.That(flow.CurrentIndex, Is.EqualTo(0));
            Assert.That(flow.Navigate(StageNavigation.Previous), Is.False);
            Assert.That(flow.CurrentIndex, Is.EqualTo(0));
        }
    }
}
