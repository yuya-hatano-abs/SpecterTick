using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Presentation;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class StageCatalogTests
    {
        [Test]
        public void ST1_Count_And_Names()
        {
            Assert.That(StageCatalog.Count, Is.EqualTo(5));
            Assert.That(StageCatalog.GetName(0), Is.EqualTo("WALK"));
            Assert.That(StageCatalog.GetName(1), Is.EqualTo("ECHO"));
            Assert.That(StageCatalog.GetName(2), Is.EqualTo("GATE"));
            Assert.That(StageCatalog.GetName(3), Is.EqualTo("TWIN"));
            Assert.That(StageCatalog.GetName(4), Is.EqualTo("TANGLE"));
        }

        [Test]
        public void SL1_Walk_Layout()
        {
            var stage = StageCatalog.CreateStage(0);
            Assert.That(stage.Width, Is.EqualTo(4));
            Assert.That(stage.Height, Is.EqualTo(3));
            Assert.That(stage.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(stage.SpecterPosition, Is.EqualTo(new GridPosition(3, 0)));
            Assert.That(stage.ExitPosition, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(stage.Walls.Count, Is.EqualTo(2));
            Assert.That(stage.Walls[0], Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(stage.Walls[1], Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(stage.Switches.Count, Is.EqualTo(0));
            Assert.That(stage.Doors.Count, Is.EqualTo(0));
        }

        [Test]
        public void SL2_Echo_Layout()
        {
            var stage = StageCatalog.CreateStage(1);
            Assert.That(stage.Width, Is.EqualTo(5));
            Assert.That(stage.Height, Is.EqualTo(3));
            Assert.That(stage.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(stage.SpecterPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(stage.ExitPosition, Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(stage.Walls.Count, Is.EqualTo(1));
            Assert.That(stage.Walls[0], Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(stage.Switches.Count, Is.EqualTo(1));
            Assert.That(stage.Switches[0].Id.RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Switches[0].Position, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(stage.Doors.Count, Is.EqualTo(0));
        }

        [Test]
        public void SL3_Gate_Layout()
        {
            var stage = StageCatalog.CreateStage(2);
            Assert.That(stage.Width, Is.EqualTo(6));
            Assert.That(stage.Height, Is.EqualTo(4));
            Assert.That(stage.PlayerPosition, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(stage.SpecterPosition, Is.EqualTo(new GridPosition(3, 0)));
            Assert.That(stage.ExitPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(stage.Walls.Count, Is.EqualTo(4));
            Assert.That(stage.Walls[0], Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(stage.Walls[1], Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(stage.Walls[2], Is.EqualTo(new GridPosition(4, 0)));
            Assert.That(stage.Walls[3], Is.EqualTo(new GridPosition(4, 2)));
            Assert.That(stage.Switches.Count, Is.EqualTo(1));
            Assert.That(stage.Switches[0].Id.RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Switches[0].Position, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(stage.Doors.Count, Is.EqualTo(1));
            Assert.That(stage.Doors[0].Id.RawValue, Is.EqualTo("D1"));
            Assert.That(stage.Doors[0].Position, Is.EqualTo(new GridPosition(1, 3)));
            Assert.That(stage.Doors[0].SwitchReferences.Count, Is.EqualTo(1));
            Assert.That(stage.Doors[0].SwitchReferences[0].RawValue, Is.EqualTo("S1"));
        }

        [Test]
        public void SL4_Twin_Layout()
        {
            var stage = StageCatalog.CreateStage(3);
            Assert.That(stage.Width, Is.EqualTo(6));
            Assert.That(stage.Height, Is.EqualTo(4));
            Assert.That(stage.PlayerPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(stage.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(stage.ExitPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(stage.Walls.Count, Is.EqualTo(4));
            Assert.That(stage.Walls[0], Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(stage.Walls[1], Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(stage.Walls[2], Is.EqualTo(new GridPosition(2, 3)));
            Assert.That(stage.Walls[3], Is.EqualTo(new GridPosition(5, 0)));
            Assert.That(stage.Switches.Count, Is.EqualTo(2));
            Assert.That(stage.Switches[0].Id.RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Switches[0].Position, Is.EqualTo(new GridPosition(4, 2)));
            Assert.That(stage.Switches[1].Id.RawValue, Is.EqualTo("S2"));
            Assert.That(stage.Switches[1].Position, Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(stage.Doors.Count, Is.EqualTo(2));
            Assert.That(stage.Doors[0].Id.RawValue, Is.EqualTo("D1"));
            Assert.That(stage.Doors[0].Position, Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(stage.Doors[0].SwitchReferences[0].RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Doors[1].Id.RawValue, Is.EqualTo("D2"));
            Assert.That(stage.Doors[1].Position, Is.EqualTo(new GridPosition(3, 1)));
            Assert.That(stage.Doors[1].SwitchReferences[0].RawValue, Is.EqualTo("S2"));
        }

        [Test]
        public void SL5_Tangle_Layout()
        {
            var stage = StageCatalog.CreateStage(4);
            Assert.That(stage.Width, Is.EqualTo(6));
            Assert.That(stage.Height, Is.EqualTo(4));
            Assert.That(stage.PlayerPosition, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(stage.SpecterPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(stage.ExitPosition, Is.EqualTo(new GridPosition(0, 3)));
            Assert.That(stage.Walls.Count, Is.EqualTo(4));
            Assert.That(stage.Walls[0], Is.EqualTo(new GridPosition(2, 0)));
            Assert.That(stage.Walls[1], Is.EqualTo(new GridPosition(3, 2)));
            Assert.That(stage.Walls[2], Is.EqualTo(new GridPosition(4, 2)));
            Assert.That(stage.Walls[3], Is.EqualTo(new GridPosition(5, 1)));
            Assert.That(stage.Switches.Count, Is.EqualTo(2));
            Assert.That(stage.Switches[0].Id.RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Switches[0].Position, Is.EqualTo(new GridPosition(3, 1)));
            Assert.That(stage.Switches[1].Id.RawValue, Is.EqualTo("S2"));
            Assert.That(stage.Switches[1].Position, Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(stage.Doors.Count, Is.EqualTo(2));
            Assert.That(stage.Doors[0].Id.RawValue, Is.EqualTo("D1"));
            Assert.That(stage.Doors[0].Position, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(stage.Doors[0].SwitchReferences[0].RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Doors[0].SwitchReferences[1].RawValue, Is.EqualTo("S2"));
            Assert.That(stage.Doors[1].Id.RawValue, Is.EqualTo("D2"));
            Assert.That(stage.Doors[1].Position, Is.EqualTo(new GridPosition(1, 3)));
            Assert.That(stage.Doors[1].SwitchReferences[0].RawValue, Is.EqualTo("S1"));
            Assert.That(stage.Doors[1].SwitchReferences[1].RawValue, Is.EqualTo("S2"));
        }

        [Test]
        public void ST3_OutOfRange_Throws()
        {
            var createLow = Assert.Throws<ArgumentOutOfRangeException>(() => StageCatalog.CreateStage(-1));
            Assert.That(createLow.ParamName, Is.EqualTo("index"));

            var createHigh = Assert.Throws<ArgumentOutOfRangeException>(() => StageCatalog.CreateStage(5));
            Assert.That(createHigh.ParamName, Is.EqualTo("index"));

            var nameLow = Assert.Throws<ArgumentOutOfRangeException>(() => StageCatalog.GetName(-1));
            Assert.That(nameLow.ParamName, Is.EqualTo("index"));

            var nameHigh = Assert.Throws<ArgumentOutOfRangeException>(() => StageCatalog.GetName(5));
            Assert.That(nameHigh.ParamName, Is.EqualTo("index"));
        }

        [Test]
        public void ST4_CreateStage_ReturnsNewInstanceEachTime()
        {
            var a = StageCatalog.CreateStage(2);
            var b = StageCatalog.CreateStage(2);
            Assert.That(a, Is.Not.SameAs(b));
            Assert.That(a.Width, Is.EqualTo(b.Width));
            Assert.That(a.PlayerPosition, Is.EqualTo(b.PlayerPosition));
            Assert.That(a.ExitPosition, Is.EqualTo(b.ExitPosition));
        }
    }
}
