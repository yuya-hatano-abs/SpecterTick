using System;
using System.Collections.Generic;

namespace SpecterTick.Core.Stages
{
    /// <summary>
    /// Maps DESIGN_SPEC placement conflict pairs (8.4 items 1-18) to element kinds.
    /// Same-kind pairs (13, 16, 18) are handled via SameKindPositionOverlap.
    /// </summary>
    public static class PlacementConflictPairs
    {
        private static readonly (StageElementKind KindA, StageElementKind KindB)[] CrossKindPairsBacking =
        {
            (StageElementKind.Player, StageElementKind.Specter),
            (StageElementKind.Player, StageElementKind.Exit),
            (StageElementKind.Specter, StageElementKind.Exit),
            (StageElementKind.Player, StageElementKind.Wall),
            (StageElementKind.Specter, StageElementKind.Wall),
            (StageElementKind.Player, StageElementKind.Switch),
            (StageElementKind.Specter, StageElementKind.Switch),
            (StageElementKind.Player, StageElementKind.Door),
            (StageElementKind.Specter, StageElementKind.Door),
            (StageElementKind.Exit, StageElementKind.Wall),
            (StageElementKind.Exit, StageElementKind.Switch),
            (StageElementKind.Exit, StageElementKind.Door),
            (StageElementKind.Wall, StageElementKind.Switch),
            (StageElementKind.Wall, StageElementKind.Door),
            (StageElementKind.Switch, StageElementKind.Door)
        };

        private static readonly StageElementKind[] SameKindOverlapKindsBacking =
        {
            StageElementKind.Wall,
            StageElementKind.Switch,
            StageElementKind.Door
        };

        public static readonly IReadOnlyList<(StageElementKind KindA, StageElementKind KindB)> CrossKindPairsInOrder =
            Array.AsReadOnly(CrossKindPairsBacking);

        public static readonly IReadOnlyList<StageElementKind> SameKindOverlapKinds =
            Array.AsReadOnly(SameKindOverlapKindsBacking);

        public static void NormalizeKinds(
            StageElementKind kindA,
            StageElementKind kindB,
            out StageElementKind normalizedA,
            out StageElementKind normalizedB)
        {
            if ((int)kindA <= (int)kindB)
            {
                normalizedA = kindA;
                normalizedB = kindB;
            }
            else
            {
                normalizedA = kindB;
                normalizedB = kindA;
            }
        }
    }
}
