namespace SpecterTick.Core.Stages
{
    /// <summary>
    /// Element kinds for stage validation. Enum order defines normalization (KindA &lt;= KindB) for cross-kind conflicts.
    /// </summary>
    public enum StageElementKind
    {
        Player = 0,
        Specter = 1,
        Exit = 2,
        Wall = 3,
        Switch = 4,
        Door = 5,
        Stage = 6
    }
}
