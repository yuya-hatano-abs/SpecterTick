using System;

namespace SpecterTick.Core
{
    public sealed class GameState : IEquatable<GameState>
    {
        internal ActivatedSwitchSet ActivatedSwitches { get; }

        public int Tick { get; }
        public GridPosition PlayerPosition { get; }
        public GridPosition SpecterPosition { get; }

        internal GameState(
            int tick,
            GridPosition playerPosition,
            GridPosition specterPosition,
            ActivatedSwitchSet activatedSwitches)
        {
            Tick = tick;
            PlayerPosition = playerPosition;
            SpecterPosition = specterPosition;
            if (activatedSwitches == null)
            {
                throw new ArgumentNullException(nameof(activatedSwitches));
            }

            ActivatedSwitches = activatedSwitches;
        }

        internal static GameState CreateInitial(GridPosition playerPosition, GridPosition specterPosition)
        {
            return new GameState(0, playerPosition, specterPosition, ActivatedSwitchSet.Empty);
        }

        public bool IsSwitchActivated(SwitchId switchId)
        {
            if (!switchId.IsValid)
            {
                throw new ArgumentException("Switch ID is not valid.", nameof(switchId));
            }

            return ActivatedSwitches.Contains(switchId);
        }

        public bool Equals(GameState other)
        {
            if (other == null)
            {
                return false;
            }

            return Tick == other.Tick
                   && PlayerPosition.Equals(other.PlayerPosition)
                   && SpecterPosition.Equals(other.SpecterPosition)
                   && ActivatedSwitches.Equals(other.ActivatedSwitches);
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
                hash = (hash * 397) ^ ActivatedSwitches.GetHashCode();
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
