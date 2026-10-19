using System;
using System.Collections.Generic;

namespace SpecterTick.Core.Stages
{
    public readonly struct StageDoor
    {
        public StageDoor(DoorId id, GridPosition position, SwitchId[] switchReferences)
        {
            Id = id;
            Position = position;
            var source = switchReferences ?? Array.Empty<SwitchId>();
            var owned = new SwitchId[source.Length];
            Array.Copy(source, owned, source.Length);
            SwitchReferences = Array.AsReadOnly(owned);
        }

        public DoorId Id { get; }
        public GridPosition Position { get; }
        public IReadOnlyList<SwitchId> SwitchReferences { get; }
    }
}
