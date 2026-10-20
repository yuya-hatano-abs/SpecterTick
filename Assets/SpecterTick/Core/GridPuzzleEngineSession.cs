using System;
using SpecterTick.Core.Stages;

namespace SpecterTick.Core
{
    public sealed class GridPuzzleEngineSession : IGameEngine
    {
        private readonly StageLayoutSnapshot _layout;
        private GameState _state;
        private RecordedCommandSequence _recordedCommands;

        public GridPuzzleEngineSession(IStage stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            // Assumes a stage that passed StageValidator (e.g. no player/specter on switch cells).
            // Switch latch at Tick 0 is not evaluated; initial activation is empty.
            Stage = stage;
            _layout = StageLayoutSnapshot.FromStage(stage);
            _recordedCommands = RecordedCommandSequence.Empty;
            _state = GameState.CreateInitial(
                _layout.InitialPlayerPosition,
                _layout.InitialSpecterPosition);
        }

        public IStage Stage { get; }

        public GameState State => _state;

        public RecordedCommandSequence RecordedCommands => _recordedCommands;

        /// <summary>
        /// True when mode is Playback and all recorded commands have been consumed.
        /// Always false in Recording and Cleared modes.
        /// </summary>
        public bool IsPlaybackFinished =>
            _state.Mode == GameMode.Playback
            && _state.PlaybackIndex == _recordedCommands.Count;

        public bool AllSwitchesActivated => _layout.AllSwitchesActivated(_state.ActivatedSwitches);

        public bool IsExitOpen => _layout.IsExitOpen(_state.ActivatedSwitches);

        public bool IsDoorOpen(DoorId doorId)
        {
            return _layout.IsDoorOpen(_state.ActivatedSwitches, doorId);
        }

        public void Step(MoveDirection direction)
        {
            if (!MoveDirectionUtility.IsDefined(direction))
            {
                throw new ArgumentOutOfRangeException(nameof(direction), direction, "The move direction is not defined.");
            }

            if (_state.Mode != GameMode.Recording)
            {
                throw new InvalidOperationException("Player moves can only be recorded in Recording mode.");
            }

            var activatedAtStart = _state.ActivatedSwitches;
            var nextPlayer = ResolveMove(_state.PlayerPosition, direction, activatedAtStart);
            var nextActivated = _layout.ApplyLatch(activatedAtStart, new[] { nextPlayer });
            var nextTick = checked(_state.Tick + 1);
            var nextState = new GameState(
                nextTick,
                nextPlayer,
                _state.SpecterPosition,
                nextActivated,
                GameMode.Recording,
                0);
            var nextCommands = _recordedCommands.Append(new RecordedCommand(direction));
            _state = nextState;
            _recordedCommands = nextCommands;
        }

        public void ConfirmRecording()
        {
            if (_state.Mode != GameMode.Recording)
            {
                throw new InvalidOperationException("Recording can only be confirmed from Recording mode.");
            }

            var player = _state.PlayerPosition;
            var specter = _layout.InitialSpecterPosition;
            var activated = _layout.ApplyLatch(
                ActivatedSwitchSet.Empty,
                new[] { player, specter });
            var nextState = new GameState(
                0,
                player,
                specter,
                activated,
                GameMode.Playback,
                0);
            nextState = ApplyClearIfNeeded(nextState);
            _state = nextState;
        }

        public void StepPlayback()
        {
            if (_state.Mode != GameMode.Playback)
            {
                throw new InvalidOperationException("Playback steps are only allowed in Playback mode.");
            }

            var activatedAtStart = _state.ActivatedSwitches;
            var player = _state.PlayerPosition;
            var specter = _state.SpecterPosition;
            var index = _state.PlaybackIndex;

            if (index < _recordedCommands.Count)
            {
                var direction = _recordedCommands[index].Direction;
                specter = ResolveMove(specter, direction, activatedAtStart);
                index++;
            }

            var nextActivated = _layout.ApplyLatch(
                activatedAtStart,
                new[] { player, specter });
            var nextTick = checked(_state.Tick + 1);
            var nextState = new GameState(
                nextTick,
                player,
                specter,
                nextActivated,
                GameMode.Playback,
                index);
            nextState = ApplyClearIfNeeded(nextState);
            _state = nextState;
        }

        private GridPosition ResolveMove(
            GridPosition from,
            MoveDirection direction,
            ActivatedSwitchSet activatedAtTickStart)
        {
            var offset = MoveDirectionUtility.ToOffset(direction);
            var candidate = from + offset;
            if (_layout.IsInside(candidate)
                && !_layout.BlocksMovement(candidate, activatedAtTickStart))
            {
                return candidate;
            }

            return from;
        }

        public void Retry()
        {
            var initialState = GameState.CreateInitial(
                _layout.InitialPlayerPosition,
                _layout.InitialSpecterPosition);
            var emptyCommands = RecordedCommandSequence.Empty;
            // Reset session state atomically: keep the two assignments adjacent with no fallible work between them.
            _state = initialState;
            _recordedCommands = emptyCommands;
        }

        private GameState ApplyClearIfNeeded(GameState state)
        {
            if (state.PlayerPosition.Equals(_layout.ExitPosition)
                && _layout.IsExitOpen(state.ActivatedSwitches))
            {
                return new GameState(
                    state.Tick,
                    state.PlayerPosition,
                    state.SpecterPosition,
                    state.ActivatedSwitches,
                    GameMode.Cleared,
                    state.PlaybackIndex);
            }

            return state;
        }
    }
}
