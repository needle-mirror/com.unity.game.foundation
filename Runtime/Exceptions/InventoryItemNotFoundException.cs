using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    /// Thrown when the <see cref="InventoryItem"/> fails to be found by its
    /// <see cref="GameItem.id"/>
    /// </summary>
    public class InventoryItemNotFoundException : Exception
    {
        /// <summary>
        /// The identifier of the <see cref="InventoryItem"/> not found.
        /// </summary>
        public readonly string itemId;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="InventoryItemNotFoundException"/> class.
        /// </summary>
        /// <param name="id"></param>
        public InventoryItemNotFoundException(string id)
        {
            itemId = id;
        }

        /// <inheritdoc/>
        public override string Message
            => $"{nameof(InventoryItem)} {itemId} not found";
    }
}
