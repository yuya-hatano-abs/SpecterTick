using System;
using SpecterTick.Core.Stages;

namespace SpecterTick.Core
{
    public sealed class GridPuzzleEngineSession : IGameEngine
    {
        private readonly StageLayoutSnapshot _layout;
        private GameState _state;

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
            _state = GameState.CreateInitial(
                _layout.InitialPlayerPosition,
                _layout.InitialSpecterPosition);
        }

        public IStage Stage { get; }

        public GameState State => _state;

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

            var offset = MoveDirectionUtility.ToOffset(direction);
            var candidate = _state.PlayerPosition + offset;
            var nextPlayer = _state.PlayerPosition;

            if (_layout.IsInside(candidate)
                && !_layout.BlocksMovement(candidate, _state.ActivatedSwitches))
            {
                nextPlayer = candidate;
            }

            var nextActivated = _layout.ApplyLatch(
                _state.ActivatedSwitches,
                new[] { nextPlayer });
            var nextTick = checked(_state.Tick + 1);
            _state = new GameState(
                nextTick,
                nextPlayer,
                _state.SpecterPosition,
                nextActivated);
        }
    }
}
