using System;
using NUnit.Framework;
using SpecterTick.Core;
using SpecterTick.Presentation;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// Characterization: fixes contrast ratios for the current palette. Update intentionally when the palette changes.
    /// Known limits (Task 16b): Player vs Specter luminance contrast ~1.13 (hue-only distinction);
    /// Specter vs floor ~2.80 (below WCAG 3:1 for non-text). Does not guarantee on-screen readability.
    /// </summary>
    [TestFixture]
    public sealed class PaletteContrastTests
    {
        private const float Tolerance = 0.02f;

        private GameObject _root;
        private BoardView _boardView;
        private GameObject _hudRoot;
        private HudView _hudView;
        private GameObject _clearRoot;
        private ClearScreenView _clearScreenView;
        private GameObject _labelRoot;
        private StageLabelView _stageLabelView;

        [TearDown]
        public void TearDown()
        {
            if (_boardView != null)
            {
                _boardView.ReleaseResources();
            }

            if (_hudView != null)
            {
                _hudView.ReleaseResources();
            }

            if (_clearScreenView != null)
            {
                _clearScreenView.ReleaseResources();
            }

            if (_stageLabelView != null)
            {
                _stageLabelView.ReleaseResources();
            }

            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            if (_hudRoot != null)
            {
                Object.DestroyImmediate(_hudRoot);
            }

            if (_clearRoot != null)
            {
                Object.DestroyImmediate(_clearRoot);
            }

            if (_labelRoot != null)
            {
                Object.DestroyImmediate(_labelRoot);
            }

            _boardView = null;
            _root = null;
            _hudView = null;
            _hudRoot = null;
            _clearScreenView = null;
            _clearRoot = null;
            _stageLabelView = null;
            _labelRoot = null;
        }

        [Test]
        public void PC1_Actors_And_Wall_Vs_Floor()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.Create());

            var floor = GetFloorColor();
            var specter = GetActorColor(BoardView.SpecterActorName);
            var player = GetActorColor(BoardView.PlayerActorName);
            var wall = GetFirstWallColor();

            Assert.That(Contrast(floor, specter), Is.EqualTo(2.80f).Within(Tolerance));
            Assert.That(Contrast(floor, player), Is.EqualTo(3.17f).Within(Tolerance));
            Assert.That(Contrast(floor, wall), Is.EqualTo(3.27f).Within(Tolerance));
        }

        [Test]
        public void PC2_Elements_Vs_Floor()
        {
            var stage = SampleStageFactory.CreateDemo();

            CreateBoardView();
            _boardView.Show(stage);

            var floor = GetFloorColor();
            var sessionInitial = new GridPuzzleEngineSession(stage);
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(sessionInitial));
            var elements = _boardView.transform.Find(BoardView.ElementsContainerName);

            Assert.That(Contrast(floor, elements.GetChild(0).GetComponent<SpriteRenderer>().color), Is.EqualTo(4.72f).Within(Tolerance));
            Assert.That(Contrast(floor, elements.GetChild(1).GetComponent<SpriteRenderer>().color), Is.EqualTo(3.89f).Within(Tolerance));
            Assert.That(Contrast(floor, elements.GetChild(2).GetComponent<SpriteRenderer>().color), Is.EqualTo(3.89f).Within(Tolerance));
            Assert.That(Contrast(floor, elements.GetChild(3).GetComponent<SpriteRenderer>().color), Is.EqualTo(3.17f).Within(Tolerance));
            Assert.That(Contrast(floor, elements.GetChild(4).GetComponent<SpriteRenderer>().color), Is.EqualTo(3.17f).Within(Tolerance));

            var sessionCleared = BuildDemoClearedSession();
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(sessionCleared));

            Assert.That(Contrast(floor, elements.GetChild(0).GetComponent<SpriteRenderer>().color), Is.EqualTo(6.10f).Within(Tolerance));
            Assert.That(Contrast(floor, elements.GetChild(1).GetComponent<SpriteRenderer>().color), Is.EqualTo(8.28f).Within(Tolerance));
            Assert.That(Contrast(floor, elements.GetChild(2).GetComponent<SpriteRenderer>().color), Is.EqualTo(8.28f).Within(Tolerance));
            Assert.That(Contrast(floor, elements.GetChild(3).GetComponent<SpriteRenderer>().color), Is.EqualTo(5.52f).Within(Tolerance));
            Assert.That(Contrast(floor, elements.GetChild(4).GetComponent<SpriteRenderer>().color), Is.EqualTo(5.52f).Within(Tolerance));
        }

        [Test]
        public void PC3_PairRatios()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.Create());

            var player = GetActorColor(BoardView.PlayerActorName);
            var specter = GetActorColor(BoardView.SpecterActorName);
            Assert.That(Contrast(specter, player), Is.EqualTo(1.13f).Within(Tolerance));

            _boardView.ReleaseResources();
            Object.DestroyImmediate(_root);
            _boardView = null;
            _root = null;

            CreateBoardView();
            _boardView.Show(SampleStageFactory.CreateDemo());
            var sessionInitial = new GridPuzzleEngineSession(SampleStageFactory.CreateDemo());
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(sessionInitial));
            var elements = _boardView.transform.Find(BoardView.ElementsContainerName);

            var exitClosed = elements.GetChild(0).GetComponent<SpriteRenderer>().color;
            var switchOff = elements.GetChild(1).GetComponent<SpriteRenderer>().color;
            var doorClosed = elements.GetChild(3).GetComponent<SpriteRenderer>().color;

            var sessionCleared = BuildDemoClearedSession();
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(sessionCleared));

            var exitOpen = elements.GetChild(0).GetComponent<SpriteRenderer>().color;
            var switchOn = elements.GetChild(1).GetComponent<SpriteRenderer>().color;
            var doorOpen = elements.GetChild(3).GetComponent<SpriteRenderer>().color;

            // State pairs also differ by sprite shape (fill vs outline), not luminance alone.
            Assert.That(Contrast(exitClosed, exitOpen), Is.EqualTo(1.29f).Within(Tolerance));
            Assert.That(Contrast(switchOff, switchOn), Is.EqualTo(2.13f).Within(Tolerance));
            Assert.That(Contrast(doorClosed, doorOpen), Is.EqualTo(1.74f).Within(Tolerance));
        }

        [Test]
        public void PC4_Text_Vs_Panel_OverFloor()
        {
            var floor = GetFloorColorFromBoard();

            var hudPanel = GetHudPanelColor();
            var hudText = GetHudTextColor();
            Assert.That(TextOverPanelContrast(hudText, hudPanel, floor), Is.EqualTo(15.84f).Within(Tolerance));

            var labelPanel = GetStageLabelPanelColor();
            var labelText = GetStageLabelTextColor();
            Assert.That(TextOverPanelContrast(labelText, labelPanel, floor), Is.EqualTo(15.84f).Within(Tolerance));

            var clearPanel = GetClearScreenPanelColor();
            var clearText = GetClearScreenTextColor();
            Assert.That(TextOverPanelContrast(clearText, clearPanel, floor), Is.EqualTo(16.73f).Within(Tolerance));
        }

        [Test]
        public void PC5_Text_Vs_Panel_WorstCaseWhite()
        {
            var white = new Color(1f, 1f, 1f, 1f);

            var hudPanel = GetHudPanelColor();
            var hudText = GetHudTextColor();
            var hudRatio = TextOverPanelContrast(hudText, hudPanel, white);
            Assert.That(hudRatio, Is.EqualTo(5.14f).Within(Tolerance));
            Assert.That(hudRatio, Is.GreaterThanOrEqualTo(4.5f));

            var labelPanel = GetStageLabelPanelColor();
            var labelText = GetStageLabelTextColor();
            var labelRatio = TextOverPanelContrast(labelText, labelPanel, white);
            Assert.That(labelRatio, Is.EqualTo(5.14f).Within(Tolerance));
            Assert.That(labelRatio, Is.GreaterThanOrEqualTo(4.5f));

            var clearPanel = GetClearScreenPanelColor();
            var clearText = GetClearScreenTextColor();
            var clearRatio = TextOverPanelContrast(clearText, clearPanel, white);
            Assert.That(clearRatio, Is.EqualTo(7.63f).Within(Tolerance));
            Assert.That(clearRatio, Is.GreaterThanOrEqualTo(4.5f));
        }

        private static GridPuzzleEngineSession BuildDemoClearedSession()
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

        private void CreateBoardView()
        {
            _root = new GameObject("PaletteContrastTestsRoot");
            _boardView = _root.AddComponent<BoardView>();
        }

        private Color GetFloorColor()
        {
            var floor = _boardView.transform.Find(BoardView.FloorContainerName);
            return floor.GetChild(0).GetComponent<SpriteRenderer>().color;
        }

        private Color GetFloorColorFromBoard()
        {
            CreateBoardView();
            _boardView.Show(SampleStageFactory.Create());
            return GetFloorColor();
        }

        private Color GetActorColor(string actorName)
        {
            var actor = _boardView.transform.Find(BoardView.ActorsContainerName + "/" + actorName);
            return actor.GetComponent<SpriteRenderer>().color;
        }

        private Color GetFirstWallColor()
        {
            var walls = _boardView.transform.Find(BoardView.WallsContainerName);
            return walls.GetChild(0).GetComponent<SpriteRenderer>().color;
        }

        private static float Contrast(Color a, Color b)
        {
            var luminanceA = ColorContrast.RelativeLuminance(a.r, a.g, a.b);
            var luminanceB = ColorContrast.RelativeLuminance(b.r, b.g, b.b);
            return ColorContrast.ContrastRatio(luminanceA, luminanceB);
        }

        private static float TextOverPanelContrast(Color text, Color panel, Color backdrop)
        {
            var panelR = ColorContrast.Blend(0f, panel.a, backdrop.r);
            var panelG = ColorContrast.Blend(0f, panel.a, backdrop.g);
            var panelB = ColorContrast.Blend(0f, panel.a, backdrop.b);
            var panelLuminance = ColorContrast.RelativeLuminance(panelR, panelG, panelB);
            var textLuminance = ColorContrast.RelativeLuminance(text.r, text.g, text.b);
            return ColorContrast.ContrastRatio(textLuminance, panelLuminance);
        }

        private Color GetHudPanelColor()
        {
            _hudRoot = new GameObject("PaletteContrastHudRoot");
            _hudView = _hudRoot.AddComponent<HudView>();
            _hudView.Apply(HudModelBuilder.Build(GameMode.Recording, 0, 0, Array.Empty<MoveDirection>()));
            return _hudRoot.GetComponentInChildren<Image>(true).color;
        }

        private Color GetHudTextColor()
        {
            return _hudRoot.GetComponentInChildren<Text>(true).color;
        }

        private Color GetStageLabelPanelColor()
        {
            _labelRoot = new GameObject("PaletteContrastLabelRoot");
            _stageLabelView = _labelRoot.AddComponent<StageLabelView>();
            _stageLabelView.Apply(StageLabelModelBuilder.Build(0, 5, "WALK"));
            return _labelRoot.GetComponentInChildren<Image>(true).color;
        }

        private Color GetStageLabelTextColor()
        {
            return _labelRoot.GetComponentInChildren<Text>(true).color;
        }

        private Color GetClearScreenPanelColor()
        {
            _clearRoot = new GameObject("PaletteContrastClearRoot");
            _clearScreenView = _clearRoot.AddComponent<ClearScreenView>();
            _clearScreenView.Apply(ClearScreenModelBuilder.Build(GameMode.Cleared, 4, 5));
            return _clearRoot.GetComponentInChildren<Image>(true).color;
        }

        private Color GetClearScreenTextColor()
        {
            return _clearRoot.GetComponentInChildren<Text>(true).color;
        }
    }
}
