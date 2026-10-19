namespace SpecterTick.Core.Stages
{
    public readonly struct StageSwitch
    {
        public StageSwitch(SwitchId id, GridPosition position)
        {
            Id = id;
            Position = position;
        }

        public SwitchId Id { get; }
        public GridPosition Position { get; }
    }
}
