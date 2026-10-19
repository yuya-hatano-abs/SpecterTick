using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorSuccessTests
    {
        [Test]
        public void ValidMinimalStage_Succeeds()
        {
            var result = StageValidator.Validate(StageDefinitionTestBuilder.CreateMinimalValid());
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void ValidStage_WithWallsSwitchesDoors_MatchesIStage()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(6, 6);
            definition.Walls.Add(new GridPosition(2, 2));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), new GridPosition(2, 0)));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s2"), new GridPosition(3, 0)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                new GridPosition(5, 0),
                new List<SwitchId> { new SwitchId("s1"), new SwitchId("s2") }));

            var stage = StageValidator.Validate(definition).Stage;

            Assert.That(stage.Width, Is.EqualTo(6));
            Assert.That(stage.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(stage.Walls.Count, Is.EqualTo(1));
            Assert.That(stage.Switches.Count, Is.EqualTo(2));
            Assert.That(stage.Doors.Count, Is.EqualTo(1));
            Assert.That(stage.Doors[0].SwitchReferences.Count, Is.EqualTo(2));
        }

        [Test]
        public void MultipleDoors_CanReferenceSameSwitch()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(5, 5);
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), new GridPosition(2, 1)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                new GridPosition(4, 1),
                new List<SwitchId> { new SwitchId("s1") }));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d2"),
                new GridPosition(4, 2),
                new List<SwitchId> { new SwitchId("s1") }));

            Assert.That(StageValidator.Validate(definition).IsSuccess, Is.True);
        }

        [Test]
        public void EmptyCollections_AreValid()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid();
            definition.Walls = new List<GridPosition>();
            definition.Switches = new List<SwitchPlacement>();
            definition.Doors = new List<DoorPlacement>();

            Assert.That(StageValidator.Validate(definition).IsSuccess, Is.True);
        }
    }
}
