using System;
using System.Collections.Generic;
using System.Text;
using SpecterTick.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace SpecterTick.Presentation.PlayModeTests
{
    internal static class PlayModeHarness
    {
        public static Camera CreateMainCamera()
        {
            var cameraObject = new GameObject("TestCamera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        public static GameObject CreateBoard()
        {
            var board = new GameObject("Board");
            board.AddComponent<BoardSessionDriver>();
            return board;
        }

        public static string CanvasText(GameObject board, int sortingOrder)
        {
            if (board == null)
            {
                return null;
            }

            var canvases = board.GetComponentsInChildren<Canvas>(true);
            Canvas target = null;
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (canvas.isRootCanvas && canvas.sortingOrder == sortingOrder)
                {
                    target = canvas;
                    break;
                }
            }

            if (target == null)
            {
                return null;
            }

            var texts = target.GetComponentsInChildren<Text>(true);
            var builder = new StringBuilder();
            for (var i = 0; i < texts.Length; i++)
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(texts[i].text);
            }

            return builder.ToString();
        }

        public static bool IsClearScreenVisible(GameObject board)
        {
            if (board == null)
            {
                return false;
            }

            var canvases = board.GetComponentsInChildren<Canvas>(true);
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (!canvas.isRootCanvas || canvas.sortingOrder != 110)
                {
                    continue;
                }

                if (!canvas.gameObject.activeInHierarchy)
                {
                    return false;
                }

                var texts = canvas.GetComponentsInChildren<Text>(true);
                for (var t = 0; t < texts.Length; t++)
                {
                    if (texts[t].text.Contains("CLEARED!"))
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        public static int ChildCount(GameObject board, string containerName)
        {
            if (board == null)
            {
                return -1;
            }

            var container = board.transform.Find(containerName);
            return container == null ? -1 : container.childCount;
        }

        public static Transform FindActor(GameObject board, string actorName)
        {
            if (board == null)
            {
                return null;
            }

            var actors = board.transform.Find("Actors");
            return actors == null ? null : actors.Find(actorName);
        }

        public static bool ScreenRectsOverlap(ScreenRect a, ScreenRect b)
        {
            if (a.Right <= b.Left || a.Left >= b.Right)
            {
                return false;
            }

            if (a.Top <= b.Bottom || a.Bottom >= b.Top)
            {
                return false;
            }

            return true;
        }

        public static string FormatScreenRect(ScreenRect rect)
        {
            return "L="
                + rect.Left.ToString("F1")
                + " R="
                + rect.Right.ToString("F1")
                + " B="
                + rect.Bottom.ToString("F1")
                + " T="
                + rect.Top.ToString("F1");
        }

        public static bool TryGetPanelScreenRect(GameObject board, int sortingOrder, out ScreenRect rect)
        {
            rect = default;
            if (board == null)
            {
                return false;
            }

            var canvases = board.GetComponentsInChildren<Canvas>(true);
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (!canvas.isRootCanvas || canvas.sortingOrder != sortingOrder)
                {
                    continue;
                }

                Canvas.ForceUpdateCanvases();
                var images = canvas.GetComponentsInChildren<Image>(true);
                if (images.Length == 0)
                {
                    return false;
                }

                var corners = new Vector3[4];
                images[0].rectTransform.GetWorldCorners(corners);
                var left = corners[0].x;
                var right = corners[0].x;
                var bottom = corners[0].y;
                var top = corners[0].y;
                for (var c = 1; c < corners.Length; c++)
                {
                    left = Math.Min(left, corners[c].x);
                    right = Math.Max(right, corners[c].x);
                    bottom = Math.Min(bottom, corners[c].y);
                    top = Math.Max(top, corners[c].y);
                }

                rect = new ScreenRect
                {
                    Left = left,
                    Right = right,
                    Bottom = bottom,
                    Top = top
                };
                return true;
            }

            return false;
        }

        public static bool TryGetFloorScreenBounds(GameObject board, Camera camera, out ScreenRect rect)
        {
            rect = default;
            if (board == null || camera == null)
            {
                return false;
            }

            var floor = board.transform.Find(BoardView.FloorContainerName);
            if (floor == null || floor.childCount == 0)
            {
                return false;
            }

            var left = float.PositiveInfinity;
            var right = float.NegativeInfinity;
            var bottom = float.PositiveInfinity;
            var top = float.NegativeInfinity;
            for (var i = 0; i < floor.childCount; i++)
            {
                var world = floor.GetChild(i).position;
                var half = 0.5f;
                var bl = camera.WorldToScreenPoint(new Vector3(world.x - half, world.y - half, world.z));
                var tr = camera.WorldToScreenPoint(new Vector3(world.x + half, world.y + half, world.z));
                left = Math.Min(left, Math.Min(bl.x, tr.x));
                right = Math.Max(right, Math.Max(bl.x, tr.x));
                bottom = Math.Min(bottom, Math.Min(bl.y, tr.y));
                top = Math.Max(top, Math.Max(bl.y, tr.y));
            }

            rect = new ScreenRect
            {
                Left = left,
                Right = right,
                Bottom = bottom,
                Top = top
            };
            return true;
        }

        public static bool TryGetFloorViewportBounds(GameObject board, Camera camera, out FloorViewportBounds bounds)
        {
            bounds = default;
            if (board == null || camera == null)
            {
                return false;
            }

            var floor = board.transform.Find(BoardView.FloorContainerName);
            if (floor == null || floor.childCount == 0)
            {
                return false;
            }

            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;
            var centerXSum = 0f;
            var centerYSum = 0f;
            for (var i = 0; i < floor.childCount; i++)
            {
                var world = floor.GetChild(i).position;
                var half = 0.5f;
                var bl = camera.WorldToViewportPoint(new Vector3(world.x - half, world.y - half, world.z));
                var tr = camera.WorldToViewportPoint(new Vector3(world.x + half, world.y + half, world.z));
                minX = Math.Min(minX, Math.Min(bl.x, tr.x));
                maxX = Math.Max(maxX, Math.Max(bl.x, tr.x));
                minY = Math.Min(minY, Math.Min(bl.y, tr.y));
                maxY = Math.Max(maxY, Math.Max(bl.y, tr.y));
                var center = camera.WorldToViewportPoint(world);
                centerXSum += center.x;
                centerYSum += center.y;
            }

            var count = floor.childCount;
            bounds = new FloorViewportBounds
            {
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY,
                CenterX = centerXSum / count,
                CenterY = centerYSum / count
            };
            return true;
        }

        internal struct FloorViewportBounds
        {
            public float MinX;

            public float MaxX;

            public float MinY;

            public float MaxY;

            public float CenterX;

            public float CenterY;
        }

        internal struct ScreenRect
        {
            public float Left;

            public float Right;

            public float Bottom;

            public float Top;
        }

        internal sealed class LogCapture : IDisposable
        {
            private readonly List<string> _messages = new List<string>();

            public LogCapture()
            {
                Application.logMessageReceived += OnLogMessageReceived;
            }

            public IReadOnlyList<string> Messages => _messages;

            public int CountExact(string message)
            {
                var count = 0;
                for (var i = 0; i < _messages.Count; i++)
                {
                    if (_messages[i] == message)
                    {
                        count++;
                    }
                }

                return count;
            }

            public void Dispose()
            {
                Application.logMessageReceived -= OnLogMessageReceived;
            }

            private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
            {
                _messages.Add(condition);
            }
        }
    }
}
