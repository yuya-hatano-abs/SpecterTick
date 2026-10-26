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
    /// Verifies clear-screen Text strings, object counts, and panel dimensions only —
    /// not on-screen appearance, fonts, text fitting, or overlap with board/HUD.
    /// </summary>
    [TestFixture]
    public sealed class ClearScreenViewTests
    {
        private const string Clr3 =
            "CLEARED!\nTICK: 4  RECORDED: 5\nPRESS R TO RETRY";

        private GameObject _root;
        private ClearScreenView _clearScreenView;

        [TearDown]
        public void TearDown()
        {
            if (_clearScreenView != null)
            {
                _clearScreenView.ReleaseResources();
            }

            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            _clearScreenView = null;
            _root = null;
        }

        [Test]
        public void CV1_Apply_Visible_BuildsOverlay()
        {
            var eventSystemsBefore = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;

            CreateClearScreenView();
            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));

            var canvases = _root.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases.Length, Is.EqualTo(1));
            Assert.That(canvases[0].renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
            Assert.That(canvases[0].sortingOrder, Is.EqualTo(110));
            Assert.That(canvases[0].gameObject.name, Is.EqualTo("ClearScreenCanvas"));

            var scalers = _root.GetComponentsInChildren<CanvasScaler>(true);
            Assert.That(scalers.Length, Is.EqualTo(1));
            Assert.That(scalers[0].uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scalers[0].referenceResolution.x, Is.EqualTo(1280f));
            Assert.That(scalers[0].referenceResolution.y, Is.EqualTo(720f));
            Assert.That(scalers[0].matchWidthOrHeight, Is.EqualTo(0.5f));

            var images = _root.GetComponentsInChildren<Image>(true);
            Assert.That(images.Length, Is.EqualTo(1));
            var panelRect = images[0].rectTransform;
            Assert.That(panelRect.sizeDelta.x, Is.EqualTo(520f));
            Assert.That(panelRect.sizeDelta.y, Is.EqualTo(96f));
            Assert.That(panelRect.anchorMin, Is.EqualTo(new Vector2(0.5f, 0f)));
            Assert.That(panelRect.anchorMax, Is.EqualTo(new Vector2(0.5f, 0f)));
            Assert.That(panelRect.pivot, Is.EqualTo(new Vector2(0.5f, 0f)));
            Assert.That(panelRect.anchoredPosition, Is.EqualTo(new Vector2(0f, 8f)));

            var raycasters = _root.GetComponentsInChildren<GraphicRaycaster>(true);
            Assert.That(raycasters.Length, Is.EqualTo(0));

            var eventSystemsAfter = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            Assert.That(eventSystemsAfter, Is.EqualTo(eventSystemsBefore));

            var texts = _root.GetComponentsInChildren<Text>(true);
            Assert.That(texts.Length, Is.EqualTo(1));
            Assert.That(texts[0].alignment, Is.EqualTo(TextAnchor.MiddleCenter));
            Assert.That(texts[0].text, Is.EqualTo(Clr3));
        }

        [Test]
        public void CV2_Apply_Hidden_BuildsNothing()
        {
            CreateClearScreenView();
            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Recording, 0, 0));

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(0));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(0));
            Assert.That(_root.GetComponentsInChildren<Image>(true).Length, Is.EqualTo(0));
        }

        [Test]
        public void CV3_VisibleThenHidden_Destroys_ThenVisibleRebuilds()
        {
            CreateClearScreenView();
            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));

            var canvasBefore = _root.GetComponentsInChildren<Canvas>(true)[0];
            var textBefore = _root.GetComponentsInChildren<Text>(true)[0];

            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Playback, 2, 5));

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(0));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(0));
            Assert.That(canvasBefore == null, Is.True);
            Assert.That(textBefore == null, Is.True);

            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true)[0].text, Is.EqualTo(Clr3));
        }

        [Test]
        public void CV4_ApplyTwice_UpdatesWithoutDuplicating()
        {
            CreateClearScreenView();
            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));
            Assert.That(_root.GetComponentsInChildren<Text>(true)[0].text, Is.EqualTo(Clr3));

            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 0, 7));

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(1));
            Assert.That(
                _root.GetComponentsInChildren<Text>(true)[0].text,
                Is.EqualTo("CLEARED!\nTICK: 0  RECORDED: 7\nPRESS R TO RETRY"));
        }

        [Test]
        public void CV5_Apply_Null_Throws()
        {
            CreateClearScreenView();
            var ex = Assert.Throws<ArgumentNullException>(() => _clearScreenView.Apply(null));
            Assert.That(ex.ParamName, Is.EqualTo("model"));
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(0));
        }

        [Test]
        public void CV6_CoexistsWithHudView()
        {
            _root = new GameObject("ClearScreenViewTestsRoot");
            var hudView = _root.AddComponent<HudView>();
            _clearScreenView = _root.AddComponent<ClearScreenView>();

            hudView.Apply(HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>()));
            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(2));

            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Recording, 0, 0));

            var canvases = _root.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases.Length, Is.EqualTo(1));
            Assert.That(canvases[0].gameObject.name, Is.EqualTo("HudCanvas"));

            var hudText = _root.GetComponentsInChildren<Text>(true)[0];
            Assert.That(hudText == null, Is.False);

            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(2));

            var clearText = _root.GetComponentsInChildren<Text>(true);
            Text clearTextRef = null;
            for (var i = 0; i < clearText.Length; i++)
            {
                if (clearText[i].text == Clr3)
                {
                    clearTextRef = clearText[i];
                    break;
                }
            }

            Assert.That(clearTextRef == null, Is.False);

            hudView.ReleaseResources();

            canvases = _root.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases.Length, Is.EqualTo(1));
            Assert.That(canvases[0].gameObject.name, Is.EqualTo("ClearScreenCanvas"));
            Assert.That(clearTextRef == null, Is.False);
        }

        [Test]
        public void CV7_BoardViewShow_KeepsClearCanvas()
        {
            _root = new GameObject("ClearScreenViewTestsRoot");
            var boardView = _root.AddComponent<BoardView>();
            _clearScreenView = _root.AddComponent<ClearScreenView>();

            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));
            var canvasBefore = _root.GetComponentsInChildren<Canvas>(true)[0];

            boardView.Show(SampleStageFactory.Create());

            Assert.That(canvasBefore == null, Is.False);
            Assert.That(_root.transform.Find("ClearScreenCanvas") != null, Is.True);
            Assert.That(_root.transform.childCount, Is.EqualTo(5));

            boardView.ReleaseResources();

            Assert.That(canvasBefore == null, Is.False);
            Assert.That(_root.transform.childCount, Is.EqualTo(1));
        }

        private void CreateClearScreenView()
        {
            _root = new GameObject("ClearScreenViewTestsRoot");
            _clearScreenView = _root.AddComponent<ClearScreenView>();
        }
    }
}
