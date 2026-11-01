using System;
using System.Collections.Generic;
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
        public const string ElementsContainerName = "Elements";
        public const string PlayerActorName = "Player";
        public const string SpecterActorName = "Specter";

        private const int ElementSpriteCount = 6;
        private const int ElementSortingOrder = 1;

        private static readonly Color FloorColor = new Color(0.22f, 0.24f, 0.28f);
        private static readonly Color WallColor = new Color(0.55f, 0.55f, 0.58f);
        private static readonly Color SpecterColor = new Color(0.85f, 0.35f, 0.15f);
        private static readonly Color PlayerColor = new Color(0.15f, 0.55f, 0.95f);

        private static readonly Color ExitClosedColor = new Color(0.65f, 0.67f, 0.72f, 1f);
        private static readonly Color ExitOpenColor = new Color(0.20f, 0.85f, 0.75f, 1f);
        private static readonly Color SwitchOffColor = new Color(0.70f, 0.60f, 0.20f, 1f);
        private static readonly Color SwitchOnColor = new Color(1.00f, 0.88f, 0.20f, 1f);
        private static readonly Color DoorClosedColor = new Color(0.75f, 0.40f, 0.85f, 1f);
        private static readonly Color DoorOpenColor = new Color(0.85f, 0.65f, 0.95f, 1f);

        [SerializeField]
        private float cellSize = 1f;

        [SerializeField]
        private Camera targetCamera;

        private Texture2D _sharedTexture;
        private Sprite _sharedSprite;

        private readonly Sprite[] _elementSprites = new Sprite[ElementSpriteCount];
        private readonly Texture2D[] _elementTextures = new Texture2D[ElementSpriteCount];

        private Transform _floorRoot;
        private Transform _wallsRoot;
        private Transform _actorsRoot;
        private Transform _elementsRoot;

        private readonly List<ElementEntry> _elementEntries = new List<ElementEntry>();

        private GridWorldMapper _mapper;
        private int _stageWidth;
        private int _stageHeight;
        private bool _isBoardBuilt;
        private Transform _playerTransform;
        private Transform _specterTransform;

        private BoardSkin _skin;

        public void ApplySkin(BoardSkin skin)
        {
            _skin = skin;
        }

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
            CreateElementSprites();

            _floorRoot = CreateContainer(FloorContainerName);
            _wallsRoot = CreateContainer(WallsContainerName);
            _actorsRoot = CreateContainer(ActorsContainerName);
            _elementsRoot = CreateContainer(ElementsContainerName);

            for (var y = 0; y < stage.Height; y++)
            {
                for (var x = 0; x < stage.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    CreateCell(_floorRoot, position, FloorColor, 0, sharedSprite, null, _skin?.Floor);
                }
            }

            var walls = stage.Walls;
            for (var i = 0; i < walls.Count; i++)
            {
                CreateCell(_wallsRoot, walls[i], WallColor, 1, sharedSprite, null, _skin?.Wall);
            }

            _playerTransform = CreateCell(_actorsRoot, stage.PlayerPosition, PlayerColor, 3, sharedSprite, PlayerActorName, _skin?.Player).transform;
            _specterTransform = CreateCell(_actorsRoot, stage.SpecterPosition, SpecterColor, 2, sharedSprite, SpecterActorName, _skin?.Specter).transform;

            CreateStageElements(stage);

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

        public void UpdateElements(BoardElementsModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!_isBoardBuilt)
            {
                throw new InvalidOperationException("Show must be called before UpdateElements.");
            }

            if (model.Elements.Count != _elementEntries.Count)
            {
                throw new ArgumentException("Element count does not match the built board.");
            }

            for (var i = 0; i < _elementEntries.Count; i++)
            {
                if (model.Elements[i].Position != _elementEntries[i].Position)
                {
                    throw new ArgumentException("Element position does not match the built board.");
                }
            }

            for (var i = 0; i < _elementEntries.Count; i++)
            {
                ApplyElementVisual(_elementEntries[i], model.Elements[i].IsActive);
            }
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

        public void FitCamera(Camera camera, float topInset, float bottomInset)
        {
            if (!_isBoardBuilt)
            {
                throw new InvalidOperationException("Show must be called before FitCamera.");
            }

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (topInset < 0f || topInset >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(topInset));
            }

            if (bottomInset < 0f || bottomInset >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(bottomInset));
            }

            if (topInset + bottomInset >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(topInset));
            }

            if (!camera.orthographic)
            {
                Debug.LogWarning("BoardView.FitCamera requires an orthographic camera.");
                return;
            }

            var aspect = camera.aspect;
            var availableHeightFraction = 1f - topInset - bottomInset;
            var bandCenterY = bottomInset + (availableHeightFraction / 2f);
            var verticalHalfExtent = (_stageHeight + BoardScreenInsets.VerticalPadding) * cellSize / (2f * availableHeightFraction);
            var horizontalHalfExtent = (_stageWidth + 2) * cellSize / (2f * aspect);
            var size = Math.Max(verticalHalfExtent, horizontalHalfExtent);

            _mapper.GetBoardCenter(_stageWidth, _stageHeight, out var centerX, out var centerY);
            centerY += (0.5f - bandCenterY) * 2f * size;

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
                Debug.LogWarning("BoardView.FitCamera: no camera assigned and Camera.main is null.");
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

        private void CreateStageElements(IStage stage)
        {
            _elementEntries.Clear();

            var exitRenderer = CreateElementCell(
                stage.ExitPosition,
                "Exit",
                BoardElementKind.Exit,
                false);
            _elementEntries.Add(new ElementEntry(BoardElementKind.Exit, stage.ExitPosition, exitRenderer));

            var switches = stage.Switches;
            for (var i = 0; i < switches.Count; i++)
            {
                var sw = switches[i];
                var name = "Switch_" + sw.Id.RawValue;
                var renderer = CreateElementCell(sw.Position, name, BoardElementKind.Switch, false);
                _elementEntries.Add(new ElementEntry(BoardElementKind.Switch, sw.Position, renderer));
            }

            var doors = stage.Doors;
            for (var i = 0; i < doors.Count; i++)
            {
                var door = doors[i];
                var name = "Door_" + door.Id.RawValue;
                var renderer = CreateElementCell(door.Position, name, BoardElementKind.Door, false);
                _elementEntries.Add(new ElementEntry(BoardElementKind.Door, door.Position, renderer));
            }
        }

        private SpriteRenderer CreateElementCell(
            GridPosition gridPosition,
            string objectName,
            BoardElementKind kind,
            bool isActive)
        {
            var cell = new GameObject(objectName);
            cell.transform.SetParent(_elementsRoot, false);
            cell.transform.localPosition = new Vector3(
                _mapper.CellCenterX(gridPosition),
                _mapper.CellCenterY(gridPosition),
                0f);

            var renderer = cell.AddComponent<SpriteRenderer>();
            ApplyElementVisual(new ElementEntry(kind, gridPosition, renderer), isActive);
            renderer.sortingOrder = ElementSortingOrder;
            return renderer;
        }

        private void ApplyElementVisual(ElementEntry entry, bool isActive)
        {
            switch (entry.Kind)
            {
                case BoardElementKind.Exit:
                    ApplyCellLook(
                        entry.Renderer,
                        isActive ? _skin?.ExitOpen : _skin?.ExitClosed,
                        isActive ? _elementSprites[1] : _elementSprites[0],
                        isActive ? ExitOpenColor : ExitClosedColor);
                    break;
                case BoardElementKind.Switch:
                    ApplyCellLook(
                        entry.Renderer,
                        isActive ? _skin?.SwitchOn : _skin?.SwitchOff,
                        isActive ? _elementSprites[3] : _elementSprites[2],
                        isActive ? SwitchOnColor : SwitchOffColor);
                    break;
                case BoardElementKind.Door:
                    ApplyCellLook(
                        entry.Renderer,
                        isActive ? _skin?.DoorOpen : _skin?.DoorClosed,
                        isActive ? _elementSprites[5] : _elementSprites[4],
                        isActive ? DoorOpenColor : DoorClosedColor);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry));
            }
        }

        private static void ApplyCellLook(
            SpriteRenderer renderer,
            Sprite skinSprite,
            Sprite fallbackSprite,
            Color fallbackColor)
        {
            if (skinSprite != null)
            {
                renderer.sprite = skinSprite;
                renderer.color = Color.white;
            }
            else
            {
                renderer.sprite = fallbackSprite;
                renderer.color = fallbackColor;
            }
        }

        private void CreateElementSprites()
        {
            CreateMaskedSprite(0, "ExitClosed", IsExitClosedPixel);
            CreateMaskedSprite(1, "ExitOpen", IsExitOpenPixel);
            CreateMaskedSprite(2, "SwitchOff", IsSwitchOffPixel);
            CreateMaskedSprite(3, "SwitchOn", IsSwitchOnPixel);
            CreateMaskedSprite(4, "DoorClosed", IsDoorClosedPixel);
            CreateMaskedSprite(5, "DoorOpen", IsDoorOpenPixel);
        }

        private void CreateMaskedSprite(int index, string spriteName, Func<double, double, bool> mask)
        {
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[32 * 32];
            for (var y = 0; y < 32; y++)
            {
                for (var x = 0; x < 32; x++)
                {
                    var dx = x - 15.5;
                    var dy = y - 15.5;
                    var on = mask(dx, dy);
                    pixels[y * 32 + x] = on ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
            sprite.name = spriteName;

            _elementTextures[index] = texture;
            _elementSprites[index] = sprite;
        }

        private static bool IsExitClosedPixel(double dx, double dy)
        {
            var manhattan = Math.Abs(dx) + Math.Abs(dy);
            return manhattan > 10d && manhattan <= 14d;
        }

        private static bool IsExitOpenPixel(double dx, double dy)
        {
            return Math.Abs(dx) + Math.Abs(dy) <= 14d;
        }

        private static bool IsSwitchOffPixel(double dx, double dy)
        {
            var r2 = dx * dx + dy * dy;
            return r2 > 49d && r2 <= 121d;
        }

        private static bool IsSwitchOnPixel(double dx, double dy)
        {
            return dx * dx + dy * dy <= 121d;
        }

        private static bool IsDoorClosedPixel(double dx, double dy)
        {
            return Math.Max(Math.Abs(dx), Math.Abs(dy)) <= 12.5d;
        }

        private static bool IsDoorOpenPixel(double dx, double dy)
        {
            var chebyshev = Math.Max(Math.Abs(dx), Math.Abs(dy));
            return chebyshev > 9d && chebyshev <= 12.5d;
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
            string objectName = null,
            Sprite skinSprite = null)
        {
            var cell = new GameObject(objectName ?? "Cell");
            cell.transform.SetParent(parent, false);
            cell.transform.localPosition = new Vector3(
                _mapper.CellCenterX(gridPosition),
                _mapper.CellCenterY(gridPosition),
                0f);

            var renderer = cell.AddComponent<SpriteRenderer>();
            ApplyCellLook(renderer, skinSprite, sprite, color);
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
            ClearContainerRoot(ref _elementsRoot);
            _elementEntries.Clear();
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

            for (var i = 0; i < ElementSpriteCount; i++)
            {
                if (_elementSprites[i] != null)
                {
                    DestroyObject(_elementSprites[i]);
                    _elementSprites[i] = null;
                }

                if (_elementTextures[i] != null)
                {
                    DestroyObject(_elementTextures[i]);
                    _elementTextures[i] = null;
                }
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

        private sealed class ElementEntry
        {
            public ElementEntry(BoardElementKind kind, GridPosition position, SpriteRenderer renderer)
            {
                Kind = kind;
                Position = position;
                Renderer = renderer;
            }

            public BoardElementKind Kind { get; }

            public GridPosition Position { get; }

            public SpriteRenderer Renderer { get; }
        }
    }
}
