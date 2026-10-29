using System;

namespace SpecterTick.Presentation
{
    public sealed class ScreenSizeTracker
    {
        public ScreenSizeTracker(int width, int height)
        {
            if (width < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Width = width;
            Height = height;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool Update(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                return false;
            }

            if (width == Width && height == Height)
            {
                return false;
            }

            Width = width;
            Height = height;
            return true;
        }
    }
}
