namespace SpecterTick.Presentation
{
    public static class StageNavigationResolver
    {
        public static StageNavigation Resolve(bool nextPressed, bool previousPressed)
        {
            if (nextPressed)
            {
                return StageNavigation.Next;
            }

            if (previousPressed)
            {
                return StageNavigation.Previous;
            }

            return StageNavigation.None;
        }
    }
}
