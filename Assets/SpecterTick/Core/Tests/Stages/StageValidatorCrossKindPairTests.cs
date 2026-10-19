using System.Collections.Generic;
using System.Linq;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using NUnit.Framework;

namespace SpecterTick.Core.Tests.Stages
{
    [TestFixture]
    public sealed class StageValidatorCrossKindPairTests
    {
        private static readonly GridPosition Conflict = new GridPosition(2, 2);

        private static IEnumerable<TestCaseData> CrossKindPairCases()
        {
            for (var i = 0; i < PlacementConflictPairs.CrossKindPairsInOrder.Count; i++)
            {
                var pair = PlacementConflictPairs.CrossKindPairsInOrder[i];
                yield return new TestCaseData(pair.KindA, pair.KindB)
                    .SetName($"CrossKindConflict_{pair.KindA}_And_{pair.KindB}");
            }
        }

        [TestCaseSource(nameof(CrossKindPairCases))]
        public void CrossKindConflict_IsDetected(StageElementKind kindA, StageElementKind kindB)
        {
            var definition = CreateIsolatedConflict(kindA, kindB);
            var errors = StageValidator.Validate(definition).Errors
                .Where(e => e.Code == StageValidationErrorCode.CrossKindPositionConflict)
                .ToList();

            Assert.That(errors.Count, Is.EqualTo(1));
            PlacementConflictPairs.NormalizeKinds(kindA, kindB, out var normalizedA, out var normalizedB);
            Assert.That(errors[0].Subject.Kind, Is.EqualTo(normalizedA));
            Assert.That(errors[0].Subject.OtherKind, Is.EqualTo(normalizedB));
            Assert.That(errors[0].Subject.Position, Is.EqualTo(Conflict));
        }

        private static StageDefinition CreateIsolatedConflict(StageElementKind kindA, StageElementKind kindB)
        {
            var definition = StageDefinitionTestBuilder.CreateMinimalValid(5, 5);
            definition.PlayerPosition = new GridPosition(0, 0);
            definition.SpecterPosition = new GridPosition(1, 0);
            definition.ExitPosition = new GridPosition(3, 3);
            Place(definition, kindA, Conflict);
            Place(definition, kindB, Conflict);
            return definition;
        }

        private static void Place(StageDefinition definition, StageElementKind kind, GridPosition position)
        {
            switch (kind)
            {
                case StageElementKind.Player:
                    definition.PlayerPosition = position;
                    break;
                case StageElementKind.Specter:
                    definition.SpecterPosition = position;
                    break;
                case StageElementKind.Exit:
                    definition.ExitPosition = position;
                    break;
                case StageElementKind.Wall:
                    definition.Walls.Add(position);
                    break;
                case StageElementKind.Switch:
                    definition.Switches.Add(new SwitchPlacement(new SwitchId("s_" + definition.Switches.Count), position));
                    break;
                case StageElementKind.Door:
                    SwitchId doorSwitchId;
                    if (definition.Switches.Count == 0)
                    {
                        doorSwitchId = new SwitchId("s_door");
                        definition.Switches.Add(new SwitchPlacement(doorSwitchId, new GridPosition(4, 4)));
                    }
                    else
                    {
                        doorSwitchId = definition.Switches[definition.Switches.Count - 1].Id;
                    }

                    definition.Doors.Add(new DoorPlacement(
                        new DoorId("d_" + definition.Doors.Count),
                        position,
                        new List<SwitchId> { doorSwitchId }));
                    break;
            }
        }
    }
}
