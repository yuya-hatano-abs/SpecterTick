using System;
using System.Collections.Generic;
using SpecterTick.Core.Stages;

namespace SpecterTick.Core
{
    internal sealed class StageLayoutSnapshot
    {
        private readonly Dictionary<GridPosition, SwitchId> _switchAtPosition;
        private readonly DoorSnapshot[] _doors;
        private readonly Dictionary<DoorId, int> _doorIndexById;
        private readonly SwitchId[] _allSwitchIds;

        internal StageLayoutSnapshot(
            int width,
            int height,
            GridPosition initialPlayerPosition,
            GridPosition initialSpecterPosition,
            GridPosition exitPosition,
            HashSet<GridPosition> wallCells,
            Dictionary<GridPosition, SwitchId> switchAtPosition,
            DoorSnapshot[] doors,
            SwitchId[] allSwitchIds)
        {
            Width = width;
            Height = height;
            InitialPlayerPosition = initialPlayerPosition;
            InitialSpecterPosition = initialSpecterPosition;
            ExitPosition = exitPosition;
            WallCells = wallCells ?? new HashSet<GridPosition>();
            _switchAtPosition = switchAtPosition ?? new Dictionary<GridPosition, SwitchId>();
            _doors = doors ?? Array.Empty<DoorSnapshot>();
            _allSwitchIds = allSwitchIds ?? Array.Empty<SwitchId>();
            _doorIndexById = new Dictionary<DoorId, int>();
            for (var i = 0; i < _doors.Length; i++)
            {
                _doorIndexById[_doors[i].Id] = i;
            }
        }

        internal int Width { get; }
        internal int Height { get; }
        internal GridPosition InitialPlayerPosition { get; }
        internal GridPosition InitialSpecterPosition { get; }
        internal GridPosition ExitPosition { get; }
        internal HashSet<GridPosition> WallCells { get; }

        internal static StageLayoutSnapshot FromStage(IStage stage)
        {
            var walls = new HashSet<GridPosition>();
            for (var i = 0; i < stage.Walls.Count; i++)
            {
                walls.Add(stage.Walls[i]);
            }

            var switchAtPosition = new Dictionary<GridPosition, SwitchId>();
            var allSwitches = new List<SwitchId>();
            for (var i = 0; i < stage.Switches.Count; i++)
            {
                var sw = stage.Switches[i];
                switchAtPosition[sw.Position] = sw.Id;
                allSwitches.Add(sw.Id);
            }

            var doors = new DoorSnapshot[stage.Doors.Count];
            for (var i = 0; i < stage.Doors.Count; i++)
            {
                var door = stage.Doors[i];
                var refs = new SwitchId[door.SwitchReferences.Count];
                for (var r = 0; r < door.SwitchReferences.Count; r++)
                {
                    refs[r] = door.SwitchReferences[r];
                }

                doors[i] = new DoorSnapshot(door.Id, door.Position, refs);
            }

            return new StageLayoutSnapshot(
                stage.Width,
                stage.Height,
                stage.PlayerPosition,
                stage.SpecterPosition,
                stage.ExitPosition,
                walls,
                switchAtPosition,
                doors,
                allSwitches.ToArray());
        }

        internal bool IsInside(GridPosition position)
        {
            return position.X >= 0
                   && position.Y >= 0
                   && position.X < Width
                   && position.Y < Height;
        }

        internal bool BlocksMovement(GridPosition position, ActivatedSwitchSet activated)
        {
            if (WallCells.Contains(position))
            {
                return true;
            }

            for (var i = 0; i < _doors.Length; i++)
            {
                var door = _doors[i];
                if (door.Position.Equals(position) && !IsDoorOpen(activated, door.Id))
                {
                    return true;
                }
            }

            return false;
        }

        internal ActivatedSwitchSet ApplyLatch(
            ActivatedSwitchSet current,
            IEnumerable<GridPosition> occupantPositions)
        {
            if (occupantPositions == null)
            {
                throw new ArgumentNullException(nameof(occupantPositions));
            }

            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            var next = current;
            foreach (var position in occupantPositions)
            {
                if (_switchAtPosition.TryGetValue(position, out var switchId))
                {
                    next = next.Add(switchId);
                }
            }

            return next;
        }

        internal bool IsDoorOpen(ActivatedSwitchSet activated, DoorId doorId)
        {
            if (!doorId.IsValid)
            {
                throw new ArgumentException("Door ID is not valid.", nameof(doorId));
            }

            if (!_doorIndexById.TryGetValue(doorId, out var index))
            {
                throw new ArgumentException("Door ID was not found on the stage.", nameof(doorId));
            }

            return AreAllReferencesActivated(activated, _doors[index].SwitchReferences);
        }

        internal bool AllSwitchesActivated(ActivatedSwitchSet activated)
        {
            for (var i = 0; i < _allSwitchIds.Length; i++)
            {
                if (!activated.Contains(_allSwitchIds[i]))
                {
                    return false;
                }
            }

            return true;
        }

        internal bool IsExitOpen(ActivatedSwitchSet activated)
        {
            return AllSwitchesActivated(activated);
        }

        private static bool AreAllReferencesActivated(ActivatedSwitchSet activated, SwitchId[] references)
        {
            for (var i = 0; i < references.Length; i++)
            {
                if (!activated.Contains(references[i]))
                {
                    return false;
                }
            }

            return true;
        }

        internal readonly struct DoorSnapshot
        {
            internal DoorSnapshot(DoorId id, GridPosition position, SwitchId[] switchReferences)
            {
                Id = id;
                Position = position;
                SwitchReferences = switchReferences ?? Array.Empty<SwitchId>();
            }

            internal DoorId Id { get; }
            internal GridPosition Position { get; }
            internal SwitchId[] SwitchReferences { get; }
        }
    }
}
