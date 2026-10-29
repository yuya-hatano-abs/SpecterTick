using System;
using NUnit.Framework;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class ScreenSizeTrackerTests
    {
        [Test]
        public void SZ1_Initial_Size()
        {
            var tracker = new ScreenSizeTracker(1280, 720);

            Assert.That(tracker.Width, Is.EqualTo(1280));
            Assert.That(tracker.Height, Is.EqualTo(720));
        }

        [Test]
        public void SZ2_Update_DetectsChanges()
        {
            var tracker = new ScreenSizeTracker(1280, 720);

            Assert.That(tracker.Update(1280, 720), Is.False);
            Assert.That(tracker.Width, Is.EqualTo(1280));
            Assert.That(tracker.Height, Is.EqualTo(720));

            Assert.That(tracker.Update(1920, 720), Is.True);
            Assert.That(tracker.Width, Is.EqualTo(1920));
            Assert.That(tracker.Height, Is.EqualTo(720));

            Assert.That(tracker.Update(1920, 720), Is.False);
            Assert.That(tracker.Width, Is.EqualTo(1920));
            Assert.That(tracker.Height, Is.EqualTo(720));

            Assert.That(tracker.Update(1920, 1080), Is.True);
            Assert.That(tracker.Width, Is.EqualTo(1920));
            Assert.That(tracker.Height, Is.EqualTo(1080));

            Assert.That(tracker.Update(1920, 1080), Is.False);
            Assert.That(tracker.Width, Is.EqualTo(1920));
            Assert.That(tracker.Height, Is.EqualTo(1080));

            Assert.That(tracker.Update(1280, 720), Is.True);
            Assert.That(tracker.Width, Is.EqualTo(1280));
            Assert.That(tracker.Height, Is.EqualTo(720));
        }

        [Test]
        public void SZ3_Update_IgnoresInvalidSizes()
        {
            var tracker = new ScreenSizeTracker(1280, 720);

            Assert.That(tracker.Update(0, 0), Is.False);
            Assert.That(tracker.Update(0, 720), Is.False);
            Assert.That(tracker.Update(1280, 0), Is.False);
            Assert.That(tracker.Update(-1, 720), Is.False);
            Assert.That(tracker.Width, Is.EqualTo(1280));
            Assert.That(tracker.Height, Is.EqualTo(720));

            Assert.That(tracker.Update(1280, 720), Is.False);
            Assert.That(tracker.Width, Is.EqualTo(1280));
            Assert.That(tracker.Height, Is.EqualTo(720));
        }

        [Test]
        public void SZ4_Constructor_InvalidArguments()
        {
            var widthException = Assert.Throws<ArgumentOutOfRangeException>(() => new ScreenSizeTracker(0, 720));
            Assert.That(widthException.ParamName, Is.EqualTo("width"));

            var heightException = Assert.Throws<ArgumentOutOfRangeException>(() => new ScreenSizeTracker(1280, 0));
            Assert.That(heightException.ParamName, Is.EqualTo("height"));

            var bothException = Assert.Throws<ArgumentOutOfRangeException>(() => new ScreenSizeTracker(0, 0));
            Assert.That(bothException.ParamName, Is.EqualTo("width"));

            var negativeWidthException = Assert.Throws<ArgumentOutOfRangeException>(() => new ScreenSizeTracker(-1, 1));
            Assert.That(negativeWidthException.ParamName, Is.EqualTo("width"));

            var valid = new ScreenSizeTracker(1, 1);
            Assert.That(valid.Width, Is.EqualTo(1));
            Assert.That(valid.Height, Is.EqualTo(1));
        }
    }
}
