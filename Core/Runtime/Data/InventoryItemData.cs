using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure of an <see cref="InventoryItem"/>.
    /// </summary>
    [Serializable]
    public struct InventoryItemData
    {
        /// <summary>
        ///     Unique identifier of the item's definition.
        /// </summary>
        public string definitionKey;

        /// <inheritdoc cref="InventoryItem.id" />
        public string id;

        /// <summary>
        ///     Item's serializable properties data.
        /// </summary>
        public PropertyData[] properties;
    }
}
