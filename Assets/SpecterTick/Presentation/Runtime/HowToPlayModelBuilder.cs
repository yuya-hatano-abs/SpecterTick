using System;
using System.Collections.Generic;
using System.Text;
using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public static class HowToPlayModelBuilder
    {
        public const int MaxSingleLineLength = 68;

        private const string Heading = "HOW TO PLAY";
        private const string PurposeText =
            "OPEN THE EXIT AND REACH IT.\nSWITCHES ARE PRESSED BY YOU OR THE SPECTER.";
        private const string StepsText =
            "1. RECORD  MOVE WITH ARROWS/WASD. EVERY MOVE IS RECORDED.\n"
            + "2. CONFIRM  PRESS ENTER. SPECTER REPLAYS YOUR MOVES FROM START.\n"
            + "3. STEP  PRESS SPACE TO ADVANCE ONE TICK AT A TIME.";
        private const string RulesText =
            "END YOUR RECORDING ON THE EXIT.\n"
            + "WHEN ALL SWITCHES ARE ON, THE EXIT OPENS.\n"
            + "DOORS OPEN WHEN THEIR SWITCHES ARE ALL ON.";
        private const string StagesLine = "STAGES: N = NEXT  P = PREVIOUS";
        private const string HelpLine = "HELP: H";
        private const string ContinuePrompt = "PRESS ANY KEY TO CONTINUE";

        public static HowToPlayModel Build()
        {
            var keys = new StringBuilder();
            keys.Append(InputGuide.KeysLineFor(GameMode.Recording));
            keys.Append('\n');
            keys.Append(InputGuide.KeysLineFor(GameMode.Playback));
            keys.Append('\n');
            keys.Append(InputGuide.KeysLineFor(GameMode.Cleared));
            keys.Append('\n');
            keys.Append(StagesLine);
            keys.Append('\n');
            keys.Append(HelpLine);

            var legend = new HowToPlayLegendEntry[]
            {
                new HowToPlayLegendEntry("PLAYER", "YOU MOVE DURING RECORDING.", new[] { "Player" }),
                new HowToPlayLegendEntry("SPECTER", "REPLAYS YOUR MOVES FROM ITS START.", new[] { "Specter" }),
                new HowToPlayLegendEntry("WALL", "BLOCKS MOVEMENT.", new[] { "Wall" }),
                new HowToPlayLegendEntry(
                    "SWITCH",
                    "ACTIVATES WHEN STEPPED ON (LATCHED).",
                    new[] { "SwitchOff", "SwitchOn" }),
                new HowToPlayLegendEntry(
                    "DOOR",
                    "OPENS WHEN ITS SWITCHES ARE ALL ON.",
                    new[] { "DoorClosed", "DoorOpen" }),
                new HowToPlayLegendEntry(
                    "EXIT",
                    "OPENS WHEN ALL SWITCHES ARE ON.",
                    new[] { "ExitClosed", "ExitOpen" })
            };

            return new HowToPlayModel(
                Heading,
                PurposeText,
                StepsText,
                RulesText,
                keys.ToString(),
                ContinuePrompt,
                legend);
        }

        public static bool IsPrintableAscii(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c == '\n')
                {
                    continue;
                }

                if (c < 0x20 || c > 0x7E)
                {
                    return false;
                }
            }

            return true;
        }

        public static int MaxLineLength(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var max = 0;
            var start = 0;
            for (var i = 0; i <= value.Length; i++)
            {
                if (i == value.Length || value[i] == '\n')
                {
                    var len = i - start;
                    if (len > max)
                    {
                        max = len;
                    }

                    start = i + 1;
                }
            }

            return max;
        }

        public static IEnumerable<string> AllStrings(HowToPlayModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            yield return model.Heading;
            yield return model.PurposeText;
            yield return model.StepsText;
            yield return model.RulesText;
            yield return model.KeysText;
            yield return model.ContinuePrompt;
            for (var i = 0; i < model.LegendEntries.Count; i++)
            {
                var entry = model.LegendEntries[i];
                yield return entry.Title;
                yield return entry.Description;
            }
        }
    }
}
