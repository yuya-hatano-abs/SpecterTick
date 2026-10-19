using System;

namespace SpecterTick.Core
{
    /// <summary>
    /// Integer grid coordinates. X increases to the right; Y increases upward.
    /// </summary>
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public int X { get; }
        public int Y { get; }

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static GridPosition operator +(GridPosition left, GridPosition right)
        {
            return new GridPosition(
                checked(left.X + right.X),
                checked(left.Y + right.Y));
        }

        public static GridPosition operator -(GridPosition left, GridPosition right)
        {
            return new GridPosition(
                checked(left.X - right.X),
                checked(left.Y - right.Y));
        }

        public static bool operator ==(GridPosition left, GridPosition right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(GridPosition left, GridPosition right)
        {
            return !(left == right);
        }

        public bool Equals(GridPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
