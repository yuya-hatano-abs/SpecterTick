using System;
using SpecterTick.Core;

namespace SpecterTick.Presentation
{
    /// <summary>
    /// Grid-to-world and orthographic camera sizing. All coordinate formulas live here only.
    /// </summary>
    public sealed class GridWorldMapper
    {
        private readonly float _cellSize;

        public GridWorldMapper(float cellSize)
        {
            if (cellSize <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "Cell size must be greater than zero.");
            }

            _cellSize = cellSize;
        }

        public float CellCenterX(int gridX)
        {
            return gridX * _cellSize;
        }

        public float CellCenterY(int gridY)
        {
            return gridY * _cellSize;
        }

        public float CellCenterX(GridPosition position)
        {
            return CellCenterX(position.X);
        }

        public float CellCenterY(GridPosition position)
        {
            return CellCenterY(position.Y);
        }

        public void GetBoardCenter(int width, int height, out float x, out float y)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
            }

            x = ((width - 1) / 2f) * _cellSize;
            y = ((height - 1) / 2f) * _cellSize;
        }

        public float ComputeOrthographicSize(int width, int height, float aspect)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
            }

            if (aspect <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(aspect), aspect, "Aspect must be greater than zero.");
            }

            var heightTerm = (height + 2) * _cellSize / 2f;
            var widthTerm = (width + 2) * _cellSize / 2f / aspect;
            return Math.Max(heightTerm, widthTerm);
        }
    }
}
