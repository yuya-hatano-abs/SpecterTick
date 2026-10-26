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
    /// <summary>
    /// Verifies HUD Text strings and panel dimensions only — not on-screen appearance, fonts, or text fitting inside the panel.
    /// </summary>
    [TestFixture]
    public sealed class HudViewGoalTests
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
        public void HG1_Apply_SessionModel_SixLines()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var model = HudModelBuilder.Build(session);

            CreateHudView();
            _hudView.Apply(model);

            var texts = _root.GetComponentsInChildren<Text>(true);
            Assert.That(texts.Length, Is.EqualTo(1));
            Assert.That(
                texts[0].text,
                Is.EqualTo("MODE: RECORDING\nTICK: 0\nRECORDED: 0\nSWITCHES: 0 / 2  EXIT: CLOSED\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: -"));
            Assert.That(texts[0].text.Split('\n').Length, Is.EqualTo(6));
        }

        [Test]
        public void HG2_Panel_Dimensions()
        {
            CreateHudView();
            _hudView.Apply(HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>(), 0, 0, true));

            var images = _root.GetComponentsInChildren<Image>(true);
            Assert.That(images.Length, Is.EqualTo(1));
            var panelRect = images[0].rectTransform;
            Assert.That(panelRect.sizeDelta.x, Is.EqualTo(600f));
            Assert.That(panelRect.sizeDelta.y, Is.EqualTo(190f));
            Assert.That(panelRect.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(panelRect.anchorMax, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(panelRect.pivot, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(panelRect.anchoredPosition, Is.EqualTo(new Vector2(16f, -16f)));
        }

        [Test]
        public void HG3_Apply_SixLinesThenFiveLines_ReplacesText()
        {
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var sixLineModel = HudModelBuilder.Build(session);

            CreateHudView();
            _hudView.Apply(sixLineModel);

            const string sixLineText =
                "MODE: RECORDING\nTICK: 0\nRECORDED: 0\nSWITCHES: 0 / 2  EXIT: CLOSED\nMOVE: ARROWS/WASD  CONFIRM: ENTER  RESET: R\nCOMMANDS: -";
            Assert.That(_root.GetComponentsInChildren<Text>(true)[0].text, Is.EqualTo(sixLineText));

            var fiveLineModel = HudModelBuilder.Build(
                GameMode.Playback,
                2,
                2,
                new[]
                {
                    MoveDirection.Up,
                    MoveDirection.Right,
                    MoveDirection.Right,
                    MoveDirection.Down,
                    MoveDirection.Left
                });
            _hudView.Apply(fiveLineModel);

            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
            Assert.That(_root.GetComponentsInChildren<Text>(true).Length, Is.EqualTo(1));
            Assert.That(
                _root.GetComponentsInChildren<Text>(true)[0].text,
                Is.EqualTo("MODE: PLAYBACK\nTICK: 2\nPLAYBACK: 2 / 5\nSTEP: SPACE  RESET: R\nCOMMANDS: U R R D L"));
        }

        private void CreateHudView()
        {
            _root = new GameObject("HudViewGoalTestsRoot");
            _hudView = _root.AddComponent<HudView>();
        }
    }
}
