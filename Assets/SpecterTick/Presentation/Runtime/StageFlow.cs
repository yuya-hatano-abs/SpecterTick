using System;

namespace SpecterTick.Presentation
{
    public sealed class StageFlow
    {
        public StageFlow(int stageCount)
        {
            if (stageCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stageCount), stageCount, "Stage count must be at least 1.");
            }

            StageCount = stageCount;
            CurrentIndex = 0;
        }

        public int StageCount { get; }

        public int CurrentIndex { get; private set; }

        public bool Navigate(StageNavigation navigation)
        {
            switch (navigation)
            {
                case StageNavigation.None:
                    return false;

                case StageNavigation.Next:
                    if (CurrentIndex + 1 >= StageCount)
                    {
                        return false;
                    }

                    CurrentIndex++;
                    return true;

                case StageNavigation.Previous:
                    if (CurrentIndex <= 0)
                    {
                        return false;
                    }

                    CurrentIndex--;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(navigation), navigation, "The stage navigation value is not defined.");
            }
        }
    }
}
