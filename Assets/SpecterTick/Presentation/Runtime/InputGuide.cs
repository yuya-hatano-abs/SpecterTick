using System;
using System.Text;
using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public static class InputGuide
    {
        private sealed class GuideRow
        {
            public GuideRow(
                InputIntentKind kind,
                string label,
                string displayKeys,
                InputKeys[] keys,
                GameMode[] allowedModes)
            {
                Kind = kind;
                Label = label;
                DisplayKeys = displayKeys;
                Keys = keys;
                AllowedModes = allowedModes;
            }

            public InputIntentKind Kind { get; }

            public string Label { get; }

            public string DisplayKeys { get; }

            public InputKeys[] Keys { get; }

            public GameMode[] AllowedModes { get; }
        }

        private static readonly GuideRow[] Rows =
        {
            new GuideRow(
                InputIntentKind.Move,
                "MOVE",
                "ARROWS/WASD",
                new[]
                {
                    InputKeys.ArrowUp,
                    InputKeys.ArrowDown,
                    InputKeys.ArrowLeft,
                    InputKeys.ArrowRight,
                    InputKeys.KeyW,
                    InputKeys.KeyA,
                    InputKeys.KeyS,
                    InputKeys.KeyD
                },
                new[] { GameMode.Recording }),
            new GuideRow(
                InputIntentKind.ConfirmRecording,
                "CONFIRM",
                "ENTER",
                new[] { InputKeys.Enter },
                new[] { GameMode.Recording }),
            new GuideRow(
                InputIntentKind.StepForward,
                "STEP",
                "SPACE",
                new[] { InputKeys.Space },
                new[] { GameMode.Playback }),
            new GuideRow(
                InputIntentKind.Retry,
                "RESET",
                "R",
                new[] { InputKeys.KeyR },
                new[] { GameMode.Recording, GameMode.Playback, GameMode.Cleared })
        };

        public static string KeysLineFor(GameMode mode)
        {
            ValidateMode(mode);

            var builder = new StringBuilder();
            var first = true;
            for (var i = 0; i < Rows.Length; i++)
            {
                var row = Rows[i];
                if (!IsAllowedForMode(row, mode))
                {
                    continue;
                }

                if (!first)
                {
                    builder.Append("  ");
                }

                builder.Append(row.Label);
                builder.Append(": ");
                builder.Append(row.DisplayKeys);
                first = false;
            }

            return builder.ToString();
        }

        public static bool IsAllowed(InputIntentKind kind, GameMode mode)
        {
            if (!IsDefinedKind(kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            if (!IsDefinedMode(mode))
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            if (kind == InputIntentKind.None)
            {
                return false;
            }

            for (var i = 0; i < Rows.Length; i++)
            {
                var row = Rows[i];
                if (row.Kind == kind)
                {
                    return IsAllowedForMode(row, mode);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        public static InputKeys[] GetKeys(InputIntentKind kind)
        {
            if (!IsDefinedKind(kind))
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            if (kind == InputIntentKind.None)
            {
                return Array.Empty<InputKeys>();
            }

            for (var i = 0; i < Rows.Length; i++)
            {
                var row = Rows[i];
                if (row.Kind == kind)
                {
                    var copy = new InputKeys[row.Keys.Length];
                    Array.Copy(row.Keys, copy, row.Keys.Length);
                    return copy;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        private static bool IsAllowedForMode(GuideRow row, GameMode mode)
        {
            for (var i = 0; i < row.AllowedModes.Length; i++)
            {
                if (row.AllowedModes[i] == mode)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDefinedKind(InputIntentKind kind)
        {
            switch (kind)
            {
                case InputIntentKind.None:
                case InputIntentKind.Move:
                case InputIntentKind.ConfirmRecording:
                case InputIntentKind.StepForward:
                case InputIntentKind.Retry:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsDefinedMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Recording:
                case GameMode.Playback:
                case GameMode.Cleared:
                    return true;
                default:
                    return false;
            }
        }

        private static void ValidateMode(GameMode mode)
        {
            if (!IsDefinedMode(mode))
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }
}
