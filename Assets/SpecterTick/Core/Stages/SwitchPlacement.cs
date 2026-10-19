namespace SpecterTick.Core.Stages
{
    public readonly struct SwitchPlacement
    {
        public SwitchPlacement(SwitchId id, GridPosition position)
        {
            Id = id;
            Position = position;
        }

        public SwitchId Id { get; }
        public GridPosition Position { get; }
    }
}
