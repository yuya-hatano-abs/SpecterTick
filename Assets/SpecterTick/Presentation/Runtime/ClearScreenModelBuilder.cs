using System;
using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public static class ClearScreenModelBuilder
    {
        private const string TitleCleared = "CLEARED!";
        private const string HintRetry = "PRESS R TO RETRY";
        private const string RecordedSeparator = "  RECORDED: ";

        public static ClearScreenModel Build(GridPuzzleEngineSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var state = session.State;
            return Build(state.Mode, state.Tick, session.RecordedCommands.Count);
        }

        public static ClearScreenModel Build(GameMode mode, int tick, int recordedCount)
        {
            if (tick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), tick, "Tick cannot be negative.");
            }

            if (recordedCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(recordedCount), recordedCount, "Recorded count cannot be negative.");
            }

            switch (mode)
            {
                case GameMode.Recording:
                case GameMode.Playback:
                    return new ClearScreenModel(false, string.Empty, string.Empty, string.Empty);

                case GameMode.Cleared:
                    var resultLine = "TICK: " + tick + RecordedSeparator + recordedCount;
                    return new ClearScreenModel(true, TitleCleared, resultLine, HintRetry);

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "The game mode is not defined.");
            }
        }
    }
}
