using System;

namespace SpecterTick.Core
{
    /// <summary>
    /// A single recorded move command (direction only; tick and playback are handled elsewhere).
    /// </summary>
    public readonly struct RecordedCommand : IEquatable<RecordedCommand>
    {
        public MoveDirection Direction { get; }

        public RecordedCommand(MoveDirection direction)
        {
            if (!MoveDirectionUtility.IsDefined(direction))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    direction,
                    "The move direction is not defined.");
            }

            Direction = direction;
        }

        public bool Equals(RecordedCommand other)
        {
            return Direction == other.Direction;
        }

        public override bool Equals(object obj)
        {
            return obj is RecordedCommand other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Direction;
        }

        public static bool operator ==(RecordedCommand left, RecordedCommand right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RecordedCommand left, RecordedCommand right)
        {
            return !left.Equals(right);
        }
    }
}
