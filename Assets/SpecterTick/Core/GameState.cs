using System;

namespace SpecterTick.Core
{
    public sealed class GameState : IEquatable<GameState>
    {
        public int Tick { get; }
        public GridPosition PlayerPosition { get; }
        public GridPosition SpecterPosition { get; }

        internal GameState(int tick, GridPosition playerPosition, GridPosition specterPosition)
        {
            Tick = tick;
            PlayerPosition = playerPosition;
            SpecterPosition = specterPosition;
        }

        internal static GameState CreateInitial(GridPosition playerPosition, GridPosition specterPosition)
        {
            return new GameState(0, playerPosition, specterPosition);
        }

        public bool Equals(GameState other)
        {
            if (other == null)
            {
                return false;
            }

            return Tick == other.Tick
                   && PlayerPosition.Equals(other.PlayerPosition)
                   && SpecterPosition.Equals(other.SpecterPosition);
        }

        public override bool Equals(object obj)
        {
            return obj is GameState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Tick;
                hash = (hash * 397) ^ PlayerPosition.GetHashCode();
                hash = (hash * 397) ^ SpecterPosition.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(GameState left, GameState right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(GameState left, GameState right)
        {
            return !(left == right);
        }
    }
}
