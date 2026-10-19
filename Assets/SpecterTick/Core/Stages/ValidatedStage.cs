using System;
using System.Collections.Generic;

namespace SpecterTick.Core.Stages
{
    public sealed class ValidatedStage : IStage
    {
        private readonly IReadOnlyList<GridPosition> _walls;
        private readonly IReadOnlyList<StageSwitch> _switches;
        private readonly IReadOnlyList<StageDoor> _doors;

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
            _walls = CloneAsReadOnly(walls);
            _switches = CloneAsReadOnly(switches);
            _doors = CloneAsReadOnly(doors);
        }

        public int Width { get; }
        public int Height { get; }
        public GridPosition PlayerPosition { get; }
        public GridPosition SpecterPosition { get; }
        public GridPosition ExitPosition { get; }
        public IReadOnlyList<GridPosition> Walls => _walls;
        public IReadOnlyList<StageSwitch> Switches => _switches;
        public IReadOnlyList<StageDoor> Doors => _doors;

        private static IReadOnlyList<GridPosition> CloneAsReadOnly(GridPosition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.AsReadOnly(Array.Empty<GridPosition>());
            }

            var copy = new GridPosition[source.Length];
            Array.Copy(source, copy, source.Length);
            return Array.AsReadOnly(copy);
        }

        private static IReadOnlyList<StageSwitch> CloneAsReadOnly(StageSwitch[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.AsReadOnly(Array.Empty<StageSwitch>());
            }

            var copy = new StageSwitch[source.Length];
            Array.Copy(source, copy, source.Length);
            return Array.AsReadOnly(copy);
        }

        private static IReadOnlyList<StageDoor> CloneAsReadOnly(StageDoor[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.AsReadOnly(Array.Empty<StageDoor>());
            }

            var copy = new StageDoor[source.Length];
            Array.Copy(source, copy, source.Length);
            return Array.AsReadOnly(copy);
        }
    }
}
