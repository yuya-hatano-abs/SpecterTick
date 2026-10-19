using System;
using System.Collections.Generic;

namespace SpecterTick.Core.Stages
{
    public sealed class ValidatedStage : IStage
    {
        private readonly GridPosition[] _walls;
        private readonly StageSwitch[] _switches;
        private readonly StageDoor[] _doors;

        internal ValidatedStage(
            int width,
            int height,
            GridPosition playerPosition,
            GridPosition specterPosition,
            GridPosition exitPosition,
            GridPosition[] walls,
            StageSwitch[] switches,
            StageDoor[] doors)
        {
            Width = width;
            Height = height;
            PlayerPosition = playerPosition;
            SpecterPosition = specterPosition;
            ExitPosition = exitPosition;
            _walls = walls ?? Array.Empty<GridPosition>();
            _switches = switches ?? Array.Empty<StageSwitch>();
            _doors = doors ?? Array.Empty<StageDoor>();
        }

        public int Width { get; }
        public int Height { get; }
        public GridPosition PlayerPosition { get; }
        public GridPosition SpecterPosition { get; }
        public GridPosition ExitPosition { get; }
        public IReadOnlyList<GridPosition> Walls => Array.AsReadOnly(_walls);
        public IReadOnlyList<StageSwitch> Switches => Array.AsReadOnly(_switches);
        public IReadOnlyList<StageDoor> Doors => Array.AsReadOnly(_doors);
    }
}
