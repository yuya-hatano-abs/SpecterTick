using System.Collections.Generic;

namespace SpecterTick.Core.Stages
{
    public interface IStage
    {
        int Width { get; }
        int Height { get; }
        GridPosition PlayerPosition { get; }
        GridPosition SpecterPosition { get; }
        GridPosition ExitPosition { get; }
        IReadOnlyList<GridPosition> Walls { get; }
        IReadOnlyList<StageSwitch> Switches { get; }
        IReadOnlyList<StageDoor> Doors { get; }
    }
}
