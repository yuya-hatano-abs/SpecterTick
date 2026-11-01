using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;
using MoveDirection = SpecterTick.Core.MoveDirection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    [TestFixture]
    public sealed class BoardScreenInsetsTests
    {
        [Test]
        public void SI1_CanvasScale_MatchesScaleWithScreenSize()
        {
            Assert.That(BoardScreenInsets.CanvasScale(1280, 720), Is.EqualTo(1.0f).Within(0.00001f));
            Assert.That(BoardScreenInsets.CanvasScale(1920, 1080), Is.EqualTo(1.5f).Within(0.00001f));
            Assert.That(BoardScreenInsets.CanvasScale(2560, 720), Is.EqualTo(1.41421f).Within(0.00001f));
            Assert.That(BoardScreenInsets.CanvasScale(640, 360), Is.EqualTo(0.5f).Within(0.00001f));
        }

        [Test]
        public void SI2_Compute_Reference()
        {
            var insets = BoardScreenInsets.Compute(1280, 720);
            Assert.That(insets.Top, Is.EqualTo(0.286111f).Within(0.00001f));
            Assert.That(insets.Bottom, Is.EqualTo(0.144444f).Within(0.00001f));
        }

        [Test]
        public void SI3_Compute_ScalesWithResolution()
        {
            var hd = BoardScreenInsets.Compute(1920, 1080);
            Assert.That(hd.Top, Is.EqualTo(0.286111f).Within(0.00001f));
            Assert.That(hd.Bottom, Is.EqualTo(0.144444f).Within(0.00001f));

            var square = BoardScreenInsets.Compute(1000, 1000);
            Assert.That(BoardScreenInsets.CanvasScale(1000, 1000), Is.EqualTo(1.041667f).Within(0.00001f));
            Assert.That(square.Top, Is.EqualTo(0.214583f).Within(0.00001f));
            Assert.That(square.Bottom, Is.EqualTo(0.108333f).Within(0.00001f));
        }

        [Test]
        public void SI4_Compute_ClampsTotalTo06()
        {
            Assert.That(BoardScreenInsets.CanvasScale(1280, 300), Is.EqualTo(0.645497f).Within(0.00001f));
            var insets = BoardScreenInsets.Compute(1280, 300);
            Assert.That(insets.Top, Is.EqualTo(0.3987f).Within(0.0001f));
            Assert.That(insets.Bottom, Is.EqualTo(0.2013f).Within(0.0001f));
            Assert.That(insets.Top + insets.Bottom, Is.EqualTo(0.6f).Within(0.00001f));

            var reference = BoardScreenInsets.Compute(1280, 720);
            Assert.That(reference.Top + reference.Bottom, Is.LessThan(0.6f));
        }

        [Test]
        public void SI5_Compute_InvalidSize_Throws()
        {
            var zeroWidth = Assert.Throws<ArgumentOutOfRangeException>(() => BoardScreenInsets.Compute(0, 720));
            Assert.That(zeroWidth.ParamName, Is.EqualTo("screenWidth"));

            var zeroHeight = Assert.Throws<ArgumentOutOfRangeException>(() => BoardScreenInsets.Compute(1280, 0));
            Assert.That(zeroHeight.ParamName, Is.EqualTo("screenHeight"));

            var negativeWidth = Assert.Throws<ArgumentOutOfRangeException>(() => BoardScreenInsets.Compute(-1, 720));
            Assert.That(negativeWidth.ParamName, Is.EqualTo("screenWidth"));

            var negativeHeight = Assert.Throws<ArgumentOutOfRangeException>(() => BoardScreenInsets.Compute(1280, -1));
            Assert.That(negativeHeight.ParamName, Is.EqualTo("screenHeight"));
        }

        [Test]
        public void SI6_TopReference_MatchesHudPanel()
        {
            var root = new GameObject("BoardScreenInsetsTests_Hud");
            try
            {
                var hudView = root.AddComponent<HudView>();
                hudView.Apply(HudModelBuilder.Build(GameMode.Recording, 0, 0, new MoveDirection[0]));

                var images = root.GetComponentsInChildren<Image>(true);
                Assert.That(images.Length, Is.EqualTo(1));
                var panel = images[0].rectTransform;
                var topExtent = panel.sizeDelta.y + Math.Abs(panel.anchoredPosition.y);
                Assert.That(topExtent, Is.EqualTo(206f).Within(0.0001f));
                Assert.That(BoardScreenInsets.TopBandReferencePixels, Is.GreaterThanOrEqualTo(topExtent - 0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SI7_BottomReference_MatchesClearPanel()
        {
            var root = new GameObject("BoardScreenInsetsTests_Clear");
            try
            {
                var clearView = root.AddComponent<ClearScreenView>();
                clearView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));

                var images = root.GetComponentsInChildren<Image>(true);
                Assert.That(images.Length, Is.EqualTo(1));
                var panel = images[0].rectTransform;
                var bottomExtent = panel.sizeDelta.y + panel.anchoredPosition.y;
                Assert.That(bottomExtent, Is.EqualTo(104f).Within(0.0001f));
                Assert.That(BoardScreenInsets.BottomBandReferencePixels, Is.EqualTo(bottomExtent).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SI9_CanvasScalerConstants_MatchBoardScreenInsetsReference()
        {
            AssertCanvasScalerMatchesReference(CreateHudScalerRoot());
            AssertCanvasScalerMatchesReference(CreateClearScalerRoot());
            AssertCanvasScalerMatchesReference(CreateStageLabelScalerRoot());
        }

        private static void AssertCanvasScalerMatchesReference(GameObject root)
        {
            try
            {
                var scalers = root.GetComponentsInChildren<CanvasScaler>(true);
                Assert.That(scalers.Length, Is.EqualTo(1));
                var scaler = scalers[0];
                Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
                Assert.That(scaler.referenceResolution.x, Is.EqualTo(1280f));
                Assert.That(scaler.referenceResolution.y, Is.EqualTo(720f));
                Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(0.5f));
                Assert.That(scaler.referenceResolution.x, Is.EqualTo(BoardScreenInsets.ReferenceScreenWidth));
                Assert.That(scaler.referenceResolution.y, Is.EqualTo(BoardScreenInsets.ReferenceScreenHeight));
                Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(BoardScreenInsets.MatchWidthOrHeight));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateHudScalerRoot()
        {
            var root = new GameObject("BoardScreenInsetsTests_SI9_Hud");
            var hudView = root.AddComponent<HudView>();
            hudView.Apply(HudModelBuilder.Build(GameMode.Recording, 0, 0, new MoveDirection[0]));
            return root;
        }

        private static GameObject CreateClearScalerRoot()
        {
            var root = new GameObject("BoardScreenInsetsTests_SI9_Clear");
            var clearView = root.AddComponent<ClearScreenView>();
            clearView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));
            return root;
        }

        private static GameObject CreateStageLabelScalerRoot()
        {
            var root = new GameObject("BoardScreenInsetsTests_SI9_Label");
            var labelView = root.AddComponent<StageLabelView>();
            labelView.Apply(StageLabelModelBuilder.Build(0, 5, "WALK"));
            return root;
        }

        [Test]
        public void SI8_TopReference_CoversStageLabel()
        {
            var root = new GameObject("BoardScreenInsetsTests_Label");
            try
            {
                var labelView = root.AddComponent<StageLabelView>();
                labelView.Apply(StageLabelModelBuilder.Build(0, 5, "WALK"));

                var images = root.GetComponentsInChildren<Image>(true);
                Assert.That(images.Length, Is.EqualTo(1));
                var panel = images[0].rectTransform;
                var topExtent = panel.sizeDelta.y + Math.Abs(panel.anchoredPosition.y);
                Assert.That(topExtent, Is.EqualTo(92f).Within(0.0001f));
                Assert.That(BoardScreenInsets.TopBandReferencePixels, Is.GreaterThanOrEqualTo(topExtent - 0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
