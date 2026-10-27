using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;
using MoveDirection = SpecterTick.Core.MoveDirection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies stage-label Text strings, object counts, and panel dimensions only —
    /// not on-screen appearance, text fitting, or overlap with board/HUD/clear screen.
    /// </summary>
    [TestFixture]
    public sealed class StageLabelViewTests
    {
        private const string LabelWalk = "STAGE 1 / 5  WALK\nPREV: P  NEXT: N";
        private const string LabelEcho = "STAGE 2 / 5  ECHO\nPREV: P  NEXT: N";
        private const string LabelTangle = "STAGE 5 / 5  TANGLE\nPREV: P  NEXT: N";

        private GameObject _root;
        private StageLabelView _stageLabelView;

        [TearDown]
        public void TearDown()
        {
            if (_stageLabelView != null)
            {
                _stageLabelView.ReleaseResources();
            }

            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            _stageLabelView = null;
            _root = null;
        }

        [Test]
        public void LV1_Apply_BuildsOverlay()
        {
            var eventSystemsBefore = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;

            CreateStageLabelView();
            _stageLabelView.Apply(StageLabelModelBuilder.Build(0, 5, "WALK"));

            var canvases = _root.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases.Length, Is.EqualTo(1));
            Assert.That(canvases[0].renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
            Assert.That(canvases[0].sortingOrder, Is.EqualTo(95));
            Assert.That(canvases[0].gameObject.name, Is.EqualTo("StageLabelCanvas"));

            var scalers = _root.GetComponentsInChildren<CanvasScaler>(true);
            Assert.That(scalers.Length, Is.EqualTo(1));
            Assert.That(scalers[0].uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scalers[0].referenceResolution.x, Is.EqualTo(1280f));
            Assert.That(scalers[0].referenceResolution.y, Is.EqualTo(720f));
            Assert.That(scalers[0].matchWidthOrHeight, Is.EqualTo(0.5f));

            var images = _root.GetComponentsInChildren<Image>(true);
            Assert.That(images.Length, Is.EqualTo(1));
            var panelRect = images[0].rectTransform;
            Assert.That(panelRect.sizeDelta.x, Is.EqualTo(320f));
            Assert.That(panelRect.sizeDelta.y, Is.EqualTo(76f));
            Assert.That(panelRect.anchorMin, Is.EqualTo(new Vector2(1f, 1f)));
            Assert.That(panelRect.anchorMax, Is.EqualTo(new Vector2(1f, 1f)));
            Assert.That(panelRect.pivot, Is.EqualTo(new Vector2(1f, 1f)));
            Assert.That(panelRect.anchoredPosition, Is.EqualTo(new Vector2(-16f, -16f)));

            var raycasters = _root.GetComponentsInChildren<GraphicRaycaster>(true);
            Assert.That(raycasters.Length, Is.EqualTo(0));

            var eventSystemsAfter = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            Assert.That(eventSystemsAfter, Is.EqualTo(eventSystemsBefore));

            var texts = _root.GetComponentsInChildren<Text>(true);
            Assert.That(texts.Length, Is.EqualTo(1));
            Assert.That(texts[0].alignment, Is.EqualTo(TextAnchor.MiddleLeft));
            Assert.That(texts[0].text, Is.EqualTo(LabelWalk));
        }

        [Test]
        public void LV2_ApplyTwice_UpdatesWithoutDuplicating()
        {
            CreateStageLabelView();
            _stageLabelView.Apply(StageLabelModelBuilder.Build(0, 5, "WALK"));
            Assert.That(_root.GetComponentsInChildren<Text>(true)[0].text, Is.EqualTo(LabelWalk));

            _stageLabelView.Apply(StageLabelModelBuilder.Build(4, 5, "TANGLE"));
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true)[0].text, Is.EqualTo(LabelTangle));
        }

        [Test]
        public void LV3_Release_Destroys_ThenApplyRebuilds()
        {
            CreateStageLabelView();
            _stageLabelView.Apply(StageLabelModelBuilder.Build(1, 5, "ECHO"));

            var canvasBefore = _root.GetComponentsInChildren<Canvas>(true)[0];
            var textBefore = _root.GetComponentsInChildren<Text>(true)[0];

            _stageLabelView.ReleaseResources();
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(0));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(0));
            Assert.That(canvasBefore == null, Is.True);
            Assert.That(textBefore == null, Is.True);

            _stageLabelView.Apply(StageLabelModelBuilder.Build(1, 5, "ECHO"));
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true)[0].text, Is.EqualTo(LabelEcho));
        }

        [Test]
        public void LV4_Apply_Null_Throws()
        {
            CreateStageLabelView();
            var ex = Assert.Throws<ArgumentNullException>(() => _stageLabelView.Apply(null));
            Assert.That(ex.ParamName, Is.EqualTo("model"));
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(0));
        }

        [Test]
        public void LV5_CoexistsWithHudAndClearScreen()
        {
            _root = new GameObject("Root");
            var hudView = _root.AddComponent<HudView>();
            var clearScreenView = _root.AddComponent<ClearScreenView>();
            _stageLabelView = _root.AddComponent<StageLabelView>();

            hudView.Apply(HudModelBuilder.Build(GameMode.Recording, 0, 0, new MoveDirection[0]));
            clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));
            _stageLabelView.Apply(StageLabelModelBuilder.Build(0, 5, "WALK"));

            var canvases = _root.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases.Length, Is.EqualTo(3));
            Assert.That(canvases[0].gameObject.name, Is.EqualTo("HudCanvas"));
            Assert.That(canvases[1].gameObject.name, Is.EqualTo("ClearScreenCanvas"));
            Assert.That(canvases[2].gameObject.name, Is.EqualTo("StageLabelCanvas"));

            var hudText = _root.GetComponentsInChildren<Text>(true)[0];
            var clearText = _root.GetComponentsInChildren<Text>(true)[1];

            _stageLabelView.ReleaseResources();
            canvases = _root.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases.Length, Is.EqualTo(2));
            Assert.That(canvases[0].gameObject.name, Is.EqualTo("HudCanvas"));
            Assert.That(canvases[1].gameObject.name, Is.EqualTo("ClearScreenCanvas"));
            Assert.That(hudText == null, Is.False);
            Assert.That(clearText == null, Is.False);

            _stageLabelView.Apply(StageLabelModelBuilder.Build(0, 5, "WALK"));
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(3));

            clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Recording, 0, 0));
            canvases = _root.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases.Length, Is.EqualTo(2));
            Assert.That(canvases[0].gameObject.name, Is.EqualTo("HudCanvas"));
            Assert.That(canvases[1].gameObject.name, Is.EqualTo("StageLabelCanvas"));
        }

        private void CreateStageLabelView()
        {
            _root = new GameObject("Root");
            _stageLabelView = _root.AddComponent<StageLabelView>();
        }
    }
}
