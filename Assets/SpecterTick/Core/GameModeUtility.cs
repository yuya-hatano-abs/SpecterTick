namespace SpecterTick.Core
{
    internal static class GameModeUtility
    {
        internal static bool IsDefined(GameMode mode)
        {
            return mode == GameMode.Recording
                   || mode == GameMode.Playback
                   || mode == GameMode.Cleared;
        }
    }
}
