using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class ValidatedStageImmutabilityTests
    {
        [Test]
        public void MutatingInputWalls_DoesNotChangeValidatedStage()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Walls.Add(new GridPosition(1, 1));
            var stage = StageValidator.Validate(definition).Stage;
            Assert.That(stage.Walls.Count, Is.EqualTo(1));

            definition.Walls.Add(new GridPosition(2, 2));
            definition.Walls.Clear();

            Assert.That(stage.Walls.Count, Is.EqualTo(1));
            Assert.That(stage.Walls[0], Is.EqualTo(new GridPosition(1, 1)));
        }

        [Test]
        public void MutatingDoorSwitchReferences_DoesNotChangeValidatedStage()
        {
            var references = new List<SwitchId> { new SwitchId("s1") };
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), new GridPosition(2, 2)));
            definition.Doors.Add(new DoorPlacement(new DoorId("d1"), new GridPosition(2, 3), references));

            var stage = StageValidator.Validate(definition).Stage;
            Assert.That(stage.Doors[0].SwitchReferences.Count, Is.EqualTo(1));

            references.Add(new SwitchId("s2"));
            references.Clear();

            Assert.That(stage.Doors[0].SwitchReferences.Count, Is.EqualTo(1));
            Assert.That(stage.Doors[0].SwitchReferences[0], Is.EqualTo(new SwitchId("s1")));
        }

        [Test]
        public void MutatingInputWalls_Reorder_DoesNotChangeValidatedStage()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Walls.Add(new GridPosition(1, 1));
            definition.Walls.Add(new GridPosition(2, 2));
            var stage = StageValidator.Validate(definition).Stage;
            Assert.That(stage.Walls.Count, Is.EqualTo(2));

            var first = definition.Walls[0];
            definition.Walls.RemoveAt(0);
            definition.Walls.Insert(1, first);

            Assert.That(stage.Walls.Count, Is.EqualTo(2));
            Assert.That(stage.Walls[0], Is.EqualTo(new GridPosition(1, 1)));
            Assert.That(stage.Walls[1], Is.EqualTo(new GridPosition(2, 2)));
        }

        [Test]
        public void MutatingInputSwitches_DoesNotChangeValidatedStage()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), new GridPosition(1, 1)));
            var stage = StageValidator.Validate(definition).Stage;

            definition.Switches.Add(new SwitchPlacement(new SwitchId("s2"), new GridPosition(2, 2)));
            definition.Switches.RemoveAt(0);
            definition.Switches.Insert(0, new SwitchPlacement(new SwitchId("s3"), new GridPosition(3, 3)));

            Assert.That(stage.Switches.Count, Is.EqualTo(1));
            Assert.That(stage.Switches[0].Id, Is.EqualTo(new SwitchId("s1")));
            Assert.That(stage.Switches[0].Position, Is.EqualTo(new GridPosition(1, 1)));
        }

        [Test]
        public void MutatingInputDoors_DoesNotChangeValidatedStage()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), new GridPosition(1, 1)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                new GridPosition(2, 2),
                new List<SwitchId> { new SwitchId("s1") }));
            var stage = StageValidator.Validate(definition).Stage;

            definition.Doors.Add(new DoorPlacement(
                new DoorId("d2"),
                new GridPosition(3, 3),
                new List<SwitchId> { new SwitchId("s1") }));
            definition.Doors.RemoveAt(0);

            Assert.That(stage.Doors.Count, Is.EqualTo(1));
            Assert.That(stage.Doors[0].Id, Is.EqualTo(new DoorId("d1")));
            Assert.That(stage.Doors[0].Position, Is.EqualTo(new GridPosition(2, 2)));
        }
    }
}
