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
            GoalLine = null;
        }

        internal HudModel(
            string modeLine,
            string tickLine,
            string progressLine,
            string keysLine,
            string commandsLine,
            string goalLine)
        {
            ModeLine = modeLine ?? throw new ArgumentNullException(nameof(modeLine));
            TickLine = tickLine ?? throw new ArgumentNullException(nameof(tickLine));
            ProgressLine = progressLine ?? throw new ArgumentNullException(nameof(progressLine));
            KeysLine = keysLine ?? throw new ArgumentNullException(nameof(keysLine));
            CommandsLine = commandsLine ?? throw new ArgumentNullException(nameof(commandsLine));
            GoalLine = goalLine ?? throw new ArgumentNullException(nameof(goalLine));
        }

        public string ModeLine { get; }

        public string TickLine { get; }

        public string ProgressLine { get; }

        /// <summary>
        /// Goal progress line; null for models built by the four-argument <c>HudModelBuilder.Build</c>.
        /// </summary>
        public string GoalLine { get; }

        public string KeysLine { get; }

        public string CommandsLine { get; }

        public string ToDisplayText()
        {
            if (GoalLine == null)
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

            return ModeLine
                + "\n"
                + TickLine
                + "\n"
                + ProgressLine
                + "\n"
                + GoalLine
                + "\n"
                + KeysLine
                + "\n"
                + CommandsLine;
        }
    }
}
