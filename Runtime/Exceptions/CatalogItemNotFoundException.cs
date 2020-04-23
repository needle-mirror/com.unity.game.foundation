using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    /// Thrown when a <see cref="CatalogItem"/> fails to be found by its
    /// <see cref="CatalogItem.id"/>.
    /// </summary>
    public class CatalogItemNotFoundException : Exception
    {
        /// <summary>
        /// The id of the item not found.
        /// </summary>
        public readonly string itemId;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CatalogItemNotFoundException"/> class.
        /// </summary>
        /// <param name="id">The id of the <see cref="CatalogItem"/> not
        /// found.</param>
        public CatalogItemNotFoundException(string id)
        {
            itemId = id;
        }

        /// <inheritdoc/>
        public override string Message
            => $"{nameof(CatalogItem)} {itemId} not found";
    }
}
