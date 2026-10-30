using System;
using System.IO;
using NUnit.Framework;
using SpecterTick.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Reads PNG files directly to verify size, transparency, subject bounds, luminance, and state-pair fill differences.
    /// Does not verify art-style unity, silhouette readability, or board harmony (manual review).
    /// Thresholds follow Task 16b §3.3 (Grok_image_prompts.md unavailable; prompt values are authoritative).
    /// </summary>
    [TestFixture]
    public sealed class SkinAssetQualityTests
    {
        private const int ExpectedSize = 256;
        private const int PixelCount = ExpectedSize * ExpectedSize;
        private const float SubjectAlphaThreshold = 10f;
        private const float OpaqueAlphaThreshold = 200f;
        private const int MinOpaquePixels = 1000;
        private const float CenterTolerancePixels = 8f;
        private const float MaxFloorLuminance = 0.08f;
        private const float MinContrastVsFloor = 3f;
        private const float MinPlayerSpecterContrast = 1.5f;
        private const float MinStateOpaqueDelta = 0.05f;

        private Texture2D _loadedTexture;

        [TearDown]
        public void TearDown()
        {
            if (_loadedTexture != null)
            {
                Object.DestroyImmediate(_loadedTexture);
                _loadedTexture = null;
            }
        }

        [Test]
        public void AS1_Files_Exist_And_256x256()
        {
            foreach (var name in BoardSkin.SpriteNames)
            {
                var path = GetSkinPath(name);
                Assert.That(File.Exists(path), Is.True);
                var texture = LoadPng(path);
                Assert.That(texture.width, Is.EqualTo(ExpectedSize));
                Assert.That(texture.height, Is.EqualTo(ExpectedSize));
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void AS2_Alpha_Tiles_Opaque_Others_TransparentCorners()
        {
            Assert.That(GetMinimumAlpha("Floor"), Is.EqualTo(255));
            Assert.That(GetMinimumAlpha("Wall"), Is.EqualTo(255));

            foreach (var name in GetTransparentSubjectNames())
            {
                var pixels = LoadPixels(name);
                AssertCornerAlphaZero(pixels);
                Assert.That(CountAlphaAbove(pixels, OpaqueAlphaThreshold), Is.GreaterThanOrEqualTo(MinOpaquePixels));
            }
        }

        [Test]
        public void AS3_SubjectSize_And_Center()
        {
            AssertSubjectSize("Player", 0.50f, 0.70f);
            AssertSubjectSize("Specter", 0.50f, 0.70f);
            AssertSubjectSize("ExitClosed", 0.70f, 0.90f);
            AssertSubjectSize("ExitOpen", 0.70f, 0.90f);
            AssertSubjectSize("SwitchOff", 0.60f, 0.80f);
            AssertSubjectSize("SwitchOn", 0.60f, 0.80f);
            AssertSubjectSize("DoorClosed", 0.80f, 0.97f);
            AssertSubjectSize("DoorOpen", 0.80f, 0.97f);

            foreach (var name in GetTransparentSubjectNames())
            {
                var bounds = GetSubjectBounds(LoadPixels(name), SubjectAlphaThreshold);
                Assert.That(Mathf.Abs(bounds.CenterX - 128f), Is.LessThanOrEqualTo(CenterTolerancePixels));
                Assert.That(Mathf.Abs(bounds.CenterY - 128f), Is.LessThanOrEqualTo(CenterTolerancePixels));
            }
        }

        [Test]
        public void AS4_Luminance_Vs_Floor()
        {
            var floorPixels = LoadPixels("Floor");
            var floorLuminance = AverageLuminanceAllPixels(floorPixels);
            Assert.That(floorLuminance, Is.LessThanOrEqualTo(MaxFloorLuminance));

            AssertContrastVsFloor("Player", floorLuminance);
            AssertContrastVsFloor("Specter", floorLuminance);
            AssertContrastVsFloor("Wall", floorLuminance);
            AssertContrastVsFloor("ExitClosed", floorLuminance);
            AssertContrastVsFloor("ExitOpen", floorLuminance);
            AssertContrastVsFloor("SwitchOff", floorLuminance);
            AssertContrastVsFloor("SwitchOn", floorLuminance);
            AssertContrastVsFloor("DoorClosed", floorLuminance);
            AssertContrastVsFloor("DoorOpen", floorLuminance);
        }

        [Test]
        public void AS5_Player_Brighter_Than_Specter()
        {
            var playerLum = AverageLuminanceOpaque(LoadPixels("Player"), OpaqueAlphaThreshold);
            var specterLum = AverageLuminanceOpaque(LoadPixels("Specter"), OpaqueAlphaThreshold);
            Assert.That(playerLum, Is.GreaterThan(specterLum));
            var ratio = ColorContrast.ContrastRatio(playerLum, specterLum);
            Assert.That(ratio, Is.GreaterThanOrEqualTo(MinPlayerSpecterContrast));
        }

        [Test]
        public void AS6_StatePairs_DifferInFill()
        {
            AssertOpaqueDelta("ExitOpen", "ExitClosed", MinStateOpaqueDelta);
            AssertOpaqueDelta("SwitchOn", "SwitchOff", MinStateOpaqueDelta);
            AssertOpaqueDelta("DoorClosed", "DoorOpen", MinStateOpaqueDelta);
        }

        private static string[] GetTransparentSubjectNames()
        {
            return new[]
            {
                "Player",
                "Specter",
                "ExitClosed",
                "ExitOpen",
                "SwitchOff",
                "SwitchOn",
                "DoorClosed",
                "DoorOpen"
            };
        }

        private void AssertSubjectSize(string name, float minRatio, float maxRatio)
        {
            var bounds = GetSubjectBounds(LoadPixels(name), SubjectAlphaThreshold);
            var ratio = bounds.MaxExtent / ExpectedSize;
            Assert.That(ratio, Is.GreaterThanOrEqualTo(minRatio));
            Assert.That(ratio, Is.LessThanOrEqualTo(maxRatio));
        }

        private void AssertContrastVsFloor(string name, float floorLuminance)
        {
            var luminance = AverageLuminanceOpaque(LoadPixels(name), OpaqueAlphaThreshold);
            var ratio = ColorContrast.ContrastRatio(luminance, floorLuminance);
            Assert.That(ratio, Is.GreaterThanOrEqualTo(MinContrastVsFloor));
        }

        private void AssertOpaqueDelta(string higherName, string lowerName, float minDelta)
        {
            var higherRate = OpaqueRate(LoadPixels(higherName), OpaqueAlphaThreshold);
            var lowerRate = OpaqueRate(LoadPixels(lowerName), OpaqueAlphaThreshold);
            Assert.That(higherRate - lowerRate, Is.GreaterThanOrEqualTo(minDelta));
        }

        private static string GetSkinPath(string name)
        {
            return Path.Combine(Application.dataPath, "SpecterTick/Presentation/Resources/Skin", name + ".png");
        }

        private Color32[] LoadPixels(string name)
        {
            var texture = LoadPng(GetSkinPath(name));
            return texture.GetPixels32();
        }

        private Texture2D LoadPng(string path)
        {
            if (_loadedTexture != null)
            {
                Object.DestroyImmediate(_loadedTexture);
            }

            var bytes = File.ReadAllBytes(path);
            _loadedTexture = new Texture2D(2, 2);
            var loaded = ImageConversion.LoadImage(_loadedTexture, bytes);
            Assert.That(loaded, Is.True);
            return _loadedTexture;
        }

        private static byte GetMinimumAlpha(string name)
        {
            var path = GetSkinPath(name);
            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            ImageConversion.LoadImage(texture, bytes);
            var pixels = texture.GetPixels32();
            byte min = 255;
            for (var i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a < min)
                {
                    min = pixels[i].a;
                }
            }

            Object.DestroyImmediate(texture);
            return min;
        }

        private static void AssertCornerAlphaZero(Color32[] pixels)
        {
            Assert.That(pixels[0].a, Is.EqualTo(0));
            Assert.That(pixels[ExpectedSize - 1].a, Is.EqualTo(0));
            Assert.That(pixels[(ExpectedSize - 1) * ExpectedSize].a, Is.EqualTo(0));
            Assert.That(pixels[PixelCount - 1].a, Is.EqualTo(0));
        }

        private static int CountAlphaAbove(Color32[] pixels, float threshold)
        {
            var count = 0;
            for (var i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > threshold)
                {
                    count++;
                }
            }

            return count;
        }

        private static float OpaqueRate(Color32[] pixels, float threshold)
        {
            return CountAlphaAbove(pixels, threshold) / (float)PixelCount;
        }

        private static float AverageLuminanceAllPixels(Color32[] pixels)
        {
            var sum = 0f;
            for (var i = 0; i < pixels.Length; i++)
            {
                sum += PixelLuminance(pixels[i]);
            }

            return sum / pixels.Length;
        }

        private static float AverageLuminanceOpaque(Color32[] pixels, float alphaThreshold)
        {
            var sum = 0f;
            var count = 0;
            for (var i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > alphaThreshold)
                {
                    sum += PixelLuminance(pixels[i]);
                    count++;
                }
            }

            Assert.That(count, Is.GreaterThan(0));
            return sum / count;
        }

        private static float PixelLuminance(Color32 pixel)
        {
            return ColorContrast.RelativeLuminance(pixel.r / 255f, pixel.g / 255f, pixel.b / 255f);
        }

        private struct SubjectBounds
        {
            public float CenterX;
            public float CenterY;
            public float MaxExtent;
        }

        private static SubjectBounds GetSubjectBounds(Color32[] pixels, float alphaThreshold)
        {
            var minX = ExpectedSize;
            var maxX = -1;
            var minY = ExpectedSize;
            var maxY = -1;
            for (var y = 0; y < ExpectedSize; y++)
            {
                for (var x = 0; x < ExpectedSize; x++)
                {
                    if (pixels[y * ExpectedSize + x].a > alphaThreshold)
                    {
                        if (x < minX)
                        {
                            minX = x;
                        }

                        if (x > maxX)
                        {
                            maxX = x;
                        }

                        if (y < minY)
                        {
                            minY = y;
                        }

                        if (y > maxY)
                        {
                            maxY = y;
                        }
                    }
                }
            }

            var width = maxX - minX + 1;
            var height = maxY - minY + 1;
            return new SubjectBounds
            {
                CenterX = (minX + maxX) / 2f,
                CenterY = (minY + maxY) / 2f,
                MaxExtent = width > height ? width : height
            };
        }
    }
}
