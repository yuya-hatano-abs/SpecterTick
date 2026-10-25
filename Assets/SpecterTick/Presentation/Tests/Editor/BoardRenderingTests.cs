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
    /// Verifies board structure and local positions only — not on-screen rendering appearance.
    /// Depends on BoardView container and actor object names.
    /// </summary>
    [TestFixture]
    public sealed class BoardRenderingTests
    {
        private GameObject _root;
        private BoardView _boardView;

        [TearDown]
        public void TearDown()
        {
            if (_boardView != null)
            {
                _boardView.ReleaseResources();
            }

            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            _boardView = null;
            _root = null;
        }

        [Test]
        public void S1_SampleStage_Shape()
        {
            var stage = SampleStageFactory.Create();

            Assert.That(stage.Width, Is.EqualTo(5));
            Assert.That(stage.Height, Is.EqualTo(4));
            Assert.That(stage.PlayerPosition, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(stage.SpecterPosition, Is.EqualTo(new GridPosition(1, 0)));
            Assert.That(stage.ExitPosition, Is.EqualTo(new GridPosition(4, 3)));
            Assert.That(stage.Walls.Count, Is.EqualTo(4));
            Assert.That(
                new HashSet<GridPosition>(stage.Walls),
                Is.EquivalentTo(new[]
                {
                    new GridPosition(2, 0),
                    new GridPosition(2, 1),
                    new GridPosition(2, 2),
                    new GridPosition(4, 1)
                }));
            Assert.That(stage.Switches.Count, Is.EqualTo(0));
            Assert.That(stage.Doors.Count, Is.EqualTo(0));
        }

        [Test]
        public void V1_Show_CreatesFloorAndWallCells()
        {
            var stage = SampleStageFactory.Create();
            CreateBoardView();
            _boardView.Show(stage);

            var floor = _boardView.transform.Find(BoardView.FloorContainerName);
            var walls = _boardView.transform.Find(BoardView.WallsContainerName);
            Assert.That(floor.childCount, Is.EqualTo(20));
            Assert.That(walls.childCount, Is.EqualTo(4));

            var wallPositions = CollectLocalPositions(walls);
            Assert.That(
                wallPositions,
                Is.EquivalentTo(new[]
                {
                    new Vector3(2f, 0f, 0f),
                    new Vector3(2f, 1f, 0f),
                    new Vector3(2f, 2f, 0f),
                    new Vector3(4f, 1f, 0f)
                }));

            var floorPositions = CollectLocalPositions(floor);
            Assert.That(floorPositions, Does.Contain(new Vector3(0f, 0f, 0f)));
            Assert.That(floorPositions, Does.Contain(new Vector3(4f, 3f, 0f)));
            Assert.That(floorPositions, Does.Contain(new Vector3(4f, 0f, 0f)));
            Assert.That(floorPositions, Does.Contain(new Vector3(0f, 3f, 0f)));
        }

        [Test]
        public void V2_ShowTwice_DoesNotDuplicate()
        {
            var stage = SampleStageFactory.Create();
            CreateBoardView();
            _boardView.Show(stage);
            _boardView.Show(stage);

            var floor = _boardView.transform.Find(BoardView.FloorContainerName);
            var walls = _boardView.transform.Find(BoardView.WallsContainerName);
            var actors = _boardView.transform.Find(BoardView.ActorsContainerName);
            Assert.That(floor.childCount, Is.EqualTo(20));
            Assert.That(walls.childCount, Is.EqualTo(4));
            Assert.That(actors.childCount, Is.EqualTo(2));
            Assert.That(_boardView.transform.childCount, Is.EqualTo(4));

            Assert.Throws<ArgumentNullException>(() => _boardView.Show(null));
        }

        [Test]
        public void V4_ReleaseResources_ClearsBoardAndAssets()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            CreateBoardView();
            _boardView.Show(stage);

            var floor = _boardView.transform.Find(BoardView.FloorContainerName);
            var firstFloorCell = floor.GetChild(0);
            var renderer = firstFloorCell.GetComponent<SpriteRenderer>();
            var sprite = renderer.sprite;
            var texture = sprite.texture;

            _boardView.ReleaseResources();

            Assert.That(_boardView.transform.childCount, Is.EqualTo(0));
            Assert.That(sprite == null, Is.True);
            Assert.That(texture == null, Is.True);
            Assert.Throws<InvalidOperationException>(() => _boardView.UpdateActors(session.State));
            _boardView.ReleaseResources();
        }

        [Test]
        public void V3_UpdateActors_FollowsState()
        {
            var stage = SampleStageFactory.Create();
            var session = new GridPuzzleEngineSession(stage);
            CreateBoardView();
            _boardView.Show(stage);
            _boardView.UpdateActors(session.State);

            var player = _boardView.transform.Find(BoardView.ActorsContainerName + "/" + BoardView.PlayerActorName);
            var specter = _boardView.transform.Find(BoardView.ActorsContainerName + "/" + BoardView.SpecterActorName);
            Assert.That(player.localPosition, Is.EqualTo(new Vector3(0f, 0f, 0f)));
            Assert.That(specter.localPosition, Is.EqualTo(new Vector3(1f, 0f, 0f)));

            session.Step(MoveDirection.Up);
            _boardView.UpdateActors(session.State);
            Assert.That(player.localPosition, Is.EqualTo(new Vector3(0f, 1f, 0f)));
            Assert.That(specter.localPosition, Is.EqualTo(new Vector3(1f, 0f, 0f)));

            var fresh = new GameObject("FreshBoardView").AddComponent<BoardView>();
            Assert.Throws<InvalidOperationException>(() => fresh.UpdateActors(session.State));
            Object.DestroyImmediate(fresh.gameObject);
        }

        private void CreateBoardView()
        {
            _root = new GameObject("BoardViewTestRoot");
            _boardView = _root.AddComponent<BoardView>();
        }

        private static List<Vector3> CollectLocalPositions(Transform parent)
        {
            var list = new List<Vector3>(parent.childCount);
            for (var i = 0; i < parent.childCount; i++)
            {
                list.Add(parent.GetChild(i).localPosition);
            }

            return list;
        }
    }
}
