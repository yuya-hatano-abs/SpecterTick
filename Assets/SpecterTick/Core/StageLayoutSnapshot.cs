using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;

namespace SpecterTick.Core
{
    internal sealed class StageLayoutSnapshot
    {
        internal StageLayoutSnapshot(
            int width,
            int height,
            GridPosition initialPlayerPosition,
            GridPosition initialSpecterPosition,
            HashSet<GridPosition> wallCells,
            HashSet<GridPosition> closedDoorCells)
        {
            Width = width;
            Height = height;
            InitialPlayerPosition = initialPlayerPosition;
            InitialSpecterPosition = initialSpecterPosition;
            WallCells = wallCells ?? new HashSet<GridPosition>();
            ClosedDoorCells = closedDoorCells ?? new HashSet<GridPosition>();
        }

        internal int Width { get; }
        internal int Height { get; }
        internal GridPosition InitialPlayerPosition { get; }
        internal GridPosition InitialSpecterPosition { get; }
        internal HashSet<GridPosition> WallCells { get; }
        internal HashSet<GridPosition> ClosedDoorCells { get; }

        internal static StageLayoutSnapshot FromStage(IStage stage)
        {
            var walls = new HashSet<GridPosition>();
            for (var i = 0; i < stage.Walls.Count; i++)
            {
                walls.Add(stage.Walls[i]);
            }

            var doors = new HashSet<GridPosition>();
            for (var i = 0; i < stage.Doors.Count; i++)
            {
                doors.Add(stage.Doors[i].Position);
            }

            return new StageLayoutSnapshot(
                stage.Width,
                stage.Height,
                stage.PlayerPosition,
                stage.SpecterPosition,
                walls,
                doors);
        }

        internal bool IsInside(GridPosition position)
        {
            return position.X >= 0
                   && position.Y >= 0
                   && position.X < Width
                   && position.Y < Height;
        }

        internal bool BlocksMovement(GridPosition position)
        {
            return WallCells.Contains(position) || ClosedDoorCells.Contains(position);
        }
    }
}
