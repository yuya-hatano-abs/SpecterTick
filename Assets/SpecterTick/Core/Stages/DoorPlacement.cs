using System.Collections.Generic;

namespace SpecterTick.Core.Stages
{
    public readonly struct DoorPlacement
    {
        public DoorPlacement(DoorId id, GridPosition position, List<SwitchId> switchReferences)
        {
            Id = id;
            Position = position;
            SwitchReferences = switchReferences;
        }

        public DoorId Id { get; }
        public GridPosition Position { get; }
        public List<SwitchId> SwitchReferences { get; }
    }
}
