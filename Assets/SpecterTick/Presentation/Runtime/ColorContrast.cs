using System;

namespace SpecterTick.Presentation
{
    public static class ColorContrast
    {
        public static float RelativeLuminance(float r, float g, float b)
        {
            if (r < 0f || r > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(r));
            }

            if (g < 0f || g > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(g));
            }

            if (b < 0f || b > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(b));
            }

            return 0.2126f * Linearize(r) + 0.7152f * Linearize(g) + 0.0722f * Linearize(b);
        }

        public static float ContrastRatio(float luminanceA, float luminanceB)
        {
            if (luminanceA < 0f || luminanceA > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(luminanceA));
            }

            if (luminanceB < 0f || luminanceB > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(luminanceB));
            }

            var lighter = luminanceA >= luminanceB ? luminanceA : luminanceB;
            var darker = luminanceA >= luminanceB ? luminanceB : luminanceA;
            return (lighter + 0.05f) / (darker + 0.05f);
        }

        public static float Blend(float foreground, float alpha, float background)
        {
            if (alpha < 0f || alpha > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(alpha));
            }

            return alpha * foreground + (1f - alpha) * background;
        }

        private static float Linearize(float component)
        {
            if (component <= 0.03928f)
            {
                return component / 12.92f;
            }

            return (float)Math.Pow((component + 0.055f) / 1.055f, 2.4f);
        }
    }
}
