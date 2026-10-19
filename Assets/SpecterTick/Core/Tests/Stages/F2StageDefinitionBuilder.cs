using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;

namespace SpecterTick.Core.Tests.Stages
{
    internal static class F2StageDefinitionBuilder
    {
        public static StageDefinition CreateDefinition()
        {
            return new StageDefinition
            {
                Width = 5,
                Height = 5,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(4, 4),
                ExitPosition = new GridPosition(4, 0),
                Walls = new List<GridPosition>(),
                Switches = new List<SwitchPlacement>
                {
                    new SwitchPlacement(new SwitchId("SA"), new GridPosition(0, 1)),
                    new SwitchPlacement(new SwitchId("SB"), new GridPosition(2, 1))
                },
                Doors = new List<DoorPlacement>
                {
                    new DoorPlacement(
                        new DoorId("DA"),
                        new GridPosition(0, 2),
                        new List<SwitchId> { new SwitchId("SA") }),
                    new DoorPlacement(
                        new DoorId("DAB"),
                        new GridPosition(2, 2),
                        new List<SwitchId> { new SwitchId("SA"), new SwitchId("SB") })
                }
            };
        }

        public static IStage CreateValidatedStage()
        {
            return StageValidator.Validate(CreateDefinition()).Stage;
        }
    }
}
