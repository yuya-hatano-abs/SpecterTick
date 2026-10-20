using System;

namespace SpecterTick.Core
{
    public sealed class GameState : IEquatable<GameState>
    {
        internal ActivatedSwitchSet ActivatedSwitches { get; }

        public int Tick { get; }
        public GridPosition PlayerPosition { get; }
        public GridPosition SpecterPosition { get; }
        public GameMode Mode { get; }
        public int PlaybackIndex { get; }

        internal GameState(
            int tick,
            GridPosition playerPosition,
            GridPosition specterPosition,
            ActivatedSwitchSet activatedSwitches,
            GameMode mode,
            int playbackIndex)
        {
            if (activatedSwitches == null)
            {
                throw new ArgumentNullException(nameof(activatedSwitches));
            }

            if (!GameModeUtility.IsDefined(mode))
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "The game mode is not defined.");
            }

            if (playbackIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playbackIndex), playbackIndex, "Playback index cannot be negative.");
            }

            Tick = tick;
            PlayerPosition = playerPosition;
            SpecterPosition = specterPosition;
            ActivatedSwitches = activatedSwitches;
            Mode = mode;
            PlaybackIndex = playbackIndex;
        }

        internal static GameState CreateInitial(GridPosition playerPosition, GridPosition specterPosition)
        {
            return new GameState(
                0,
                playerPosition,
                specterPosition,
                ActivatedSwitchSet.Empty,
                GameMode.Recording,
                0);
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
                   && ActivatedSwitches.Equals(other.ActivatedSwitches)
                   && Mode == other.Mode
                   && PlaybackIndex == other.PlaybackIndex;
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
                hash = (hash * 397) ^ (int)Mode;
                hash = (hash * 397) ^ PlaybackIndex;
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
