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
