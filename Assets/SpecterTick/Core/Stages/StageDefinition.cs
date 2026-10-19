using System.Collections.Generic;

namespace SpecterTick.Core.Stages
{
    public sealed class StageDefinition
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public GridPosition? PlayerPosition { get; set; }
        public GridPosition? SpecterPosition { get; set; }
        public GridPosition? ExitPosition { get; set; }
        public List<GridPosition> Walls { get; set; } = new List<GridPosition>();
        public List<SwitchPlacement> Switches { get; set; } = new List<SwitchPlacement>();
        public List<DoorPlacement> Doors { get; set; } = new List<DoorPlacement>();
    }
}
