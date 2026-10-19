using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;

namespace SpecterTick.Core.Tests.Stages
{
    internal static class F1StageDefinitionBuilder
    {
        public static StageDefinition CreateDefinition(GridPosition? playerOverride = null)
        {
            return new StageDefinition
            {
                Width = 5,
                Height = 5,
                PlayerPosition = playerOverride ?? new GridPosition(1, 1),
                SpecterPosition = new GridPosition(3, 3),
                ExitPosition = new GridPosition(4, 4),
                Walls = new List<GridPosition> { new GridPosition(2, 1) },
                Switches = new List<SwitchPlacement>
                {
                    new SwitchPlacement(new SwitchId("S1"), new GridPosition(0, 4))
                },
                Doors = new List<DoorPlacement>
                {
                    new DoorPlacement(
                        new DoorId("D1"),
                        new GridPosition(1, 2),
                        new List<SwitchId> { new SwitchId("S1") })
                }
            };
        }

        public static IStage CreateValidatedStage(GridPosition? playerOverride = null)
        {
            return StageValidator.Validate(CreateDefinition(playerOverride)).Stage;
        }
    }
}
