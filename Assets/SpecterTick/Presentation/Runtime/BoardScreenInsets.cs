using System;

namespace SpecterTick.Presentation
{
    public readonly struct BoardScreenInsets : IEquatable<BoardScreenInsets>
    {
        public const int ReferenceScreenWidth = 1280;
        public const int ReferenceScreenHeight = 720;
        public const float MatchWidthOrHeight = 0.5f;
        public const float TopBandReferencePixels = 206f;
        public const float BottomBandReferencePixels = 104f;
        public const float MaxInsetFractionTotal = 0.6f;
        public const float VerticalPadding = 0.5f;

        public BoardScreenInsets(float top, float bottom)
        {
            Top = top;
            Bottom = bottom;
        }

        public float Top { get; }

        public float Bottom { get; }

        public static float CanvasScale(int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(screenWidth));
            }

            if (screenHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(screenHeight));
            }

            var logWidth = Math.Log(screenWidth / (double)ReferenceScreenWidth, 2d);
            var logHeight = Math.Log(screenHeight / (double)ReferenceScreenHeight, 2d);
            var logWeightedAverage = ((1d - MatchWidthOrHeight) * logWidth)
                + (MatchWidthOrHeight * logHeight);
            return (float)Math.Pow(2d, logWeightedAverage);
        }

        public static BoardScreenInsets Compute(int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(screenWidth));
            }

            if (screenHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(screenHeight));
            }

            var scale = CanvasScale(screenWidth, screenHeight);
            var top = TopBandReferencePixels * scale / screenHeight;
            var bottom = BottomBandReferencePixels * scale / screenHeight;
            var total = top + bottom;
            if (total > MaxInsetFractionTotal)
            {
                var factor = MaxInsetFractionTotal / total;
                top *= factor;
                bottom *= factor;
            }

            return new BoardScreenInsets(top, bottom);
        }

        public bool Equals(BoardScreenInsets other)
        {
            return Top.Equals(other.Top) && Bottom.Equals(other.Bottom);
        }

        public override bool Equals(object obj)
        {
            return obj is BoardScreenInsets other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Top.GetHashCode() * 397) ^ Bottom.GetHashCode();
            }
        }
    }
}
