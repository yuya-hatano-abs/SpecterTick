using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class SampleStageFactoryDemoTests
    {
        [Test]
        public void DS1_CreateDemo_Layout()
        {
            var stage = SampleStageFactory.CreateDemo();

            Assert.That(stage.Width, Is.EqualTo(6));
            Assert.That(stage.Height, Is.EqualTo(4));
            Assert.That(stage.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(stage.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(stage.ExitPosition, Is.EqualTo(new GridPosition(5, 0)));
            Assert.That(stage.Walls.Count, Is.EqualTo(2));
            Assert.That(stage.Walls[0], Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(stage.Walls[1], Is.EqualTo(new GridPosition(3, 1)));
            Assert.That(stage.Switches.Count, Is.EqualTo(2));
            Assert.That(stage.Switches[0].Id.RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Switches[0].Position, Is.EqualTo(new GridPosition(1, 2)));
            Assert.That(stage.Switches[1].Id.RawValue, Is.EqualTo("S2"));
            Assert.That(stage.Switches[1].Position, Is.EqualTo(new GridPosition(4, 2)));
            Assert.That(stage.Doors.Count, Is.EqualTo(2));
            Assert.That(stage.Doors[0].Id.RawValue, Is.EqualTo("D1"));
            Assert.That(stage.Doors[0].Position, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(stage.Doors[0].SwitchReferences.Count, Is.EqualTo(1));
            Assert.That(stage.Doors[0].SwitchReferences[0].RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Doors[1].Id.RawValue, Is.EqualTo("D2"));
            Assert.That(stage.Doors[1].Position, Is.EqualTo(new GridPosition(5, 1)));
            Assert.That(stage.Doors[1].SwitchReferences.Count, Is.EqualTo(2));
            Assert.That(stage.Doors[1].SwitchReferences[0].RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Doors[1].SwitchReferences[1].RawValue, Is.EqualTo("S2"));
        }
    }
}
