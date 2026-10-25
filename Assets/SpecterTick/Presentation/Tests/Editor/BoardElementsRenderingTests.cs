using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Core.Stages;
using SpecterTick.Core.Validation;
using SpecterTick.Presentation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Verifies element object names, positions, sortingOrder, sprite names, and colors — not on-screen shape readability.
    /// R10 and R11 use EditMode DestroyImmediate only; they do not guarantee Play-mode delayed Destroy safety.
    /// </summary>
    [TestFixture]
    public sealed class BoardElementsRenderingTests
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
        public void R1_Show_Demo_CreatesElements()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.CreateDemo());

            Assert.That(_boardView.transform.childCount, Is.EqualTo(4));
            Assert.That(_boardView.transform.GetChild(0).name, Is.EqualTo("Floor"));
            Assert.That(_boardView.transform.GetChild(1).name, Is.EqualTo("Walls"));
            Assert.That(_boardView.transform.GetChild(2).name, Is.EqualTo("Actors"));
            Assert.That(_boardView.transform.GetChild(3).name, Is.EqualTo("Elements"));

            var elements = _boardView.transform.Find("Elements");
            Assert.That(elements.childCount, Is.EqualTo(5));
            Assert.That(elements.GetChild(0).name, Is.EqualTo("Exit"));
            Assert.That(elements.GetChild(1).name, Is.EqualTo("Switch_S1"));
            Assert.That(elements.GetChild(2).name, Is.EqualTo("Switch_S2"));
            Assert.That(elements.GetChild(3).name, Is.EqualTo("Door_D1"));
            Assert.That(elements.GetChild(4).name, Is.EqualTo("Door_D2"));

            AssertElementRenderer(elements.GetChild(0), 1, 5f, 0f, 0f, "ExitClosed");
            AssertElementRenderer(elements.GetChild(1), 1, 1f, 2f, 0f, "SwitchOff");
            AssertElementRenderer(elements.GetChild(2), 1, 4f, 2f, 0f, "SwitchOff");
            AssertElementRenderer(elements.GetChild(3), 1, 3f, 2f, 0f, "DoorClosed");
            AssertElementRenderer(elements.GetChild(4), 1, 5f, 1f, 0f, "DoorClosed");
        }

        [Test]
        public void R2_Show_SG_ExitOnly_AndUpdateOpensIt()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.Create());

            var elements = _boardView.transform.Find("Elements");
            Assert.That(elements.childCount, Is.EqualTo(1));
            Assert.That(elements.GetChild(0).name, Is.EqualTo("Exit"));
            AssertElementRenderer(elements.GetChild(0), 1, 4f, 3f, 0f, "ExitClosed");

            var session = new GridPuzzleEngineSession(SampleStageFactory.Create());
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(session));

            var exitRenderer = elements.GetChild(0).GetComponent<SpriteRenderer>();
            Assert.That(exitRenderer.sprite.name, Is.EqualTo("ExitOpen"));
            Assert.That(exitRenderer.color, Is.EqualTo(new Color(0.20f, 0.85f, 0.75f, 1f)));
        }

        [Test]
        public void R3_Update_Demo_Initial_SpritesAndColors()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.CreateDemo());
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(session));

            var elements = _boardView.transform.Find("Elements");
            AssertSpriteAndColor(elements.GetChild(0), "ExitClosed", 0.65f, 0.67f, 0.72f);
            AssertSpriteAndColor(elements.GetChild(1), "SwitchOff", 0.70f, 0.60f, 0.20f);
            AssertSpriteAndColor(elements.GetChild(2), "SwitchOff", 0.70f, 0.60f, 0.20f);
            AssertSpriteAndColor(elements.GetChild(3), "DoorClosed", 0.75f, 0.40f, 0.85f);
            AssertSpriteAndColor(elements.GetChild(4), "DoorClosed", 0.75f, 0.40f, 0.85f);
        }

        [Test]
        public void R4_Update_Demo_Recording_PartialState()
        {
            var stage = SampleStageFactory.CreateDemo();
            var session = new GridPuzzleEngineSession(stage);
            var dispatcher = new SessionInputDispatcher(session);

            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(1));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(2));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 2)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(session.State.Tick, Is.EqualTo(3));
            Assert.That(session.State.PlayerPosition, Is.EqualTo(new GridPosition(1, 2)));
            Assert.That(session.State.Mode, Is.EqualTo(GameMode.Recording));

            CreateBoardView();
            _boardView.Show(stage);
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(session));

            var elements = _boardView.transform.Find("Elements");
            AssertSpriteAndColor(elements.GetChild(0), "ExitClosed", 0.65f, 0.67f, 0.72f);
            AssertSpriteAndColor(elements.GetChild(1), "SwitchOn", 1.00f, 0.88f, 0.20f);
            AssertSpriteAndColor(elements.GetChild(2), "SwitchOff", 0.70f, 0.60f, 0.20f);
            AssertSpriteAndColor(elements.GetChild(3), "DoorOpen", 0.85f, 0.65f, 0.95f);
            AssertSpriteAndColor(elements.GetChild(4), "DoorClosed", 0.75f, 0.40f, 0.85f);
        }

        [Test]
        public void R5_Update_Demo_Cleared_AllActive()
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
            Assert.That(session.State.Tick, Is.EqualTo(4));

            CreateBoardView();
            _boardView.Show(stage);
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(session));

            var elements = _boardView.transform.Find("Elements");
            AssertSpriteAndColor(elements.GetChild(0), "ExitOpen", 0.20f, 0.85f, 0.75f);
            AssertSpriteAndColor(elements.GetChild(1), "SwitchOn", 1.00f, 0.88f, 0.20f);
            AssertSpriteAndColor(elements.GetChild(2), "SwitchOn", 1.00f, 0.88f, 0.20f);
            AssertSpriteAndColor(elements.GetChild(3), "DoorOpen", 0.85f, 0.65f, 0.95f);
            AssertSpriteAndColor(elements.GetChild(4), "DoorOpen", 0.85f, 0.65f, 0.95f);
        }

        [Test]
        public void R6_Update_BeforeShow_Throws()
        {
            CreateBoardView();
            var session = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            var model = BoardElementsModelBuilder.Build(session);
            Assert.Throws<InvalidOperationException>(() => _boardView.UpdateElements(model));
        }

        [Test]
        public void R7_Update_NullModel_Throws()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.CreateDemo());
            Assert.Throws<ArgumentNullException>(() => _boardView.UpdateElements(null));
        }

        [Test]
        public void R8_Update_CountMismatch_Throws()
        {
            CreateBoardView();
            _boardView.Show(CreateStageA());

            var stageE = CreateStageE();
            var sessionE = new GridPuzzleEngineSession(stageE);
            var model = BoardElementsModelBuilder.Build(sessionE);

            Assert.Throws<ArgumentException>(() => _boardView.UpdateElements(model));

            var elements = _boardView.transform.Find("Elements");
            Assert.That(elements.GetChild(0).GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("ExitClosed"));
            Assert.That(elements.GetChild(1).GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("SwitchOff"));
        }

        [Test]
        public void R9_Update_PositionMismatch_Throws_AndChangesNothing()
        {
            CreateBoardView();
            _boardView.Show(CreateStageA());

            var stageD = CreateStageD();
            var sessionD = new GridPuzzleEngineSession(stageD);
            var dispatcher = new SessionInputDispatcher(sessionD);
            Assert.That(dispatcher.Dispatch(InputIntent.Move(MoveDirection.Up)), Is.EqualTo(InputResult.Executed));
            Assert.That(sessionD.State.Tick, Is.EqualTo(1));
            Assert.That(sessionD.State.PlayerPosition, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(sessionD.State.Mode, Is.EqualTo(GameMode.Recording));

            var model = BoardElementsModelBuilder.Build(sessionD);
            Assert.Throws<ArgumentException>(() => _boardView.UpdateElements(model));

            var elements = _boardView.transform.Find("Elements");
            Assert.That(elements.GetChild(0).GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("ExitClosed"));
            Assert.That(elements.GetChild(1).GetComponent<SpriteRenderer>().sprite.name, Is.EqualTo("SwitchOff"));
        }

        [Test]
        public void R10_Show_Twice_ReplacesElements()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.CreateDemo());
            var elementsBefore = _boardView.transform.Find("Elements");

            _boardView.Show(SampleStageFactory.Create());

            Assert.That(elementsBefore == null, Is.True);
            Assert.That(_boardView.transform.childCount, Is.EqualTo(4));
            Assert.That(_boardView.transform.GetChild(0).name, Is.EqualTo("Floor"));
            Assert.That(_boardView.transform.GetChild(1).name, Is.EqualTo("Walls"));
            Assert.That(_boardView.transform.GetChild(2).name, Is.EqualTo("Actors"));
            Assert.That(_boardView.transform.GetChild(3).name, Is.EqualTo("Elements"));

            var elementsAfter = _boardView.transform.Find("Elements");
            Assert.That(elementsAfter.childCount, Is.EqualTo(1));
            Assert.That(elementsAfter.GetChild(0).name, Is.EqualTo("Exit"));
        }

        [Test]
        public void R11_ReleaseResources_DestroysElementsAndSprites()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.CreateDemo());
            var sessionInitial = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(sessionInitial));

            var elementsTransform = _boardView.transform.Find("Elements");
            var spritesBefore = new Sprite[5];
            var texturesBefore = new Texture2D[5];
            for (var i = 0; i < 5; i++)
            {
                var renderer = elementsTransform.GetChild(i).GetComponent<SpriteRenderer>();
                spritesBefore[i] = renderer.sprite;
                Assert.That(spritesBefore[i] == null, Is.False);
                texturesBefore[i] = spritesBefore[i].texture;
                Assert.That(texturesBefore[i] == null, Is.False);
            }

            var clearedStage = SampleStageFactory.CreateDemo();
            var clearedSession = new GridPuzzleEngineSession(clearedStage);
            var clearedDispatcher = new SessionInputDispatcher(clearedSession);
            Assert.That(clearedDispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.Move(MoveDirection.Right)), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.ConfirmRecording), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));
            Assert.That(clearedDispatcher.Dispatch(InputIntent.StepForward), Is.EqualTo(InputResult.Executed));

            _boardView.UpdateElements(BoardElementsModelBuilder.Build(clearedSession));

            var spritesAfter = new Sprite[5];
            var texturesAfter = new Texture2D[5];
            var elementsAfter = _boardView.transform.Find("Elements");
            for (var i = 0; i < 5; i++)
            {
                var renderer = elementsAfter.GetChild(i).GetComponent<SpriteRenderer>();
                spritesAfter[i] = renderer.sprite;
                texturesAfter[i] = spritesAfter[i].texture;
            }

            _boardView.ReleaseResources();

            Assert.That(elementsTransform == null, Is.True);
            Assert.That(elementsAfter == null, Is.True);
            for (var i = 0; i < 5; i++)
            {
                Assert.That(spritesBefore[i] == null, Is.True);
                Assert.That(texturesBefore[i] == null, Is.True);
                Assert.That(spritesAfter[i] == null, Is.True);
                Assert.That(texturesAfter[i] == null, Is.True);
            }
        }

        private void CreateBoardView()
        {
            _root = new GameObject("BoardRoot");
            _boardView = _root.AddComponent<BoardView>();
        }

        private static void AssertElementRenderer(
            Transform child,
            int sortingOrder,
            float x,
            float y,
            float z,
            string spriteName)
        {
            var renderer = child.GetComponent<SpriteRenderer>();
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.sortingOrder, Is.EqualTo(sortingOrder));
            Assert.That(child.localPosition.x, Is.EqualTo(x));
            Assert.That(child.localPosition.y, Is.EqualTo(y));
            Assert.That(child.localPosition.z, Is.EqualTo(z));
            Assert.That(renderer.sprite.name, Is.EqualTo(spriteName));
        }

        private static void AssertSpriteAndColor(Transform child, string spriteName, float r, float g, float b)
        {
            var renderer = child.GetComponent<SpriteRenderer>();
            Assert.That(renderer.sprite.name, Is.EqualTo(spriteName));
            Assert.That(renderer.color, Is.EqualTo(new Color(r, g, b, 1f)));
        }

        private static IStage CreateStageA()
        {
            var definition = new StageDefinition
            {
                Width = 5,
                Height = 4,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(3, 3)
            };
            definition.Switches.Add(new SwitchPlacement(new SwitchId("A"), new GridPosition(1, 1)));
            var result = StageValidator.Validate(definition);
            Assert.That(result.IsSuccess, Is.True);
            return result.Stage;
        }

        private static IStage CreateStageD()
        {
            var definition = new StageDefinition
            {
                Width = 5,
                Height = 4,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(3, 3)
            };
            definition.Switches.Add(new SwitchPlacement(new SwitchId("A"), new GridPosition(0, 1)));
            var result = StageValidator.Validate(definition);
            Assert.That(result.IsSuccess, Is.True);
            return result.Stage;
        }

        private static IStage CreateStageE()
        {
            var definition = new StageDefinition
            {
                Width = 5,
                Height = 4,
                PlayerPosition = new GridPosition(0, 0),
                SpecterPosition = new GridPosition(1, 0),
                ExitPosition = new GridPosition(3, 3)
            };
            var result = StageValidator.Validate(definition);
            Assert.That(result.IsSuccess, Is.True);
            return result.Stage;
        }
    }
}
