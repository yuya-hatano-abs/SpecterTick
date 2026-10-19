using System;

namespace SpecterTick.Core
{
    /// <summary>
    /// Maps <see cref="MoveDirection"/> values to grid offsets and validates enum values.
    /// </summary>
    public static class MoveDirectionUtility
    {
        public static bool IsDefined(MoveDirection direction)
        {
            return direction == MoveDirection.Up
                   || direction == MoveDirection.Down
                   || direction == MoveDirection.Left
                   || direction == MoveDirection.Right;
        }

        /// <summary>
        /// Returns the coordinate delta for one step in the given direction.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The direction is not a defined member.</exception>
        public static GridPosition ToOffset(MoveDirection direction)
        {
            if (!IsDefined(direction))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(direction),
                    direction,
                    "The move direction is not defined.");
            }

            switch (direction)
            {
                case MoveDirection.Up:
                    return new GridPosition(0, 1);
                case MoveDirection.Down:
                    return new GridPosition(0, -1);
                case MoveDirection.Left:
                    return new GridPosition(-1, 0);
                case MoveDirection.Right:
                    return new GridPosition(1, 0);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(direction),
                        direction,
                        "The move direction is not defined.");
            }
        }
    }
}
