namespace SpecterTick.Presentation
{
    public enum HowToPlayAction
    {
        None = 0,
        Opened = 1,
        Closed = 2
    }

    public sealed class HowToPlayState
    {
        public bool IsOpen { get; private set; }

        public void Open()
        {
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public HowToPlayAction Update(bool helpPressed, bool anyKeyPressed)
        {
            if (IsOpen)
            {
                if (anyKeyPressed)
                {
                    Close();
                    return HowToPlayAction.Closed;
                }

                return HowToPlayAction.None;
            }

            if (helpPressed)
            {
                Open();
                return HowToPlayAction.Opened;
            }

            return HowToPlayAction.None;
        }
    }
}
