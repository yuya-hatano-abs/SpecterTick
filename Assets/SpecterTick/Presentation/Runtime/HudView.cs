using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpecterTick.Presentation
{
    public sealed class HudView : MonoBehaviour
    {
        private const float ReferenceFontSize = 20f;
        private const float PanelWidth = 600f;
        private const float PanelHeight = 160f;
        private const float PanelPadding = 12f;
        private const float PanelOffsetX = 16f;
        private const float PanelOffsetY = -16f;

        private static readonly Color TextColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.6f);

        private Transform _canvasRoot;
        private Text _text;
        private bool _fontWarningLogged;

        public void Apply(HudModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            EnsureUiBuilt();
            _text.text = model.ToDisplayText();
        }

        public void ReleaseResources()
        {
            if (_canvasRoot != null)
            {
                _canvasRoot.SetParent(null, false);
                DestroyObject(_canvasRoot.gameObject);
                _canvasRoot = null;
                _text = null;
            }
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void EnsureUiBuilt()
        {
            if (_canvasRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("HudCanvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var panelObject = new GameObject("HudPanel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            var panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(PanelOffsetX, PanelOffsetY);
            panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            var panelImage = panelObject.AddComponent<Image>();
            panelImage.color = PanelColor;

            var textObject = new GameObject("HudText");
            textObject.transform.SetParent(panelObject.transform, false);
            var textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(PanelPadding, PanelPadding);
            textRect.offsetMax = new Vector2(-PanelPadding, -PanelPadding);

            var text = textObject.AddComponent<Text>();
            text.font = ResolveFont();
            text.fontSize = (int)ReferenceFontSize;
            text.color = TextColor;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;

            _canvasRoot = canvasObject.transform;
            _text = text;
        }

        private Font ResolveFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null && !_fontWarningLogged)
            {
                Debug.LogWarning("HudView: LegacyRuntime.ttf builtin font was not found; HUD text will not render.");
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
