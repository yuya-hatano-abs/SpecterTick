using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies BoardView.FitCamera with screen insets. Does not guarantee built-player appearance.
    /// </summary>
    [TestFixture]
    public sealed class BoardViewFitCameraInsetsTests
    {
        private GameObject _cameraObject;
        private Camera _camera;
        private RenderTexture _renderTexture;
        private GameObject _boardRoot;
        private BoardView _boardView;

        [TearDown]
        public void TearDown()
        {
            if (_boardView != null)
            {
                _boardView.ReleaseResources();
            }

            if (_boardRoot != null)
            {
                Object.DestroyImmediate(_boardRoot);
            }

            if (_camera != null)
            {
                _camera.targetTexture = null;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Object.DestroyImmediate(_renderTexture);
            }

            if (_cameraObject != null)
            {
                Object.DestroyImmediate(_cameraObject);
            }

            _boardView = null;
            _boardRoot = null;
            _camera = null;
            _cameraObject = null;
            _renderTexture = null;
        }

        [Test]
        public void FI1_ZeroInsets_MatchesLegacyPosition_AndUsesVerticalPaddingSize()
        {
            CreateCamera(16f / 9f);
            CreateBoardView();
            _boardView.Show(StageCatalog.CreateStage(0));

            _boardView.FitCamera(_camera);
            var legacyPosition = _camera.transform.position;
            var legacySize = _camera.orthographicSize;

            _boardView.FitCamera(_camera, 0f, 0f);
            Assert.That(_camera.transform.position.x, Is.EqualTo(legacyPosition.x).Within(0.0001f));
            Assert.That(_camera.transform.position.y, Is.EqualTo(legacyPosition.y).Within(0.0001f));
            Assert.That(_camera.transform.position.z, Is.EqualTo(legacyPosition.z).Within(0.0001f));
            Assert.That(_camera.orthographicSize, Is.EqualTo(1.75f).Within(0.0001f));
            Assert.That(legacySize, Is.EqualTo(2.5f).Within(0.0001f));

            DestroyCameraResources();
            CreateCamera(0.5f);
            _boardView.Show(StageCatalog.CreateStage(4));
            _boardView.FitCamera(_camera);
            legacySize = _camera.orthographicSize;
            _boardView.FitCamera(_camera, 0f, 0f);
            Assert.That(_camera.orthographicSize, Is.EqualTo(legacySize).Within(0.0001f));
            Assert.That(_camera.orthographicSize, Is.EqualTo(8f).Within(0.0001f));
        }

        private void DestroyCameraResources()
        {
            if (_camera != null)
            {
                _camera.targetTexture = null;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Object.DestroyImmediate(_renderTexture);
                _renderTexture = null;
            }

            if (_cameraObject != null)
            {
                Object.DestroyImmediate(_cameraObject);
                _cameraObject = null;
                _camera = null;
            }
        }

        [Test]
        public void FI2_Insets_BoardFitsBetweenBands()
        {
            CreateCamera(16f / 9f);
            CreateBoardView();
            _boardView.Show(StageCatalog.CreateStage(0));
            _boardView.FitCamera(_camera, 0.286111f, 0.144444f);

            Assert.That(_camera.orthographicSize, Is.EqualTo(3.0732f).Within(0.0001f));
            AssertTopBottomViewportForFloor(0.286111f, 0.144444f, true);
        }

        [Test]
        public void FI3_Insets_WideWindow_VerticalLimited()
        {
            CreateCamera(4f);
            CreateBoardView();
            _boardView.Show(StageCatalog.CreateStage(1));
            _boardView.FitCamera(_camera, 0.286111f, 0.144444f);

            Assert.That(_camera.orthographicSize, Is.EqualTo(3.0732f).Within(0.0001f));
            AssertTopBottomViewportForFloor(0.286111f, 0.144444f, true);
        }

        [Test]
        public void FI4_Insets_TallWindow_HorizontalLimited()
        {
            CreateCamera(0.5f);
            CreateBoardView();
            _boardView.Show(StageCatalog.CreateStage(4));
            _boardView.FitCamera(_camera, 0.286111f, 0.144444f);

            Assert.That(_camera.orthographicSize, Is.EqualTo(8f).Within(0.0001f));
            var floor = _boardRoot.transform.Find(BoardView.FloorContainerName);
            var left = _camera.WorldToViewportPoint(floor.GetChild(0).position);
            var right = _camera.WorldToViewportPoint(floor.GetChild(floor.childCount - 1).position);
            Assert.That(left.x, Is.GreaterThanOrEqualTo(-0.0001f));
            Assert.That(right.x, Is.LessThanOrEqualTo(1.0001f));

            var bandCenter = 0.144444f + (1f - 0.286111f - 0.144444f) / 2f;
            var boardCenterY = 0f;
            for (var i = 0; i < floor.childCount; i++)
            {
                boardCenterY += floor.GetChild(i).position.y;
            }

            boardCenterY /= floor.childCount;
            var viewportCenter = _camera.WorldToViewportPoint(new Vector3(2.5f, boardCenterY, 0f)).y;
            Assert.That(viewportCenter, Is.EqualTo(bandCenter).Within(0.0001f));
        }

        [Test]
        public void FI5_InvalidInsets_Throws()
        {
            CreateCamera(16f / 9f);
            CreateBoardView();
            _boardView.Show(StageCatalog.CreateStage(0));

            var negativeTop = Assert.Throws<ArgumentOutOfRangeException>(
                () => _boardView.FitCamera(_camera, -0.1f, 0f));
            Assert.That(negativeTop.ParamName, Is.EqualTo("topInset"));

            var topOne = Assert.Throws<ArgumentOutOfRangeException>(
                () => _boardView.FitCamera(_camera, 1f, 0f));
            Assert.That(topOne.ParamName, Is.EqualTo("topInset"));

            var negativeBottom = Assert.Throws<ArgumentOutOfRangeException>(
                () => _boardView.FitCamera(_camera, 0f, -0.1f));
            Assert.That(negativeBottom.ParamName, Is.EqualTo("bottomInset"));

            var sumOne = Assert.Throws<ArgumentOutOfRangeException>(
                () => _boardView.FitCamera(_camera, 0.6f, 0.5f));
            Assert.That(sumOne.ParamName, Is.EqualTo("topInset"));
        }

        [Test]
        public void FI6_NullCamera_Throws()
        {
            CreateBoardView();
            _boardView.Show(StageCatalog.CreateStage(0));
            var ex = Assert.Throws<ArgumentNullException>(() => _boardView.FitCamera(null, 0f, 0f));
            Assert.That(ex.ParamName, Is.EqualTo("camera"));
        }

        [Test]
        public void FI7_BeforeShow_Throws()
        {
            CreateCamera(16f / 9f);
            CreateBoardView();
            var ex = Assert.Throws<InvalidOperationException>(() => _boardView.FitCamera(_camera, 0f, 0f));
            Assert.That(ex.Message, Does.StartWith("Show must be called before FitCamera."));
        }

        [Test]
        public void FI8_CellSize_At1280x720_Reference()
        {
            CreateCamera(16f / 9f);
            CreateBoardView();
            _boardView.Show(StageCatalog.CreateStage(1));
            _boardView.FitCamera(_camera, 0.286111f, 0.144444f);
            var echoCellHeight = 720f / (2f * _camera.orthographicSize);
            Assert.That(echoCellHeight, Is.EqualTo(117.1f).Within(0.5f));

            _boardView.Show(StageCatalog.CreateStage(4));
            _boardView.FitCamera(_camera, 0.286111f, 0.144444f);
            var tangleCellHeight = 720f / (2f * _camera.orthographicSize);
            Assert.That(tangleCellHeight, Is.EqualTo(91.1f).Within(0.5f));
        }

        private void CreateCamera(float aspect)
        {
            DestroyCameraResources();
            _cameraObject = new GameObject("BoardViewFitCameraInsetsTests_Camera");
            _camera = _cameraObject.AddComponent<Camera>();
            _camera.orthographic = true;
            var heightPixels = 900;
            var widthPixels = (int)Math.Round(heightPixels * aspect);
            _renderTexture = new RenderTexture(widthPixels, heightPixels, 24);
            _camera.targetTexture = _renderTexture;
            _cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void CreateBoardView()
        {
            _boardRoot = new GameObject("BoardViewFitCameraInsetsTests_Board");
            _boardView = _boardRoot.AddComponent<BoardView>();
        }

        private void AssertTopBottomViewportForFloor(float topInset, float bottomInset, bool expectVerticalTouch)
        {
            var floor = _boardRoot.transform.Find(BoardView.FloorContainerName);
            Assert.That(floor, Is.Not.Null);

            var minY = 1f;
            var maxY = 0f;
            for (var i = 0; i < floor.childCount; i++)
            {
                var center = floor.GetChild(i).position;
                var bottomLeft = _camera.WorldToViewportPoint(new Vector3(center.x - 0.5f, center.y - 0.5f, center.z));
                var topRight = _camera.WorldToViewportPoint(new Vector3(center.x + 0.5f, center.y + 0.5f, center.z));
                Assert.That(bottomLeft.x, Is.GreaterThanOrEqualTo(-0.0001f));
                Assert.That(topRight.x, Is.LessThanOrEqualTo(1.0001f));
                minY = Math.Min(minY, bottomLeft.y);
                maxY = Math.Max(maxY, topRight.y);
            }

            Assert.That(maxY, Is.LessThanOrEqualTo(1f - topInset + 0.0001f));
            Assert.That(minY, Is.GreaterThanOrEqualTo(bottomInset - 0.0001f));

            if (expectVerticalTouch)
            {
                Assert.That(maxY, Is.EqualTo(0.673214f).Within(0.0005f));
                Assert.That(minY, Is.EqualTo(0.185119f).Within(0.0005f));
            }
        }
    }
}
