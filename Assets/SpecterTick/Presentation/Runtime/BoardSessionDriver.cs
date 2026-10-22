using SpecterTick.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpecterTick.Presentation
{
    [RequireComponent(typeof(BoardView))]
    public sealed class BoardSessionDriver : MonoBehaviour
    {
        private BoardView _boardView;
        private GridPuzzleEngineSession _session;
        private SessionInputDispatcher _dispatcher;
        private GameMode _lastLoggedMode;
        private bool _updateEnabled;
        private bool _keyboardNullWarningLogged;
        private bool _startFailedLogged;

        private void Start()
        {
            // Temporary driver until Task 15 (stage loading / game flow). Does not call session.Step from Start.
            try
            {
                _boardView = GetComponent<BoardView>();
                var stage = SampleStageFactory.Create();
                _session = new GridPuzzleEngineSession(stage);
                _dispatcher = new SessionInputDispatcher(_session);
                _boardView.Show(stage);
                _boardView.UpdateActors(_session.State);
                _boardView.FitCamera();
                _lastLoggedMode = _session.State.Mode;
                _updateEnabled = true;
            }
            catch (System.Exception ex)
            {
                _updateEnabled = false;
                if (!_startFailedLogged)
                {
                    Debug.LogError("BoardSessionDriver.Start failed: " + ex.Message);
                    _startFailedLogged = true;
                }
            }
        }

        private void Update()
        {
            if (!_updateEnabled || _dispatcher == null || _boardView == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                if (!_keyboardNullWarningLogged)
                {
                    Debug.LogWarning("BoardSessionDriver: Keyboard.current is null; input is ignored.");
                    _keyboardNullWarningLogged = true;
                }

                return;
            }

            var keys = ReadPressedKeys(keyboard);
            if (keys == InputKeys.None)
            {
                return;
            }

            var intent = InputIntentResolver.Resolve(keys);
            var stateBefore = _session.State;
            var result = _dispatcher.Dispatch(intent);

            if (result == InputResult.RejectedByMode)
            {
                Debug.Log(
                    "BoardSessionDriver: input rejected by mode (intent="
                    + intent.Kind
                    + ", mode="
                    + stateBefore.Mode
                    + ").");
                return;
            }

            if (result == InputResult.Executed)
            {
                _boardView.UpdateActors(_session.State);
                var stateAfter = _session.State;
                if (stateAfter.Mode != _lastLoggedMode)
                {
                    Debug.Log(
                        "BoardSessionDriver: mode changed to "
                        + stateAfter.Mode
                        + " (tick="
                        + stateAfter.Tick
                        + ").");
                    _lastLoggedMode = stateAfter.Mode;
                }
            }
        }

        private static InputKeys ReadPressedKeys(Keyboard keyboard)
        {
            var keys = InputKeys.None;
            if (keyboard.upArrowKey.wasPressedThisFrame)
            {
                keys |= InputKeys.ArrowUp;
            }

            if (keyboard.downArrowKey.wasPressedThisFrame)
            {
                keys |= InputKeys.ArrowDown;
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame)
            {
                keys |= InputKeys.ArrowLeft;
            }

            if (keyboard.rightArrowKey.wasPressedThisFrame)
            {
                keys |= InputKeys.ArrowRight;
            }

            if (keyboard.wKey.wasPressedThisFrame)
            {
                keys |= InputKeys.KeyW;
            }

            if (keyboard.aKey.wasPressedThisFrame)
            {
                keys |= InputKeys.KeyA;
            }

            if (keyboard.sKey.wasPressedThisFrame)
            {
                keys |= InputKeys.KeyS;
            }

            if (keyboard.dKey.wasPressedThisFrame)
            {
                keys |= InputKeys.KeyD;
            }

            if (keyboard.enterKey.wasPressedThisFrame)
            {
                keys |= InputKeys.Enter;
            }

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                keys |= InputKeys.Space;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                keys |= InputKeys.KeyR;
            }

            return keys;
        }
    }
}
