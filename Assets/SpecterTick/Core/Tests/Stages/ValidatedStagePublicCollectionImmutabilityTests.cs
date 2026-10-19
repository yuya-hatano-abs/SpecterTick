using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class ValidatedStagePublicCollectionImmutabilityTests
    {
        private IStage _stage;
        private GridPosition _originalWall;
        private SwitchId _originalSwitchId;
        private GridPosition _originalSwitchPosition;
        private DoorId _originalDoorId;
        private GridPosition _originalDoorPosition;
        private SwitchId _originalDoorSwitchRef;

        [SetUp]
        public void SetUp()
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(5, 5);
            definition.ExitPosition = new GridPosition(0, 3);
            definition.Walls.Add(new GridPosition(1, 1));
            definition.Switches.Add(new SwitchPlacement(new SwitchId("s1"), new GridPosition(2, 2)));
            definition.Doors.Add(new DoorPlacement(
                new DoorId("d1"),
                new GridPosition(4, 4),
                new List<SwitchId> { new SwitchId("s1") }));

            _stage = StageValidator.Validate(definition).Stage;
            _originalWall = _stage.Walls[0];
            _originalSwitchId = _stage.Switches[0].Id;
            _originalSwitchPosition = _stage.Switches[0].Position;
            _originalDoorId = _stage.Doors[0].Id;
            _originalDoorPosition = _stage.Doors[0].Position;
            _originalDoorSwitchRef = _stage.Doors[0].SwitchReferences[0];
        }

        [Test]
        public void Walls_IListMutation_DoesNotChangeStage()
        {
            var list = _stage.Walls as IList;
            if (list != null)
            {
                TryMutateList(list);
            }

            AssertStageUnchanged();
        }

        [Test]
        public void Walls_ArrayMutation_DoesNotChangeStage()
        {
            var array = _stage.Walls as GridPosition[];
            if (array != null)
            {
                array[0] = new GridPosition(9, 9);
            }

            AssertStageUnchanged();
        }

        [Test]
        public void Switches_IListMutation_DoesNotChangeStage()
        {
            var list = _stage.Switches as IList;
            if (list != null)
            {
                TryMutateList(list);
            }

            AssertStageUnchanged();
        }

        [Test]
        public void Doors_IListMutation_DoesNotChangeStage()
        {
            var list = _stage.Doors as IList;
            if (list != null)
            {
                TryMutateList(list);
            }

            AssertStageUnchanged();
        }

        [Test]
        public void DoorSwitchReferences_IListMutation_DoesNotChangeStage()
        {
            var references = _stage.Doors[0].SwitchReferences;
            var list = references as IList;
            if (list != null)
            {
                TryMutateList(list);
            }

            var array = references as SwitchId[];
            if (array != null && array.Length > 0)
            {
                array[0] = new SwitchId("mutated");
            }

            AssertStageUnchanged();
        }

        private static void TryMutateList(IList list)
        {
            try
            {
                if (list.Count > 0)
                {
                    list[0] = list[0];
                }

                list.Add(list.Count > 0 ? list[0] : null);
            }
            catch (NotSupportedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void AssertStageUnchanged()
        {
            Assert.That(_stage.Walls.Count, Is.EqualTo(1));
            Assert.That(_stage.Walls[0], Is.EqualTo(_originalWall));
            Assert.That(_stage.Switches.Count, Is.EqualTo(1));
            Assert.That(_stage.Switches[0].Id, Is.EqualTo(_originalSwitchId));
            Assert.That(_stage.Switches[0].Position, Is.EqualTo(_originalSwitchPosition));
            Assert.That(_stage.Doors.Count, Is.EqualTo(1));
            Assert.That(_stage.Doors[0].Id, Is.EqualTo(_originalDoorId));
            Assert.That(_stage.Doors[0].Position, Is.EqualTo(_originalDoorPosition));
            Assert.That(_stage.Doors[0].SwitchReferences.Count, Is.EqualTo(1));
            Assert.That(_stage.Doors[0].SwitchReferences[0], Is.EqualTo(_originalDoorSwitchRef));
        }
    }
}
