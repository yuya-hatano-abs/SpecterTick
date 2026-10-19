using System.Collections.Generic;
using SpecterTick.Core.Stages;

namespace SpecterTick.Core.Tests.Stages
{
    internal static class StageDefinitionTestBuilder
    {
        public static StageDefinition CreateMinimalValid(int width = 4, int height = 4)
        {
            return new StageDefinition
            {
                Width = width,
                Height = height,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(3, 3),
                Walls = new List<GridPosition>(),
                Switches = new List<SwitchPlacement>(),
                Doors = new List<DoorPlacement>()
            };
        }
    }
}
