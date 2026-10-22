using System;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using UnityEngine;

namespace SpecterTick.Presentation
{
    /// <summary>
    /// Renders a validated stage as colored sprites. Container and actor child names are part of the test contract.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        public const string FloorContainerName = "Floor";
        public const string WallsContainerName = "Walls";
        public const string ActorsContainerName = "Actors";
        public const string PlayerActorName = "Player";
        public const string SpecterActorName = "Specter";

        private static readonly Color FloorColor = new Color(0.22f, 0.24f, 0.28f);
        private static readonly Color WallColor = new Color(0.55f, 0.55f, 0.58f);
        private static readonly Color SpecterColor = new Color(0.85f, 0.35f, 0.15f);
        private static readonly Color PlayerColor = new Color(0.15f, 0.55f, 0.95f);

        [SerializeField]
        private float cellSize = 1f;

        [SerializeField]
        private Camera targetCamera;

        private Texture2D _sharedTexture;
        private Sprite _sharedSprite;

        private Transform _floorRoot;
        private Transform _wallsRoot;
        private Transform _actorsRoot;

        private GridWorldMapper _mapper;
        private int _stageWidth;
        private int _stageHeight;
        private bool _isBoardBuilt;
        private Transform _playerTransform;
        private Transform _specterTransform;

        public void Show(IStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            ClearBoardHierarchy();
            ReleaseOwnedAssets();

            _mapper = new GridWorldMapper(cellSize);
            _stageWidth = stage.Width;
            _stageHeight = stage.Height;

            var sharedSprite = CreateSharedSprite();

            _floorRoot = CreateContainer(FloorContainerName);
            _wallsRoot = CreateContainer(WallsContainerName);
            _actorsRoot = CreateContainer(ActorsContainerName);

            for (var y = 0; y < stage.Height; y++)
            {
                for (var x = 0; x < stage.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    CreateCell(_floorRoot, position, FloorColor, 0, sharedSprite);
                }
            }

            var walls = stage.Walls;
            for (var i = 0; i < walls.Count; i++)
            {
                CreateCell(_wallsRoot, walls[i], WallColor, 1, sharedSprite);
            }

            _playerTransform = CreateCell(_actorsRoot, stage.PlayerPosition, PlayerColor, 3, sharedSprite, PlayerActorName).transform;
            _specterTransform = CreateCell(_actorsRoot, stage.SpecterPosition, SpecterColor, 2, sharedSprite, SpecterActorName).transform;

            _isBoardBuilt = true;
        }

        public void UpdateActors(GameState state)
        {
            if (!_isBoardBuilt)
            {
                throw new InvalidOperationException("Show must be called before UpdateActors.");
            }

            PlaceActor(_playerTransform, state.PlayerPosition);
            PlaceActor(_specterTransform, state.SpecterPosition);
        }

        public void FitCamera(Camera camera)
        {
            if (!_isBoardBuilt)
            {
                throw new InvalidOperationException("Show must be called before FitCamera.");
            }

            if (camera == null)
            {
                return;
            }

            if (!camera.orthographic)
            {
                Debug.LogWarning("BoardView.FitCamera requires an orthographic camera.");
                return;
            }

            var aspect = camera.aspect;
            var size = _mapper.ComputeOrthographicSize(_stageWidth, _stageHeight, aspect);
            _mapper.GetBoardCenter(_stageWidth, _stageHeight, out var centerX, out var centerY);

            var position = camera.transform.position;
            camera.transform.position = new Vector3(centerX, centerY, position.z);
            camera.orthographicSize = size;
        }

        public void FitCamera()
        {
            var camera = targetCamera != null ? targetCamera : Camera.main;
            if (camera != null)
            {
                FitCamera(camera);
            }
            else
            {
                Debug.LogWarning("BoardView.Start: no camera assigned and Camera.main is null.");
            }
        }

        public void ReleaseResources()
        {
            ClearBoardHierarchy();
            ReleaseOwnedAssets();
            _isBoardBuilt = false;
            _playerTransform = null;
            _specterTransform = null;
            _mapper = null;
            _stageWidth = 0;
            _stageHeight = 0;
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private Transform CreateContainer(string name)
        {
            var container = new GameObject(name);
            container.transform.SetParent(transform, false);
            return container.transform;
        }

        private GameObject CreateCell(
            Transform parent,
            GridPosition gridPosition,
            Color color,
            int sortingOrder,
            Sprite sprite,
            string objectName = null)
        {
            var cell = new GameObject(objectName ?? "Cell");
            cell.transform.SetParent(parent, false);
            cell.transform.localPosition = new Vector3(
                _mapper.CellCenterX(gridPosition),
                _mapper.CellCenterY(gridPosition),
                0f);

            var renderer = cell.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return cell;
        }

        private void PlaceActor(Transform actorTransform, GridPosition gridPosition)
        {
            actorTransform.localPosition = new Vector3(
                _mapper.CellCenterX(gridPosition),
                _mapper.CellCenterY(gridPosition),
                0f);
        }

        private Sprite CreateSharedSprite()
        {
            _sharedTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _sharedTexture.SetPixel(0, 0, Color.white);
            _sharedTexture.Apply();
            _sharedSprite = Sprite.Create(_sharedTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            return _sharedSprite;
        }

        private void ClearBoardHierarchy()
        {
            ClearContainerRoot(ref _floorRoot);
            ClearContainerRoot(ref _wallsRoot);
            ClearContainerRoot(ref _actorsRoot);
        }

        private void ClearContainerRoot(ref Transform root)
        {
            if (root == null)
            {
                return;
            }

            // Unparent before Destroy so delayed Play-mode teardown does not leave stale children under this transform.
            root.SetParent(null, false);
            DestroyObject(root.gameObject);
            root = null;
        }

        private void ReleaseOwnedAssets()
        {
            if (_sharedSprite != null)
            {
                DestroyObject(_sharedSprite);
                _sharedSprite = null;
            }

            if (_sharedTexture != null)
            {
                DestroyObject(_sharedTexture);
                _sharedTexture = null;
            }
        }

        private static void DestroyObject(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }
    }
}
