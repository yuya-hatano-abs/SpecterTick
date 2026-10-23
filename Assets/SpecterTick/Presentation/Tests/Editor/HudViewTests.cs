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
    /// Verifies HUD Text strings and created object counts only — not on-screen appearance or fonts.
    /// U3 validates EditMode DestroyImmediate rebuild only, not Play-mode delayed Destroy safety.
    /// </summary>
    [TestFixture]
    public sealed class HudViewTests
    {
        private GameObject _root;
        private HudView _hudView;

        [TearDown]
        public void TearDown()
        {
            if (_hudView != null)
            {
                _hudView.ReleaseResources();
            }

            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            _hudView = null;
            _root = null;
        }

        [Test]
        public void U1_Apply_BuildsOverlayCanvasAndWritesText()
        {
            var eventSystemsBefore = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;

            CreateHudView();
            _hudView.Apply(HudModelBuilder.Build(GameMode.Recording, 3, 0, new[]
            {
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right
            }));

            var canvases = _root.GetComponentsInChildren<Canvas>(true);
            Assert.That(canvases.Length, Is.EqualTo(1));
            Assert.That(canvases[0].renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));

            var scalers = _root.GetComponentsInChildren<CanvasScaler>(true);
            Assert.That(scalers.Length, Is.EqualTo(1));
            Assert.That(scalers[0].uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scalers[0].referenceResolution.x, Is.EqualTo(1280f));
            Assert.That(scalers[0].referenceResolution.y, Is.EqualTo(720f));
            Assert.That(scalers[0].matchWidthOrHeight, Is.EqualTo(0.5f));

            var images = _root.GetComponentsInChildren<Image>(true);
            Assert.That(images.Length, Is.EqualTo(1));

            var raycasters = _root.GetComponentsInChildren<GraphicRaycaster>(true);
            Assert.That(raycasters.Length, Is.EqualTo(0));

            var eventSystemsAfter = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            Assert.That(eventSystemsAfter, Is.EqualTo(eventSystemsBefore));

            var texts = _root.GetComponentsInChildren<Text>(true);
            Assert.That(texts.Length, Is.EqualTo(1));
            Assert.That(
                texts[0].text,
                Is.EqualTo("MODE: RECORDING\nTICK: 3\nRECORDED: 3\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: U R R"));
        }

        [Test]
        public void U2_Apply_Twice_UpdatesWithoutDuplicating()
        {
            CreateHudView();
            _hudView.Apply(HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>()));

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(1));

            _hudView.Apply(HudModelBuilder.Build(GameMode.Playback, 0, 0, new[]
            {
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Down,
                MoveDirection.Left
            }));

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(1));
            Assert.That(
                _root.GetComponentsInChildren<Text>(true)[0].text,
                Is.EqualTo("MODE: PLAYBACK\nTICK: 0\nPLAYBACK: 0 / 5\nSTEP: SPACE  RESET: R\nCOMMANDS: U R R D L"));
        }

        [Test]
        public void U3_ReleaseResources_RemovesCanvas_And_ApplyRebuilds()
        {
            CreateHudView();
            var model = HudModelBuilder.Build(GameMode.Playback, 2, 2, new[]
            {
                MoveDirection.Up,
                MoveDirection.Right,
                MoveDirection.Right,
                MoveDirection.Down,
                MoveDirection.Left
            });
            _hudView.Apply(model);

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            var canvasBefore = _root.GetComponentsInChildren<Canvas>(true)[0];
            var textBefore = _root.GetComponentsInChildren<Text>(true)[0];

            _hudView.ReleaseResources();

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(0));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(0));
            // ReleaseResources は、破棄の前に SetParent(null) で親から外す。子の検索だけでは、『外れた』と『破棄された』を区別できない。破棄の確認には、保持した参照が破棄済みであることを検査する。
            // UnityEngine.Object は NUnit の Is.Null では null と判定されないため、== null と Is.True を使う。
            Assert.That(canvasBefore == null, Is.True);
            Assert.That(textBefore == null, Is.True);

            _hudView.Apply(model);

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(1));
            Assert.That(
                _root.GetComponentsInChildren<Text>(true)[0].text,
                Is.EqualTo("MODE: PLAYBACK\nTICK: 2\nPLAYBACK: 2 / 5\nSTEP: SPACE  RESET: R\nCOMMANDS: U R R D L"));
        }

        private void CreateHudView()
        {
            _root = new GameObject("HudViewTestsRoot");
            _hudView = _root.AddComponent<HudView>();
        }
    }
}
