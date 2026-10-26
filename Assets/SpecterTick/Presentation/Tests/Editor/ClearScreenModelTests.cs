using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class ClearScreenModelTests
    {
        [Test]
        public void CM1_Cleared_TickAndRecorded()
        {
            var model = ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5);

            Assert.That(model.IsVisible, Is.True);
            Assert.That(model.TitleLine, Is.EqualTo("CLEARED!"));
            Assert.That(model.ResultLine, Is.EqualTo("TICK: 4  RECORDED: 5"));
            Assert.That(model.HintLine, Is.EqualTo("PRESS R TO RETRY"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("CLEARED!\nTICK: 4  RECORDED: 5\nPRESS R TO RETRY"));
        }

        [Test]
        public void CM2_Cleared_ZeroTick_SevenRecorded()
        {
            var model = ClearScreenModelBuilder.Build(GameMode.Cleared, 0, 7);

            Assert.That(model.IsVisible, Is.True);
            Assert.That(model.TitleLine, Is.EqualTo("CLEARED!"));
            Assert.That(model.ResultLine, Is.EqualTo("TICK: 0  RECORDED: 7"));
            Assert.That(model.HintLine, Is.EqualTo("PRESS R TO RETRY"));
            Assert.That(
                model.ToDisplayText(),
                Is.EqualTo("CLEARED!\nTICK: 0  RECORDED: 7\nPRESS R TO RETRY"));
        }

        [Test]
        public void CM3_Recording_Hidden()
        {
            var model = ClearScreenModelBuilder.Build(GameMode.Recording, 3, 3);

            Assert.That(model.IsVisible, Is.False);
            Assert.That(model.TitleLine, Is.EqualTo(""));
            Assert.That(model.ResultLine, Is.EqualTo(""));
            Assert.That(model.HintLine, Is.EqualTo(""));
            Assert.That(model.ToDisplayText(), Is.EqualTo(""));
        }

        [Test]
        public void CM4_Playback_Hidden()
        {
            var model = ClearScreenModelBuilder.Build(GameMode.Playback, 2, 5);

            Assert.That(model.IsVisible, Is.False);
            Assert.That(model.TitleLine, Is.EqualTo(""));
            Assert.That(model.ResultLine, Is.EqualTo(""));
            Assert.That(model.HintLine, Is.EqualTo(""));
            Assert.That(model.ToDisplayText(), Is.EqualTo(""));
        }

        [Test]
        public void CM5_InvalidArguments()
        {
            var tick = Assert.Throws<ArgumentOutOfRangeException>(() =>
                ClearScreenModelBuilder.Build(GameMode.Cleared, -1, 0));
            Assert.That(tick.ParamName, Is.EqualTo("tick"));

            var recorded = Assert.Throws<ArgumentOutOfRangeException>(() =>
                ClearScreenModelBuilder.Build(GameMode.Cleared, 0, -1));
            Assert.That(recorded.ParamName, Is.EqualTo("recordedCount"));

            var tickFirst = Assert.Throws<ArgumentOutOfRangeException>(() =>
                ClearScreenModelBuilder.Build(GameMode.Cleared, -1, -1));
            Assert.That(tickFirst.ParamName, Is.EqualTo("tick"));

            var mode = Assert.Throws<ArgumentOutOfRangeException>(() =>
                ClearScreenModelBuilder.Build((GameMode)99, 0, 0));
            Assert.That(mode.ParamName, Is.EqualTo("mode"));

            Assert.DoesNotThrow(() => ClearScreenModelBuilder.Build(GameMode.Cleared, 0, 0));
            Assert.That(
                ClearScreenModelBuilder.Build(GameMode.Cleared, 0, 0).ToDisplayText(),
                Is.EqualTo("CLEARED!\nTICK: 0  RECORDED: 0\nPRESS R TO RETRY"));
        }

        [Test]
        public void CM6_HiddenMode_NegativeTick_Throws()
        {
            var recordingTick = Assert.Throws<ArgumentOutOfRangeException>(() =>
                ClearScreenModelBuilder.Build(GameMode.Recording, -1, 0));
            Assert.That(recordingTick.ParamName, Is.EqualTo("tick"));

            var playbackRecorded = Assert.Throws<ArgumentOutOfRangeException>(() =>
                ClearScreenModelBuilder.Build(GameMode.Playback, 0, -1));
            Assert.That(playbackRecorded.ParamName, Is.EqualTo("recordedCount"));
        }
    }
}
