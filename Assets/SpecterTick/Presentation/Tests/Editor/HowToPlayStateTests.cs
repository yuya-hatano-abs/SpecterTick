using NUnit.Framework;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class HowToPlayStateTests
    {
        [Test]
        public void HS1_Initial_IsClosed()
        {
            var state = new HowToPlayState();
            Assert.That(state.IsOpen, Is.False);
        }

        [Test]
        public void HS2_Open_SetsIsOpen()
        {
            var state = new HowToPlayState();
            state.Open();
            Assert.That(state.IsOpen, Is.True);
        }

        [Test]
        public void HS3_Closed_HelpPressed_ReturnsOpened()
        {
            var state = new HowToPlayState();
            Assert.That(state.Update(true, false), Is.EqualTo(HowToPlayAction.Opened));
            Assert.That(state.IsOpen, Is.True);
        }

        [Test]
        public void HS4_Closed_OtherKeyOnly_ReturnsNone()
        {
            var state = new HowToPlayState();
            Assert.That(state.Update(false, true), Is.EqualTo(HowToPlayAction.None));
            Assert.That(state.IsOpen, Is.False);
        }

        [Test]
        public void HS5_Open_OtherKey_ReturnsClosed()
        {
            var state = new HowToPlayState();
            state.Open();
            Assert.That(state.Update(false, true), Is.EqualTo(HowToPlayAction.Closed));
            Assert.That(state.IsOpen, Is.False);
        }

        [Test]
        public void HS6_Open_H_ReturnsClosed()
        {
            var state = new HowToPlayState();
            state.Open();
            Assert.That(state.Update(true, true), Is.EqualTo(HowToPlayAction.Closed));
            Assert.That(state.IsOpen, Is.False);
        }

        [Test]
        public void HS7_Open_HAndOtherSameFrame_ReturnsClosedOnce()
        {
            var state = new HowToPlayState();
            state.Open();
            Assert.That(state.Update(true, true), Is.EqualTo(HowToPlayAction.Closed));
            Assert.That(state.IsOpen, Is.False);
            Assert.That(state.Update(true, true), Is.EqualTo(HowToPlayAction.Opened));
        }
    }
}
