using System;

namespace SpecterTick.Presentation
{
    public static class StageLabelModelBuilder
    {
        private const string KeysLineText = "PREV: P  NEXT: N";
        private const string TitleStagePrefix = "STAGE ";
        private const string TitleSeparator = " / ";
        private const string TitleNameSeparator = "  ";

        public static StageLabelModel Build(int index, int count, string name)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be at least 1.");
            }

            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var titleLine = TitleStagePrefix + (index + 1) + TitleSeparator + count + TitleNameSeparator + name;
            return new StageLabelModel(titleLine, KeysLineText);
        }
    }
}
