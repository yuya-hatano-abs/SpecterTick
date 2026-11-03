using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpecterTick.Presentation
{
    public sealed class HowToPlayView : MonoBehaviour
    {
        public const float PanelWidth = 1000f;
        public const float PanelHeight = 680f;
        public const float PanelPadding = 12f;
        public const float BlockGap = 4f;
        public const float LegendRowHeight = 40f;
        internal const float LegendSpriteSize = 36f;
        internal const float LegendIconSpacing = LegendSpriteSize + 4f;
        internal const int BodyFontSize = 18;
        internal const int HeadingFontSize = 24;

        private static readonly Color TextColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.85f);
        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.85f);

        private Transform _canvasRoot;
        private Transform _legendRoot;
        private readonly List<Text> _bodyTexts = new List<Text>();
        private readonly List<Transform> _legendRows = new List<Transform>();
        private Text _headingText;
        private Text _continueText;
        private bool _fontWarningLogged;
        private bool _uiBuilt;

        public void Apply(HowToPlayModel model, BoardSkin skin)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            EnsureUiBuilt();
            _headingText.text = model.Heading;
            _bodyTexts[0].text = model.PurposeText;
            _bodyTexts[1].text = model.StepsText;
            _bodyTexts[2].text = model.RulesText;
            _bodyTexts[3].text = model.KeysText;
            _continueText.text = model.ContinuePrompt;
            ApplyLegend(model, skin);
            RefreshVerticalLayout();
        }

        private void RefreshVerticalLayout()
        {
            var y = 0f;
            LayoutBlock(_headingText, ref y);
            for (var i = 0; i < _bodyTexts.Count; i++)
            {
                LayoutBlock(_bodyTexts[i], ref y);
            }

            if (_legendRoot != null)
            {
                var legendRect = _legendRoot.GetComponent<RectTransform>();
                legendRect.anchoredPosition = new Vector2(0f, y);
                var activeRowCount = 0;
                var rowY = 0f;
                for (var i = 0; i < _legendRows.Count; i++)
                {
                    if (!_legendRows[i].gameObject.activeSelf)
                    {
                        continue;
                    }

                    activeRowCount++;
                    var rowRect = _legendRows[i].GetComponent<RectTransform>();
                    rowRect.anchoredPosition = new Vector2(0f, rowY);
                    rowY -= LegendRowHeight;
                }

                y -= activeRowCount * LegendRowHeight;
            }

            LayoutBlock(_continueText, ref y);
        }

        private static void LayoutBlock(Text text, ref float yOffset)
        {
            if (text == null)
            {
                return;
            }

            var rect = text.rectTransform;
            rect.anchoredPosition = new Vector2(0f, yOffset);
            Canvas.ForceUpdateCanvases();
            var height = text.preferredHeight;
            rect.sizeDelta = new Vector2(0f, height);
            yOffset -= height + BlockGap;
        }

        public void SetVisible(bool visible)
        {
            if (_canvasRoot == null)
            {
                return;
            }

            _canvasRoot.gameObject.SetActive(visible);
        }

        public void ReleaseResources()
        {
            if (_canvasRoot != null)
            {
                _canvasRoot.SetParent(null, false);
                DestroyObject(_canvasRoot.gameObject);
                _canvasRoot = null;
                _legendRoot = null;
                _headingText = null;
                _continueText = null;
                _bodyTexts.Clear();
                _legendRows.Clear();
                _uiBuilt = false;
            }
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void ApplyLegend(HowToPlayModel model, BoardSkin skin)
        {
            for (var i = 0; i < _legendRows.Count; i++)
            {
                var row = _legendRows[i];
                if (row == null)
                {
                    continue;
                }

                row.gameObject.SetActive(false);
            }

            for (var i = 0; i < model.LegendEntries.Count; i++)
            {
                Transform row;
                if (i < _legendRows.Count)
                {
                    row = _legendRows[i];
                }
                else
                {
                    row = CreateLegendRow(_legendRoot);
                    _legendRows.Add(row);
                }

                row.gameObject.SetActive(true);
                var title = row.Find("Title").GetComponent<Text>();
                var description = row.Find("Description").GetComponent<Text>();
                var entry = model.LegendEntries[i];
                title.text = entry.Title;
                description.text = entry.Description;

                var icons = row.Find("Icons");
                for (var c = 0; c < icons.childCount; c++)
                {
                    icons.GetChild(c).gameObject.SetActive(false);
                }

                var iconCount = entry.SpriteKeys.Count;
                for (var s = 0; s < iconCount; s++)
                {
                    Image iconImage;
                    if (s < icons.childCount)
                    {
                        iconImage = icons.GetChild(s).GetComponent<Image>();
                    }
                    else
                    {
                        iconImage = CreateLegendIcon(icons);
                    }

                    iconImage.gameObject.SetActive(true);
                    var sprite = ResolveSprite(skin, entry.SpriteKeys[s]);
                    iconImage.sprite = sprite;
                    iconImage.enabled = sprite != null;
                    LayoutLegendIcon(iconImage.rectTransform, s, iconCount);
                }
            }
        }

        private static void LayoutLegendIcon(RectTransform iconRect, int index, int iconCount)
        {
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            var offsetX = (index - (iconCount - 1) / 2f) * LegendIconSpacing;
            iconRect.anchoredPosition = new Vector2(offsetX, 0f);
        }

        private void EnsureUiBuilt()
        {
            if (_uiBuilt)
            {
                return;
            }

            var canvasObject = new GameObject("HowToPlayCanvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var dimObject = new GameObject("HowToPlayDim");
            dimObject.transform.SetParent(canvasObject.transform, false);
            var dimRect = dimObject.AddComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = Vector2.zero;
            dimRect.offsetMax = Vector2.zero;
            var dimImage = dimObject.AddComponent<Image>();
            dimImage.color = DimColor;

            var panelObject = new GameObject("HowToPlayPanel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            var panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            var panelImage = panelObject.AddComponent<Image>();
            panelImage.color = PanelColor;

            var contentObject = new GameObject("HowToPlayContent");
            contentObject.transform.SetParent(panelObject.transform, false);
            var contentRect = contentObject.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(PanelPadding, PanelPadding);
            contentRect.offsetMax = new Vector2(-PanelPadding, -PanelPadding);

            var font = ResolveFont();
            var y = 0f;
            _headingText = CreateTextBlock(contentObject.transform, "HeadingText", font, HeadingFontSize, ref y);
            _bodyTexts.Add(CreateTextBlock(contentObject.transform, "PurposeText", font, BodyFontSize, ref y));
            _bodyTexts.Add(CreateTextBlock(contentObject.transform, "StepsText", font, BodyFontSize, ref y));
            _bodyTexts.Add(CreateTextBlock(contentObject.transform, "RulesText", font, BodyFontSize, ref y));
            _bodyTexts.Add(CreateTextBlock(contentObject.transform, "KeysText", font, BodyFontSize, ref y));
            _continueText = CreateTextBlock(contentObject.transform, "ContinueText", font, BodyFontSize, ref y);

            var legendObject = new GameObject("LegendRows");
            legendObject.transform.SetParent(contentObject.transform, false);
            var legendRect = legendObject.AddComponent<RectTransform>();
            legendRect.anchorMin = new Vector2(0f, 1f);
            legendRect.anchorMax = new Vector2(1f, 1f);
            legendRect.pivot = new Vector2(0f, 1f);
            legendRect.anchoredPosition = new Vector2(0f, y);
            legendRect.sizeDelta = new Vector2(0f, 0f);
            _legendRoot = legendObject.transform;
            for (var i = 0; i < 6; i++)
            {
                var row = CreateLegendRow(_legendRoot);
                _legendRows.Add(row);
            }

            _canvasRoot = canvasObject.transform;
            _uiBuilt = true;
            canvasObject.SetActive(false);
        }

        private Text CreateTextBlock(Transform parent, string name, Font font, int fontSize, ref float yOffset)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(0f, yOffset);
            rect.sizeDelta = new Vector2(0f, 0f);

            var text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = TextColor;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.text = " ";
            Canvas.ForceUpdateCanvases();
            var height = text.preferredHeight;
            rect.sizeDelta = new Vector2(0f, height);
            yOffset -= height + BlockGap;
            return text;
        }

        private Transform CreateLegendRow(Transform parent)
        {
            var rowObject = new GameObject("LegendRow");
            rowObject.transform.SetParent(parent, false);
            var rowRect = rowObject.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0f, 1f);
            rowRect.sizeDelta = new Vector2(0f, LegendRowHeight);

            var iconsObject = new GameObject("Icons");
            iconsObject.transform.SetParent(rowObject.transform, false);
            var iconsRect = iconsObject.AddComponent<RectTransform>();
            iconsRect.anchorMin = new Vector2(0f, 0.5f);
            iconsRect.anchorMax = new Vector2(0f, 0.5f);
            iconsRect.pivot = new Vector2(0f, 0.5f);
            iconsRect.anchoredPosition = Vector2.zero;
            iconsRect.sizeDelta = new Vector2(LegendSpriteSize * 2f + 4f, LegendSpriteSize);

            var titleObject = new GameObject("Title");
            titleObject.transform.SetParent(rowObject.transform, false);
            var titleRect = titleObject.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.offsetMin = new Vector2(LegendSpriteSize * 2f + 12f, -18f);
            titleRect.offsetMax = new Vector2(0f, 0f);
            var titleText = titleObject.AddComponent<Text>();
            titleText.font = ResolveFont();
            titleText.fontSize = BodyFontSize;
            titleText.color = TextColor;
            titleText.alignment = TextAnchor.UpperLeft;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleText.verticalOverflow = VerticalWrapMode.Overflow;

            var descriptionObject = new GameObject("Description");
            descriptionObject.transform.SetParent(rowObject.transform, false);
            var descriptionRect = descriptionObject.AddComponent<RectTransform>();
            descriptionRect.anchorMin = new Vector2(0f, 1f);
            descriptionRect.anchorMax = new Vector2(1f, 1f);
            descriptionRect.pivot = new Vector2(0f, 1f);
            descriptionRect.offsetMin = new Vector2(LegendSpriteSize * 2f + 12f, -38f);
            descriptionRect.offsetMax = new Vector2(0f, -20f);
            var descriptionText = descriptionObject.AddComponent<Text>();
            descriptionText.font = ResolveFont();
            descriptionText.fontSize = BodyFontSize;
            descriptionText.color = TextColor;
            descriptionText.alignment = TextAnchor.UpperLeft;
            descriptionText.horizontalOverflow = HorizontalWrapMode.Overflow;
            descriptionText.verticalOverflow = VerticalWrapMode.Overflow;

            return rowObject.transform;
        }

        private static Image CreateLegendIcon(Transform icons)
        {
            var iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(icons, false);
            var iconRect = iconObject.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(LegendSpriteSize, LegendSpriteSize);
            var image = iconObject.AddComponent<Image>();
            image.preserveAspect = true;
            return image;
        }

        private static Sprite ResolveSprite(BoardSkin skin, string key)
        {
            if (skin == null || string.IsNullOrEmpty(key))
            {
                return null;
            }

            switch (key)
            {
                case "Floor":
                    return skin.Floor;
                case "Wall":
                    return skin.Wall;
                case "Player":
                    return skin.Player;
                case "Specter":
                    return skin.Specter;
                case "ExitClosed":
                    return skin.ExitClosed;
                case "ExitOpen":
                    return skin.ExitOpen;
                case "SwitchOff":
                    return skin.SwitchOff;
                case "SwitchOn":
                    return skin.SwitchOn;
                case "DoorClosed":
                    return skin.DoorClosed;
                case "DoorOpen":
                    return skin.DoorOpen;
                default:
                    return null;
            }
        }

        private Font ResolveFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null && !_fontWarningLogged)
            {
                Debug.LogWarning("HowToPlayView: LegacyRuntime.ttf builtin font was not found; help text will not render.");
                _fontWarningLogged = true;
            }

            return font;
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
