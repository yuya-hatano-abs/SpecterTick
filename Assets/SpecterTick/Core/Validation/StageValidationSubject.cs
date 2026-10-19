using SpecterTick.Core.Stages;

namespace SpecterTick.Core.Validation
{
    public readonly struct StageValidationSubject
    {
        public StageValidationSubject(
            StageElementKind kind,
            int index = -1,
            GridPosition? position = null,
            SwitchId switchId = default,
            DoorId doorId = default,
            StageElementKind otherKind = default,
            int referenceIndex = -1)
        {
            Kind = kind;
            Index = index;
            Position = position;
            SwitchId = switchId;
            DoorId = doorId;
            OtherKind = otherKind;
            ReferenceIndex = referenceIndex;
        }

        public StageElementKind Kind { get; }
        public int Index { get; }
        public GridPosition? Position { get; }
        public SwitchId SwitchId { get; }
        public DoorId DoorId { get; }
        public StageElementKind OtherKind { get; }
        public int ReferenceIndex { get; }
    }
}
