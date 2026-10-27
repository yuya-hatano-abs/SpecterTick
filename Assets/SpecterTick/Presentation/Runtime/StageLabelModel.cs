using System;

namespace SpecterTick.Presentation
{
    public sealed class StageLabelModel
    {
        internal StageLabelModel(string titleLine, string keysLine)
        {
            TitleLine = titleLine ?? throw new ArgumentNullException(nameof(titleLine));
            KeysLine = keysLine ?? throw new ArgumentNullException(nameof(keysLine));
        }

        public string TitleLine { get; }

        public string KeysLine { get; }

        public string ToDisplayText()
        {
            return TitleLine + "\n" + KeysLine;
        }
    }
}
