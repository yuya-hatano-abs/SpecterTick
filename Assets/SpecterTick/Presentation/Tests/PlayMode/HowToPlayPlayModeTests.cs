using System.Collections;
using NUnit.Framework;
using SpecterTick.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SpecterTick.Presentation.PlayModeTests
{
    public sealed class HowToPlayPlayModeTests : InputTestFixture
    {
        private const float PanelScreenEdgeTolerancePixels = 0.5f;
        private const float DimScreenEdgeTolerancePixels = 1f;

        private Camera _camera;
        private GameObject _board;

        public override void Setup()
        {
            base.Setup();
            _camera = PlayModeHarness.CreateMainCamera();
            _board = null;
        }

        public override void TearDown()
        {
            if (_board != null)
            {
                Object.DestroyImmediate(_board);
                _board = null;
            }

            if (_camera != null)
            {
                Object.DestroyImmediate(_camera.gameObject);
                _camera = null;
            }

            base.TearDown();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PH1_StartWithHelp_ShowsOverlayAndHudRecording()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = CreateBoard(showHowToOnStart: true);
            yield return null;
            yield return null;

            Assert.That(IsHowToPlayVisible(_board), Is.True);
            Assert.That(HowToPlayText(_board), Does.Contain("HOW TO PLAY"));
            Assert.That(HudText(_board), Does.Contain("MODE: RECORDING"));
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PH2_AnyKeyClosesHelp_DoesNotPassMoveToGame()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = CreateBoard(showHowToOnStart: true);
            yield return null;
            yield return null;
            var keyboard = Keyboard.current;

            yield return Tap(keyboard.rightArrowKey);
            Assert.That(IsHowToPlayVisible(_board), Is.False);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 0"));

            yield return Tap(keyboard.rightArrowKey);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PH3_H_ReopensHelp_RightIgnoredUntilClosed()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = CreateBoard(showHowToOnStart: true);
            yield return null;
            yield return null;
            var keyboard = Keyboard.current;

            yield return Tap(keyboard.spaceKey);
            Assert.That(IsHowToPlayVisible(_board), Is.False);

            yield return Tap(keyboard.rightArrowKey);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));

            yield return Tap(keyboard.hKey);
            Assert.That(IsHowToPlayVisible(_board), Is.True);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));

            yield return Tap(keyboard.rightArrowKey);
            Assert.That(IsHowToPlayVisible(_board), Is.False);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));

            yield return Tap(keyboard.hKey);
            Assert.That(IsHowToPlayVisible(_board), Is.True);
            yield return Tap(keyboard.hKey);
            Assert.That(IsHowToPlayVisible(_board), Is.False);
            Assert.That(HudText(_board), Does.Contain("RECORDED: 1"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PH4_WhileOpen_N_DoesNotNavigate()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = CreateBoard(showHowToOnStart: true);
            yield return null;
            yield return null;
            var keyboard = Keyboard.current;

            yield return Tap(keyboard.nKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 1 / 5  WALK"));
            Assert.That(IsHowToPlayVisible(_board), Is.False);

            yield return Tap(keyboard.nKey);
            Assert.That(LabelText(_board), Does.Contain("STAGE 2 / 5  ECHO"));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PH5_HarnessDefault_DoesNotShowOnStart()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = PlayModeHarness.CreateBoard();
            yield return null;
            yield return null;

            Assert.That(IsHowToPlayVisible(_board), Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PH6_RealSkin_LegendSpriteImageCount()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = CreateBoard(showHowToOnStart: true);
            yield return null;
            yield return null;

            Assert.That(CountHowToPlayLegendSprites(_board), Is.EqualTo(9));
            LogAssert.NoUnexpectedReceived();
        }

        /// <summary>
        /// Panel bounds use HowToPlayPanel by path; the first Image on sortingOrder 200 canvas is HowToPlayDim, not the panel.
        /// </summary>
        [UnityTest]
        [Timeout(60000)]
        public IEnumerator PH7_PanelInsideScreen_DimCoversViewport()
        {
            InputSystem.AddDevice<Keyboard>();
            _board = CreateBoard(showHowToOnStart: true);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();

            Assert.That(TryGetHowToPlayPanelScreenRect(_board, out var panelRect), Is.True);
            AssertPanelScreenEdgesInsideViewport(panelRect);

            var maxPanelWidth = Screen.width * HowToPlayView.PanelWidth / 1280f + 1f;
            var maxPanelHeight = Screen.height * HowToPlayView.PanelHeight / 720f + 1f;
            Assert.That(panelRect.Width, Is.LessThanOrEqualTo(maxPanelWidth));
            Assert.That(panelRect.Height, Is.LessThanOrEqualTo(maxPanelHeight));

            var dim = FindHowToPlayDimTransform(_board);
            Assert.That(dim, Is.Not.Null);
            var dimRect = GetScreenRectFromCorners(dim.GetComponent<RectTransform>());
            AssertDimCoversViewport(dimRect);
            LogAssert.NoUnexpectedReceived();
        }

        private static void AssertPanelScreenEdgesInsideViewport(ScreenAxisRect panelRect)
        {
            Assert.That(
                panelRect.Left,
                Is.GreaterThanOrEqualTo(-PanelScreenEdgeTolerancePixels),
                "HowToPlayPanel left edge (not HowToPlayDim)");
            Assert.That(panelRect.Bottom, Is.GreaterThanOrEqualTo(-PanelScreenEdgeTolerancePixels));
            Assert.That(panelRect.Right, Is.LessThanOrEqualTo(Screen.width + PanelScreenEdgeTolerancePixels));
            Assert.That(panelRect.Top, Is.LessThanOrEqualTo(Screen.height + PanelScreenEdgeTolerancePixels));
        }

        private static void AssertDimCoversViewport(ScreenAxisRect dimRect)
        {
            Assert.That(dimRect.Left, Is.LessThanOrEqualTo(DimScreenEdgeTolerancePixels));
            Assert.That(dimRect.Bottom, Is.LessThanOrEqualTo(DimScreenEdgeTolerancePixels));
            Assert.That(dimRect.Right, Is.GreaterThanOrEqualTo(Screen.width - DimScreenEdgeTolerancePixels));
            Assert.That(dimRect.Top, Is.GreaterThanOrEqualTo(Screen.height - DimScreenEdgeTolerancePixels));
        }

        private struct ScreenAxisRect
        {
            public float Left;
            public float Right;
            public float Bottom;
            public float Top;

            public float Width
            {
                get { return Right - Left; }
            }

            public float Height
            {
                get { return Top - Bottom; }
            }
        }

        private static bool TryGetHowToPlayPanelScreenRect(GameObject board, out ScreenAxisRect rect)
        {
            rect = default;
            if (board == null)
            {
                return false;
            }

            var panel = FindHowToPlayPanelTransform(board);
            if (panel == null)
            {
                return false;
            }

            var panelTransform = panel.GetComponent<RectTransform>();
            if (panelTransform == null)
            {
                return false;
            }

            Canvas.ForceUpdateCanvases();
            rect = GetScreenRectFromCorners(panelTransform);
            return true;
        }

        private static Transform FindHowToPlayPanelTransform(GameObject board)
        {
            var canvases = board.GetComponentsInChildren<Canvas>(true);
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (!canvas.isRootCanvas || canvas.sortingOrder != 200)
                {
                    continue;
                }

                var panel = canvas.transform.Find("HowToPlayPanel");
                if (panel != null)
                {
                    return panel;
                }
            }

            return board.transform.Find("HowToPlayCanvas/HowToPlayPanel");
        }

        private static Transform FindHowToPlayDimTransform(GameObject board)
        {
            var canvases = board.GetComponentsInChildren<Canvas>(true);
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (!canvas.isRootCanvas || canvas.sortingOrder != 200)
                {
                    continue;
                }

                var dim = canvas.transform.Find("HowToPlayDim");
                if (dim != null)
                {
                    return dim;
                }
            }

            return board.transform.Find("HowToPlayCanvas/HowToPlayDim");
        }

        private static ScreenAxisRect GetScreenRectFromCorners(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            var left = corners[0].x;
            var right = corners[0].x;
            var bottom = corners[0].y;
            var top = corners[0].y;
            for (var i = 1; i < corners.Length; i++)
            {
                left = Mathf.Min(left, corners[i].x);
                right = Mathf.Max(right, corners[i].x);
                bottom = Mathf.Min(bottom, corners[i].y);
                top = Mathf.Max(top, corners[i].y);
            }

            return new ScreenAxisRect
            {
                Left = left,
                Right = right,
                Bottom = bottom,
                Top = top
            };
        }

        private static GameObject CreateBoard(bool showHowToOnStart)
        {
            var board = new GameObject("Board");
            var driver = board.AddComponent<BoardSessionDriver>();
            driver.ShowHowToPlayOnStart = showHowToOnStart;
            return board;
        }

        private static bool IsHowToPlayVisible(GameObject board)
        {
            var canvas = FindHowToCanvas(board);
            return canvas != null && canvas.gameObject.activeInHierarchy;
        }

        private static string HowToPlayText(GameObject board)
        {
            return PlayModeHarness.CanvasText(board, 200);
        }

        private static string HudText(GameObject board)
        {
            return PlayModeHarness.CanvasText(board, 100);
        }

        private static string LabelText(GameObject board)
        {
            return PlayModeHarness.CanvasText(board, 95);
        }

        private static int CountHowToPlayLegendSprites(GameObject board)
        {
            var legend = board.transform.Find(
                "HowToPlayCanvas/HowToPlayPanel/HowToPlayContent/LegendRows");
            if (legend == null)
            {
                return 0;
            }

            var images = legend.GetComponentsInChildren<Image>(true);
            var count = 0;
            for (var i = 0; i < images.Length; i++)
            {
                if (images[i].sprite != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static Canvas FindHowToCanvas(GameObject board)
        {
            var canvases = board.GetComponentsInChildren<Canvas>(true);
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (canvas.isRootCanvas && canvas.sortingOrder == 200)
                {
                    return canvas;
                }
            }

            return null;
        }

        private IEnumerator Tap(KeyControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
            yield return null;
        }
    }
}
