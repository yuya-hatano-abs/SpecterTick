using System;

namespace SpecterTick.Presentation
{
    public sealed class ClearScreenModel
    {
        internal ClearScreenModel(
            bool isVisible,
            string titleLine,
            string resultLine,
            string hintLine)
        {
            IsVisible = isVisible;
            TitleLine = titleLine ?? throw new ArgumentNullException(nameof(titleLine));
            ResultLine = resultLine ?? throw new ArgumentNullException(nameof(resultLine));
            HintLine = hintLine ?? throw new ArgumentNullException(nameof(hintLine));
        }

        public bool IsVisible { get; }

        public string TitleLine { get; }

        public string ResultLine { get; }

        public string HintLine { get; }

        public string ToDisplayText()
        {
            if (!IsVisible)
            {
                return string.Empty;
            }

            return TitleLine + "\n" + ResultLine + "\n" + HintLine;
        }
    }
}
