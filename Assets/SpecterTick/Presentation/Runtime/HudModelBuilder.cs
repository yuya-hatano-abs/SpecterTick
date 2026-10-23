using System;
using System.Collections.Generic;
using System.Text;
using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public static class HudModelBuilder
    {
        private const int MaxShownCommands = 8;

        private const string KeysRecording = "MOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R";
        private const string KeysPlayback = "STEP: SPACE  RESET: R";
        private const string KeysCleared = "RESET: R";

        public static HudModel Build(GridPuzzleEngineSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var state = session.State;
            var directions = new List<MoveDirection>(session.RecordedCommands.Count);
            foreach (var command in session.RecordedCommands)
            {
                directions.Add(command.Direction);
            }

            return Build(state.Mode, state.Tick, state.PlaybackIndex, directions);
        }

        public static HudModel Build(
            GameMode mode,
            int tick,
            int playbackIndex,
            IReadOnlyList<MoveDirection> recordedDirections)
        {
            if (recordedDirections == null)
            {
                throw new ArgumentNullException(nameof(recordedDirections));
            }

            if (tick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), tick, "Tick cannot be negative.");
            }

            if (playbackIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playbackIndex), playbackIndex, "Playback index cannot be negative.");
            }

            string modeLine;
            string progressLine;
            string keysLine;
            switch (mode)
            {
                case GameMode.Recording:
                    modeLine = "MODE: RECORDING";
                    progressLine = "RECORDED: " + recordedDirections.Count;
                    keysLine = KeysRecording;
                    break;

                case GameMode.Playback:
                    modeLine = "MODE: PLAYBACK";
                    progressLine = "PLAYBACK: " + playbackIndex + " / " + recordedDirections.Count;
                    keysLine = KeysPlayback;
                    break;

                case GameMode.Cleared:
                    modeLine = "MODE: CLEARED";
                    progressLine = "RECORDED: " + recordedDirections.Count;
                    keysLine = KeysCleared;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "The game mode is not defined.");
            }

            var tickLine = "TICK: " + tick;
            var commandsLine = BuildCommandsLine(recordedDirections);

            return new HudModel(modeLine, tickLine, progressLine, keysLine, commandsLine);
        }

        private static string BuildCommandsLine(IReadOnlyList<MoveDirection> recordedDirections)
        {
            var count = recordedDirections.Count;
            if (count == 0)
            {
                return "COMMANDS: -";
            }

            var startIndex = 0;
            var prefix = "COMMANDS: ";
            if (count > MaxShownCommands)
            {
                startIndex = count - MaxShownCommands;
                prefix = "COMMANDS: ... ";
            }

            var builder = new StringBuilder(prefix);
            for (var i = startIndex; i < count; i++)
            {
                if (i > startIndex)
                {
                    builder.Append(' ');
                }

                builder.Append(SymbolFor(recordedDirections[i]));
            }

            return builder.ToString();
        }

        private static char SymbolFor(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.Up:
                    return 'U';
                case MoveDirection.Down:
                    return 'D';
                case MoveDirection.Left:
                    return 'L';
                case MoveDirection.Right:
                    return 'R';
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, "The move direction is not defined.");
            }
        }
    }
}
