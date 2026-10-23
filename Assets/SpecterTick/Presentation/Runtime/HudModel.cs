using System;

namespace SpecterTick.Presentation
{
    public sealed class HudModel
    {
        internal HudModel(
            string modeLine,
            string tickLine,
            string progressLine,
            string keysLine,
            string commandsLine)
        {
            ModeLine = modeLine ?? throw new ArgumentNullException(nameof(modeLine));
            TickLine = tickLine ?? throw new ArgumentNullException(nameof(tickLine));
            ProgressLine = progressLine ?? throw new ArgumentNullException(nameof(progressLine));
            KeysLine = keysLine ?? throw new ArgumentNullException(nameof(keysLine));
            CommandsLine = commandsLine ?? throw new ArgumentNullException(nameof(commandsLine));
        }

        public string ModeLine { get; }

        public string TickLine { get; }

        public string ProgressLine { get; }

        public string KeysLine { get; }

        public string CommandsLine { get; }

        public string ToDisplayText()
        {
            return ModeLine
                + "\n"
                + TickLine
                + "\n"
                + ProgressLine
                + "\n"
                + KeysLine
                + "\n"
                + CommandsLine;
        }
    }
}
