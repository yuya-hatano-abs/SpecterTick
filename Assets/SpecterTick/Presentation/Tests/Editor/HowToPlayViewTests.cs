using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpecterTick.Presentation;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpecterTick.Presentation.Tests
{
    /// <summary>
    /// HowToPlayView uses CanvasScaler.ScreenMatchMode.Expand (not matchWidthOrHeight 0.5 like HUD, label, clear).
    /// </summary>
    [TestFixture]
    public sealed class HowToPlayViewTests
    {
        private GameObject _root;
        private HowToPlayView _view;
        private readonly List<Texture2D> _textures = new List<Texture2D>();
        private BoardSkin _ownedSkin;

        [TearDown]
        public void TearDown()
        {
            if (_view != null)
            {
                _view.ReleaseResources();
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

            _view = null;
            _root = null;
        }

        [Test]
        public void HV1_Apply_UsesExpandScaler_UnlikeHudLabelClear()
        {
            CreateView();
            _view.Apply(HowToPlayModelBuilder.Build(), null);

            var scaler = _root.GetComponentInChildren<CanvasScaler>(true);
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.referenceResolution.x, Is.EqualTo(BoardScreenInsets.ReferenceScreenWidth));
            Assert.That(scaler.referenceResolution.y, Is.EqualTo(BoardScreenInsets.ReferenceScreenHeight));
            Assert.That(scaler.screenMatchMode, Is.EqualTo(CanvasScaler.ScreenMatchMode.Expand));
        }

        [Test]
        public void HV2_Apply_PanelFitsReferenceCanvasRect()
        {
            CreateView();
            _view.Apply(HowToPlayModelBuilder.Build(), null);

            var panel = FindPanelRect();
            Assert.That(panel.sizeDelta.x, Is.LessThanOrEqualTo(BoardScreenInsets.ReferenceScreenWidth));
            Assert.That(panel.sizeDelta.y, Is.LessThanOrEqualTo(BoardScreenInsets.ReferenceScreenHeight));
            Assert.That(panel.sizeDelta.x, Is.EqualTo(HowToPlayView.PanelWidth));
            Assert.That(panel.sizeDelta.y, Is.EqualTo(HowToPlayView.PanelHeight));
        }

        [Test]
        public void HV3_Apply_TextBlocksFitAssignedHeights()
        {
            CreateView();
            _view.Apply(HowToPlayModelBuilder.Build(), null);
            Canvas.ForceUpdateCanvases();

            var innerWidth = HowToPlayView.PanelWidth - HowToPlayView.PanelPadding * 2f;
            AssertTextBlock("HeadingText", 24, 27f, innerWidth);
            AssertTextBlock("PurposeText", 18, 41f, innerWidth);
            AssertTextBlock("StepsText", 18, 62f, innerWidth);
            AssertTextBlock("RulesText", 18, 62f, innerWidth);
            AssertTextBlock("KeysText", 18, 104f, innerWidth);
            AssertTextBlock("ContinueText", 18, 20f, innerWidth);
        }

        [Test]
        public void HV4_SetVisible_TogglesActive_ApplyTwice_OneCanvas()
        {
            CreateView();
            _view.Apply(HowToPlayModelBuilder.Build(), null);
            _view.SetVisible(true);
            Assert.That(_root.GetComponentInChildren<Canvas>(true).gameObject.activeSelf, Is.True);
            _view.SetVisible(false);
            Assert.That(_root.GetComponentInChildren<Canvas>(true).gameObject.activeSelf, Is.False);

            _view.Apply(HowToPlayModelBuilder.Build(), null);
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1));
        }

        [Test]
        public void HV5_Apply_SyntheticSkin_LegendSpriteImageCount_NullSkinZero()
        {
            CreateView();
            var skin = CreateSyntheticSkin();
            _view.Apply(HowToPlayModelBuilder.Build(), skin);
            Assert.That(CountLegendSprites(), Is.EqualTo(9));

            _view.ReleaseResources();
            CreateViewOnExistingRoot();
            _view.Apply(HowToPlayModelBuilder.Build(), null);
            Assert.That(CountLegendSprites(), Is.EqualTo(0));
        }

        [Test]
        public void HV6_TextPanelContrast_AtLeastFourPointFiveOnWhiteBackdrop()
        {
            CreateView();
            _view.Apply(HowToPlayModelBuilder.Build(), null);

            var heading = FindText("HeadingText");
            var panelImage = FindPanelRect().GetComponent<Image>();
            var ratio = TextOverPanelContrast(heading.color, panelImage.color, Color.white);
            Assert.That(ratio, Is.GreaterThanOrEqualTo(4.5f));
        }

        [Test]
        public void HV7_ReleaseResources_DestroysOwnedUi_NotBorrowedSkinSprites()
        {
            var skin = CreateSyntheticSkin();
            var spritesBefore = CollectSkinSprites(skin);
            CreateView();
            _view.Apply(HowToPlayModelBuilder.Build(), skin);
            var canvasBefore = _root.GetComponentInChildren<Canvas>(true);

            _view.ReleaseResources();
            Assert.That(_root.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(0));
            Assert.That(canvasBefore == null, Is.True);

            for (var i = 0; i < spritesBefore.Length; i++)
            {
                Assert.That(spritesBefore[i] == null, Is.False);
            }
        }

        [Test]
        public void HV8_Apply_AllBlocksInsidePanel_LegendRowsOrdered_HeightBudget()
        {
            CreateView();
            _view.Apply(HowToPlayModelBuilder.Build(), CreateSyntheticSkin());
            Canvas.ForceUpdateCanvases();

            var panel = FindPanelRect();
            var panelBounds = panel.rect;

            AssertInsidePanel(panel, FindTextRect("HeadingText"), panelBounds);
            AssertInsidePanel(panel, FindTextRect("PurposeText"), panelBounds);
            AssertInsidePanel(panel, FindTextRect("StepsText"), panelBounds);
            AssertInsidePanel(panel, FindTextRect("RulesText"), panelBounds);
            AssertInsidePanel(panel, FindTextRect("KeysText"), panelBounds);

            var legend = _root.transform.Find("HowToPlayCanvas/HowToPlayPanel/HowToPlayContent/LegendRows");
            Assert.That(legend, Is.Not.Null);
            AssertInsidePanel(panel, legend.GetComponent<RectTransform>(), panelBounds);

            var activeRows = new List<RectTransform>();
            for (var i = 0; i < legend.childCount; i++)
            {
                var row = legend.GetChild(i);
                if (!row.gameObject.activeSelf)
                {
                    continue;
                }

                var rowRect = row.GetComponent<RectTransform>();
                activeRows.Add(rowRect);
                AssertInsidePanel(panel, rowRect, panelBounds);
            }

            Assert.That(activeRows.Count, Is.EqualTo(6));
            for (var i = 0; i < activeRows.Count - 1; i++)
            {
                var upper = LocalBounds(panel, activeRows[i]);
                var lower = LocalBounds(panel, activeRows[i + 1]);
                Assert.That(upper.yMin, Is.GreaterThanOrEqualTo(lower.yMax - 0.5f));
            }

            var textHeightSum = 0f;
            textHeightSum += FindTextRect("HeadingText").rect.height;
            textHeightSum += FindTextRect("PurposeText").rect.height;
            textHeightSum += FindTextRect("StepsText").rect.height;
            textHeightSum += FindTextRect("RulesText").rect.height;
            textHeightSum += FindTextRect("KeysText").rect.height;
            textHeightSum += HowToPlayView.BlockGap * 5f;
            var legendHeight = HowToPlayView.LegendRowHeight * activeRows.Count;
            textHeightSum += FindTextRect("ContinueText").rect.height;
            var innerHeight = HowToPlayView.PanelHeight - HowToPlayView.PanelPadding * 2f;
            Assert.That(textHeightSum + legendHeight, Is.LessThanOrEqualTo(innerHeight + 0.5f));

            var continueBounds = LocalBounds(panel, FindTextRect("ContinueText"));
            var lastRowBounds = LocalBounds(panel, activeRows[activeRows.Count - 1]);
            Assert.That(continueBounds.yMax, Is.LessThanOrEqualTo(lastRowBounds.yMin + 0.5f));
            AssertInsidePanel(panel, FindTextRect("ContinueText"), panelBounds);
        }

        [Test]
        public void HV9_Apply_LegendIconsSeparated_TitleDescriptionContinueLayout()
        {
            CreateView();
            _view.Apply(HowToPlayModelBuilder.Build(), CreateSyntheticSkin());
            Canvas.ForceUpdateCanvases();

            var panel = FindPanelRect();
            var panelBounds = panel.rect;
            var legend = _root.transform.Find("HowToPlayCanvas/HowToPlayPanel/HowToPlayContent/LegendRows");
            Assert.That(legend, Is.Not.Null);

            var expectedIcons = new[] { 1, 1, 1, 2, 2, 2 };
            var activeRowIndex = 0;
            RectTransform lastRowRect = null;
            for (var i = 0; i < legend.childCount; i++)
            {
                var row = legend.GetChild(i);
                if (!row.gameObject.activeSelf)
                {
                    continue;
                }

                var rowRect = row.GetComponent<RectTransform>();
                lastRowRect = rowRect;
                AssertInsidePanel(panel, rowRect, panelBounds);

                var titleRect = row.Find("Title").GetComponent<RectTransform>();
                var descriptionRect = row.Find("Description").GetComponent<RectTransform>();
                var titleBounds = LocalBounds(rowRect, titleRect);
                var descriptionBounds = LocalBounds(rowRect, descriptionRect);
                Assert.That(titleBounds.yMin, Is.GreaterThanOrEqualTo(descriptionBounds.yMax - 0.5f));

                var icons = row.Find("Icons");
                var enabledIcons = new List<RectTransform>();
                for (var c = 0; c < icons.childCount; c++)
                {
                    var iconImage = icons.GetChild(c).GetComponent<Image>();
                    if (!iconImage.gameObject.activeSelf || !iconImage.enabled)
                    {
                        continue;
                    }

                    enabledIcons.Add(iconImage.rectTransform);
                }

                Assert.That(enabledIcons.Count, Is.EqualTo(expectedIcons[activeRowIndex]));
                for (var a = 0; a < enabledIcons.Count; a++)
                {
                    AssertInsidePanel(panel, enabledIcons[a], panelBounds);
                    var iconBounds = LocalBounds(rowRect, enabledIcons[a]);
                    for (var b = a + 1; b < enabledIcons.Count; b++)
                    {
                        var otherBounds = LocalBounds(rowRect, enabledIcons[b]);
                        Assert.That(RectsOverlap(iconBounds, otherBounds), Is.False);
                    }
                }

                activeRowIndex++;
            }

            Assert.That(activeRowIndex, Is.EqualTo(6));
            Assert.That(lastRowRect, Is.Not.Null);
            var continueBounds = LocalBounds(panel, FindTextRect("ContinueText"));
            var lastRowPanelBounds = LocalBounds(panel, lastRowRect);
            Assert.That(continueBounds.yMax, Is.LessThanOrEqualTo(lastRowPanelBounds.yMin + 0.5f));
            AssertInsidePanel(panel, FindTextRect("ContinueText"), panelBounds);

            var textHeightSum = 0f;
            textHeightSum += FindTextRect("HeadingText").rect.height;
            textHeightSum += FindTextRect("PurposeText").rect.height;
            textHeightSum += FindTextRect("StepsText").rect.height;
            textHeightSum += FindTextRect("RulesText").rect.height;
            textHeightSum += FindTextRect("KeysText").rect.height;
            textHeightSum += HowToPlayView.BlockGap * 5f;
            textHeightSum += FindTextRect("ContinueText").rect.height;
            var legendHeight = HowToPlayView.LegendRowHeight * activeRowIndex;
            var innerHeight = HowToPlayView.PanelHeight - HowToPlayView.PanelPadding * 2f;
            Assert.That(textHeightSum + legendHeight, Is.LessThanOrEqualTo(innerHeight + 0.5f));
        }

        private static bool RectsOverlap(Rect a, Rect b)
        {
            if (a.xMax <= b.xMin + 0.5f || b.xMax <= a.xMin + 0.5f)
            {
                return false;
            }

            if (a.yMax <= b.yMin + 0.5f || b.yMax <= a.yMin + 0.5f)
            {
                return false;
            }

            return true;
        }

        private void AssertTextBlock(string objectName, int fontSize, float expectedHeight, float innerWidth)
        {
            var text = FindText(objectName);
            var rect = text.rectTransform;
            Assert.That(text.fontSize, Is.EqualTo(fontSize));
            Assert.That(text.horizontalOverflow, Is.EqualTo(HorizontalWrapMode.Overflow));
            Assert.That(text.verticalOverflow, Is.EqualTo(VerticalWrapMode.Overflow));
            Assert.That(rect.sizeDelta.y, Is.EqualTo(expectedHeight).Within(1f));
            Assert.That(text.preferredHeight, Is.LessThanOrEqualTo(rect.rect.height + 0.5f));
            Assert.That(text.preferredWidth, Is.LessThanOrEqualTo(innerWidth + 0.5f));
        }

        private Text FindText(string objectName)
        {
            return FindTextRect(objectName).GetComponent<Text>();
        }

        private RectTransform FindTextRect(string objectName)
        {
            var textObject = _root.transform.Find(
                "HowToPlayCanvas/HowToPlayPanel/HowToPlayContent/" + objectName);
            Assert.That(textObject, Is.Not.Null);
            return textObject.GetComponent<RectTransform>();
        }

        private static void AssertInsidePanel(RectTransform panel, RectTransform child, Rect panelBounds)
        {
            var bounds = LocalBounds(panel, child);
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(panelBounds.xMin - 0.5f));
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(panelBounds.xMax + 0.5f));
            Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(panelBounds.yMin - 0.5f));
            Assert.That(bounds.yMax, Is.LessThanOrEqualTo(panelBounds.yMax + 0.5f));
        }

        private static Rect LocalBounds(RectTransform panel, RectTransform child)
        {
            var corners = new Vector3[4];
            child.GetWorldCorners(corners);
            var min = panel.InverseTransformPoint(corners[0]);
            var max = min;
            for (var i = 1; i < corners.Length; i++)
            {
                var local = panel.InverseTransformPoint(corners[i]);
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private RectTransform FindPanelRect()
        {
            var panel = _root.transform.Find("HowToPlayCanvas/HowToPlayPanel");
            Assert.That(panel, Is.Not.Null);
            return panel.GetComponent<RectTransform>();
        }

        private int CountLegendSprites()
        {
            var legend = _root.transform.Find("HowToPlayCanvas/HowToPlayPanel/HowToPlayContent/LegendRows");
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

        private void CreateView()
        {
            _root = new GameObject("HowToPlayViewTestsRoot");
            _view = _root.AddComponent<HowToPlayView>();
        }

        private void CreateViewOnExistingRoot()
        {
            _view = _root.AddComponent<HowToPlayView>();
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

        private static float TextOverPanelContrast(Color text, Color panel, Color backdrop)
        {
            var blendedR = ColorContrast.Blend(panel.r, panel.a, backdrop.r);
            var blendedG = ColorContrast.Blend(panel.g, panel.a, backdrop.g);
            var blendedB = ColorContrast.Blend(panel.b, panel.a, backdrop.b);
            var panelL = ColorContrast.RelativeLuminance(blendedR, blendedG, blendedB);
            var textL = ColorContrast.RelativeLuminance(text.r, text.g, text.b);
            return ColorContrast.ContrastRatio(textL, panelL);
        }
    }
}
