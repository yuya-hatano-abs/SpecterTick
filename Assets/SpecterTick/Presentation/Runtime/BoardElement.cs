using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    public readonly struct BoardElement
    {
        internal BoardElement(BoardElementKind kind, GridPosition position, bool isActive)
        {
            Kind = kind;
            Position = position;
            IsActive = isActive;
        }

        public BoardElementKind Kind { get; }

        public GridPosition Position { get; }

        public bool IsActive { get; }
    }
}
