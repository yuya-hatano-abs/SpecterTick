using System;
using System.Collections.Generic;

namespace SpecterTick.Presentation
{
    public sealed class BoardElementsModel
    {
        internal BoardElementsModel(BoardElement[] elements)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            var copy = new BoardElement[elements.Length];
            Array.Copy(elements, copy, elements.Length);
            Elements = Array.AsReadOnly(copy);
        }

        public IReadOnlyList<BoardElement> Elements { get; }
    }
}
