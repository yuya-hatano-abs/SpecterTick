using SpecterTick.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpecterTick.Presentation
{
    [RequireComponent(typeof(BoardView))]
    public sealed class BoardSessionDriver : MonoBehaviour
    {
        private BoardView _boardView;
        private HudView _hudView;
        private ClearScreenView _clearScreenView;
        private StageLabelView _stageLabelView;
        private StageFlow _flow;
        private GridPuzzleEngineSession _session;
        private SessionInputDispatcher _dispatcher;
        private GameMode _lastLoggedMode;
        private bool _updateEnabled;
        private bool _keyboardNullWarningLogged;
        private bool _startFailedLogged;
        private ScreenSizeTracker _screenSizeTracker;
        private BoardSkin _skin;
        private HowToPlayView _howToPlayView;
        private HowToPlayState _howToPlayState;

        public bool ShowHowToPlayOnStart { get; set; } = true;

        private void Start()
        {
            // Temporary driver until Task 15 (stage loading / game flow). Does not call session.Step from Start.
            try
            {
                _boardView = GetComponent<BoardView>();
                _flow = new StageFlow(StageCatalog.Count);
                if (!TryGetComponent(out HudView hudView))
                {
                    hudView = gameObject.AddComponent<HudView>();
                }

                // Temporary HUD wiring until Task 15 (stage loading / game flow).
                _hudView = hudView;
                if (!TryGetComponent(out ClearScreenView clearScreenView))
                {
                    clearScreenView = gameObject.AddComponent<ClearScreenView>();
                }

                _clearScreenView = clearScreenView;
                if (!TryGetComponent(out StageLabelView stageLabelView))
                {
                    stageLabelView = gameObject.AddComponent<StageLabelView>();
                }

                _stageLabelView = stageLabelView;
                if (!TryGetComponent(out HowToPlayView howToPlayView))
                {
                    howToPlayView = gameObject.AddComponent<HowToPlayView>();
                }

                _howToPlayView = howToPlayView;
                _howToPlayState = new HowToPlayState();
                _screenSizeTracker = new ScreenSizeTracker(
                    Mathf.Max(1, Screen.width),
                    Mathf.Max(1, Screen.height));
                _skin = BoardSkinLoader.Load();
                _boardView.ApplySkin(_skin);
                if (_skin.MissingCount > 0)
                {
                    Debug.LogWarning("BoardSessionDriver: " + _skin.MissingCount + " skin sprite(s) are missing; procedural fallback is used.");
                }

                LoadStage();
                _howToPlayView.Apply(HowToPlayModelBuilder.Build(), _skin);
                if (ShowHowToPlayOnStart)
                {
                    _howToPlayState.Open();
                    _howToPlayView.SetVisible(true);
                }
                else
                {
                    _howToPlayView.SetVisible(false);
                }

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

        private void OnDestroy()
        {
            if (_skin != null)
            {
                _skin.Release();
                _skin = null;
            }
        }

        private void Update()
        {
            if (!_updateEnabled || _dispatcher == null || _boardView == null)
            {
                return;
            }

            if (_screenSizeTracker != null
                && _screenSizeTracker.Update(Mathf.Max(1, Screen.width), Mathf.Max(1, Screen.height)))
            {
                FitBoardCamera();
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

            var helpPressed = keyboard.hKey.wasPressedThisFrame;
            var anyKeyPressed = keyboard.anyKey.wasPressedThisFrame;
            if (_howToPlayState != null && _howToPlayView != null)
            {
                var howToAction = _howToPlayState.Update(helpPressed, anyKeyPressed);
                if (howToAction == HowToPlayAction.Opened)
                {
                    _howToPlayView.SetVisible(true);
                    return;
                }

                if (howToAction == HowToPlayAction.Closed)
                {
                    _howToPlayView.SetVisible(false);
                    return;
                }

                if (_howToPlayState.IsOpen)
                {
                    return;
                }
            }

            var navigation = StageNavigationResolver.Resolve(
                keyboard.nKey.wasPressedThisFrame,
                keyboard.pKey.wasPressedThisFrame);
            if (navigation != StageNavigation.None)
            {
                if (_flow.Navigate(navigation))
                {
                    try
                    {
                        LoadStage();
                    }
                    catch (System.Exception ex)
                    {
                        _updateEnabled = false;
                        Debug.LogError("BoardSessionDriver.LoadStage failed: " + ex.Message);
                    }
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
                _boardView.UpdateElements(BoardElementsModelBuilder.Build(_session));
                RefreshHud();
                RefreshClearScreen();
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

        private void LoadStage()
        {
            var stage = StageCatalog.CreateStage(_flow.CurrentIndex);
            _session = new GridPuzzleEngineSession(stage);
            _dispatcher = new SessionInputDispatcher(_session);
            _boardView.Show(stage);
            _boardView.UpdateActors(_session.State);
            _boardView.UpdateElements(BoardElementsModelBuilder.Build(_session));
            RefreshHud();
            RefreshClearScreen();
            RefreshStageLabel();
            FitBoardCamera();
            _lastLoggedMode = _session.State.Mode;
        }

        private void FitBoardCamera()
        {
            var width = Mathf.Max(1, Screen.width);
            var height = Mathf.Max(1, Screen.height);
            var insets = BoardScreenInsets.Compute(width, height);
            var camera = Camera.main;
            if (camera == null)
            {
                _boardView.FitCamera();
                return;
            }

            _boardView.FitCamera(camera, insets.Top, insets.Bottom);
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

        private void RefreshHud()
        {
            if (_hudView == null || _session == null)
            {
                return;
            }

            _hudView.Apply(HudModelBuilder.Build(_session));
        }

        private void RefreshClearScreen()
        {
            if (_clearScreenView == null || _session == null)
            {
                return;
            }

            _clearScreenView.Apply(ClearScreenModelBuilder.Build(_session));
        }

        private void RefreshStageLabel()
        {
            if (_stageLabelView == null || _flow == null)
            {
                return;
            }

            _stageLabelView.Apply(StageLabelModelBuilder.Build(
                _flow.CurrentIndex,
                _flow.StageCount,
                StageCatalog.GetName(_flow.CurrentIndex)));
        }
    }
}
