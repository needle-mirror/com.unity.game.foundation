using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    /// Thrown when a detail of a given type is expected but not found in the
    /// given item.
    /// </summary>
    public class DetailNotFoundException : Exception
    {
        /// <summary>
        /// The identifier of the item where the detail didn't have been found.
        /// </summary>
        public readonly string itemId;

        /// <summary>
        /// The type of the detail not found.
        /// </summary>
        public Type detailType;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DetailNotFoundException"/> instance.
        /// </summary>
        /// <param name="itemId">The identifier of the item where the detail
        /// didn't have been found.</param>
        /// <param name="detailType">The type of the detail not found</param>
        public DetailNotFoundException(string itemId, Type detailType)
        {
            this.itemId = itemId;
            this.detailType = detailType;
        }

        /// <inheritdoc/>
        public override string Message =>
            $"Detail {detailType.Name} not found in item {itemId}";
    }
}
