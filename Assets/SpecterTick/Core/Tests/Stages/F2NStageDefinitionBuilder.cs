using System.Collections.Generic;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;

namespace SpecterTick.Core.Tests.Stages
{
    internal static class F2NStageDefinitionBuilder
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
                Switches = new List<SwitchPlacement>(),
                Doors = new List<DoorPlacement>()
            };
        }

        public static IStage CreateValidatedStage()
        {
            return StageValidator.Validate(CreateDefinition()).Stage;
        }
    }
}
