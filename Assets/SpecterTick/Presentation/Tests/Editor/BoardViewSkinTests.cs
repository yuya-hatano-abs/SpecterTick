using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies BoardView skin application — not on-screen rendering appearance.
    /// </summary>
    [TestFixture]
    public sealed class BoardViewSkinTests
    {
        private GameObject _root;
        private BoardView _boardView;
        private readonly List<Texture2D> _textures = new List<Texture2D>();
        private BoardSkin _ownedSkin;

        [TearDown]
        public void TearDown()
        {
            if (_boardView != null)
            {
                _boardView.ReleaseResources();
            }

            if (_ownedSkin != null)
            {
                _ownedSkin.Release();
                _ownedSkin = null;
            }

            for (var i = 0; i < _textures.Count; i++)
            {
                if (_textures[i] != null)
                {
                    Object.DestroyImmediate(_textures[i]);
                }
            }

            _textures.Clear();

            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            _boardView = null;
            _root = null;
        }

        [Test]
        public void BV1_ApplySkin_FloorWallActors()
        {
            var skin = CreateSyntheticSkin();
            CreateBoardView();
            _boardView.ApplySkin(skin);
            _boardView.Show(SampleStageFactory.Create());

            var floor = _boardView.transform.Find(BoardView.FloorContainerName);
            var walls = _boardView.transform.Find(BoardView.WallsContainerName);
            Assert.That(floor.childCount, Is.EqualTo(20));
            Assert.That(walls.childCount, Is.EqualTo(4));

            for (var i = 0; i < floor.childCount; i++)
            {
                var renderer = floor.GetChild(i).GetComponent<SpriteRenderer>();
                Assert.That(renderer.sprite, Is.SameAs(skin.Floor));
                Assert.That(renderer.color, Is.EqualTo(Color.white));
                Assert.That(renderer.sortingOrder, Is.EqualTo(0));
                Assert.That(floor.GetChild(i).localScale, Is.EqualTo(Vector3.one));
            }

            for (var i = 0; i < walls.childCount; i++)
            {
                var renderer = walls.GetChild(i).GetComponent<SpriteRenderer>();
                Assert.That(renderer.sprite, Is.SameAs(skin.Wall));
                Assert.That(renderer.color, Is.EqualTo(Color.white));
                Assert.That(renderer.sortingOrder, Is.EqualTo(1));
            }

            var player = _boardView.transform.Find(BoardView.ActorsContainerName + "/" + BoardView.PlayerActorName);
            var specter = _boardView.transform.Find(BoardView.ActorsContainerName + "/" + BoardView.SpecterActorName);
            var playerRenderer = player.GetComponent<SpriteRenderer>();
            var specterRenderer = specter.GetComponent<SpriteRenderer>();
            Assert.That(playerRenderer.sprite, Is.SameAs(skin.Player));
            Assert.That(playerRenderer.color, Is.EqualTo(Color.white));
            Assert.That(playerRenderer.sortingOrder, Is.EqualTo(3));
            Assert.That(specterRenderer.sprite, Is.SameAs(skin.Specter));
            Assert.That(specterRenderer.color, Is.EqualTo(Color.white));
            Assert.That(specterRenderer.sortingOrder, Is.EqualTo(2));
        }

        [Test]
        public void BV2_ApplySkin_ElementStates()
        {
            var skin = CreateSyntheticSkin();
            CreateBoardView();
            _boardView.ApplySkin(skin);
            _boardView.Show(SampleStageFactory.CreateDemo());

            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(session));

            var elements = _boardView.transform.Find(BoardView.ElementsContainerName);
            Assert.That(elements.childCount, Is.EqualTo(5));
            AssertElementSkin(elements.GetChild(0), skin.ExitClosed, 1);
            AssertElementSkin(elements.GetChild(1), skin.SwitchOff, 1);
            AssertElementSkin(elements.GetChild(2), skin.SwitchOff, 1);
            AssertElementSkin(elements.GetChild(3), skin.DoorClosed, 1);
            AssertElementSkin(elements.GetChild(4), skin.DoorClosed, 1);

            var clearedSession = CreateClearedDemoSession();
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(clearedSession));

            AssertElementSkin(elements.GetChild(0), skin.ExitOpen, 1);
            AssertElementSkin(elements.GetChild(1), skin.SwitchOn, 1);
            AssertElementSkin(elements.GetChild(2), skin.SwitchOn, 1);
            AssertElementSkin(elements.GetChild(3), skin.DoorOpen, 1);
            AssertElementSkin(elements.GetChild(4), skin.DoorOpen, 1);
        }

        [Test]
        public void BV3_PartialSkin_FallsBackPerSprite()
        {
            var full = CreateSyntheticSkin();
            var skin = new BoardSkin(
                null,
                full.Wall,
                null,
                null,
                null,
                full.ExitOpen,
                null,
                null,
                null,
                null);
            _ownedSkin = full;

            CreateBoardView();
            _boardView.ApplySkin(skin);
            _boardView.Show(SampleStageFactory.CreateDemo());

            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(session));

            var walls = _boardView.transform.Find(BoardView.WallsContainerName);
            for (var i = 0; i < walls.childCount; i++)
            {
                var renderer = walls.GetChild(i).GetComponent<SpriteRenderer>();
                Assert.That(renderer.sprite, Is.SameAs(skin.Wall));
                Assert.That(renderer.color, Is.EqualTo(Color.white));
            }

            var floor = _boardView.transform.Find(BoardView.FloorContainerName);
            var floorRenderer = floor.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.That(floorRenderer.sprite == null, Is.False);
            Assert.That(floorRenderer.color, Is.EqualTo(new Color(0.22f, 0.24f, 0.28f, 1f)));

            var player = _boardView.transform.Find(BoardView.ActorsContainerName + "/" + BoardView.PlayerActorName);
            Assert.That(player.GetComponent<SpriteRenderer>().color, Is.EqualTo(new Color(0.15f, 0.55f, 0.95f, 1f)));
            var specter = _boardView.transform.Find(BoardView.ActorsContainerName + "/" + BoardView.SpecterActorName);
            Assert.That(specter.GetComponent<SpriteRenderer>().color, Is.EqualTo(new Color(0.85f, 0.35f, 0.15f, 1f)));

            var elements = _boardView.transform.Find(BoardView.ElementsContainerName);
            var exitRenderer = elements.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.That(exitRenderer.sprite.name, Is.EqualTo("ExitClosed"));
            Assert.That(exitRenderer.color, Is.EqualTo(new Color(0.65f, 0.67f, 0.72f, 1f)));

            var clearedSession = CreateClearedDemoSession();
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(clearedSession));

            exitRenderer = elements.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.That(exitRenderer.sprite, Is.SameAs(skin.ExitOpen));
            Assert.That(exitRenderer.color, Is.EqualTo(Color.white));
            Assert.That(elements.GetChild(1).GetComponent<SpriteRenderer>().color, Is.EqualTo(new Color(1.00f, 0.88f, 0.20f, 1f)));
            Assert.That(elements.GetChild(3).GetComponent<SpriteRenderer>().color, Is.EqualTo(new Color(0.85f, 0.65f, 0.95f, 1f)));
            Assert.That(elements.GetChild(4).GetComponent<SpriteRenderer>().color, Is.EqualTo(new Color(0.85f, 0.65f, 0.95f, 1f)));
        }

        [Test]
        public void BV4_ApplySkinNull_RestoresDefault()
        {
            var skin = CreateSyntheticSkin();
            CreateBoardView();
            _boardView.ApplySkin(skin);
            _boardView.Show(SampleStageFactory.Create());

            var floorBefore = _boardView.transform.Find(BoardView.FloorContainerName);
            var skinFloor = floorBefore.GetChild(0).GetComponent<SpriteRenderer>().sprite;

            _boardView.ApplySkin(null);
            _boardView.Show(SampleStageFactory.Create());

            var floor = _boardView.transform.Find(BoardView.FloorContainerName);
            var defaultRenderer = floor.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.That(defaultRenderer.color, Is.EqualTo(new Color(0.22f, 0.24f, 0.28f, 1f)));
            Assert.That(defaultRenderer.sprite, Is.Not.SameAs(skinFloor));
        }

        [Test]
        public void BV5_ApplySkin_TakesEffectOnNextShow()
        {
            var skin = CreateSyntheticSkin();
            CreateBoardView();
            _boardView.Show(SampleStageFactory.Create());

            var floor = _boardView.transform.Find(BoardView.FloorContainerName);
            var colorBefore = floor.GetChild(0).GetComponent<SpriteRenderer>().color;
            Assert.That(colorBefore, Is.EqualTo(new Color(0.22f, 0.24f, 0.28f, 1f)));

            _boardView.ApplySkin(skin);
            Assert.That(floor.GetChild(0).GetComponent<SpriteRenderer>().color, Is.EqualTo(colorBefore));

            _boardView.Show(SampleStageFactory.Create());
            floor = _boardView.transform.Find(BoardView.FloorContainerName);
            Assert.That(floor.GetChild(0).GetComponent<SpriteRenderer>().sprite, Is.SameAs(skin.Floor));
        }

        [Test]
        public void BV6_ReleaseResources_DoesNotDestroySkinSprites()
        {
            var skin = CreateSyntheticSkin();
            var sprites = CollectSkinSprites(skin);
            CreateBoardView();
            _boardView.ApplySkin(skin);
            _boardView.Show(SampleStageFactory.Create());
            _boardView.ReleaseResources();

            for (var i = 0; i < sprites.Length; i++)
            {
                Assert.That(sprites[i] == null, Is.False);
            }

            Object.DestroyImmediate(_root);
            _root = null;
            _boardView = null;

            for (var i = 0; i < sprites.Length; i++)
            {
                Assert.That(sprites[i] == null, Is.False);
            }
        }

        [Test]
        public void BV7_Skin_KeepsContainerStructure()
        {
            var skin = CreateSyntheticSkin();
            CreateBoardView();
            _boardView.ApplySkin(skin);
            _boardView.Show(SampleStageFactory.Create());

            Assert.That(_boardView.transform.childCount, Is.EqualTo(4));
            Assert.That(_boardView.transform.GetChild(0).name, Is.EqualTo(BoardView.FloorContainerName));
            Assert.That(_boardView.transform.GetChild(1).name, Is.EqualTo(BoardView.WallsContainerName));
            Assert.That(_boardView.transform.GetChild(2).name, Is.EqualTo(BoardView.ActorsContainerName));
            Assert.That(_boardView.transform.GetChild(3).name, Is.EqualTo(BoardView.ElementsContainerName));

            var elements = _boardView.transform.Find(BoardView.ElementsContainerName);
            Assert.That(elements.childCount, Is.EqualTo(1));
            var actors = _boardView.transform.Find(BoardView.ActorsContainerName);
            Assert.That(actors.childCount, Is.EqualTo(2));
        }

        private static GridPuzzleEngineSession CreateClearedDemoSession()
        {
            var stage = SampleStageFactory.CreateDemo();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(dispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Cleared));
            return session;
        }

        private static void AssertElementSkin(Transform child, Sprite expectedSprite, int sortingOrder)
        {
            var renderer = child.GetComponent<SpriteRenderer>();
            Assert.That(renderer.sprite, Is.SameAs(expectedSprite));
            Assert.That(renderer.color, Is.EqualTo(Color.white));
            Assert.That(renderer.sortingOrder, Is.EqualTo(sortingOrder));
        }

        private static Sprite[] CollectSkinSprites(BoardSkin skin)
        {
            return new[]
            {
                skin.Floor,
                skin.Wall,
                skin.Player,
                skin.Specter,
                skin.ExitClosed,
                skin.ExitOpen,
                skin.SwitchOff,
                skin.SwitchOn,
                skin.DoorClosed,
                skin.DoorOpen
            };
        }

        private BoardSkin CreateSyntheticSkin()
        {
            var sprites = new Sprite[10];
            for (var i = 0; i < BoardSkin.SpriteNames.Count; i++)
            {
                sprites[i] = CreateNamedSprite(BoardSkin.SpriteNames[i]);
            }

            _ownedSkin = new BoardSkin(
                sprites[0],
                sprites[1],
                sprites[2],
                sprites[3],
                sprites[4],
                sprites[5],
                sprites[6],
                sprites[7],
                sprites[8],
                sprites[9]);
            return _ownedSkin;
        }

        private Sprite CreateNamedSprite(string name)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _textures.Add(texture);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
            sprite.name = name;
            return sprite;
        }

        private void CreateBoardView()
        {
            _root = new GameObject("BoardViewSkinTestsRoot");
            _boardView = _root.AddComponent<BoardView>();
        }
    }
}
