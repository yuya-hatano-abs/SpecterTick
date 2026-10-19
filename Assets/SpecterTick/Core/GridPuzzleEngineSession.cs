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

            Stage = stage;
            _layout = StageLayoutSnapshot.FromStage(stage);
            // GameState.CreateInitial is only called after the null check above; it does not accept IStage.
            _state = GameState.CreateInitial(
                _layout.InitialPlayerPosition,
                _layout.InitialSpecterPosition);
        }

        public IStage Stage { get; }

        public GameState State => _state;

        public void Step(MoveDirection direction)
        {
            if (!MoveDirectionUtility.IsDefined(direction))
            {
                throw new ArgumentOutOfRangeException(nameof(direction), direction, "The move direction is not defined.");
            }

            var offset = MoveDirectionUtility.ToOffset(direction);
            var candidate = _state.PlayerPosition + offset;
            var nextPlayer = _state.PlayerPosition;

            if (_layout.IsInside(candidate) && !_layout.BlocksMovement(candidate))
            {
                nextPlayer = candidate;
            }

            var nextTick = checked(_state.Tick + 1);
            _state = new GameState(nextTick, nextPlayer, _state.SpecterPosition);
        }
    }
}
