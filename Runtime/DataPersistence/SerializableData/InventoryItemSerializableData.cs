using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of an inventory item.
    /// </summary>
    [Serializable]
    public struct InventoryItemSerializableData
    {
        /// <summary>
        /// The definition Id of the inventory item
        /// </summary>
        public string definitionId;

        /// <summary>
        /// The GameItemId of the item use by GameItemLookup
        /// </summary>
        public string id;

        /// <summary>
        /// Basic constructor that takes in the inventory item definition Id of the item and the quantity it have in this inventory.
        /// </summary>
        /// <param name="definitionId">The definition Id of the inventory item</param>
        /// <param name="id">The id of the item</param>
        public InventoryItemSerializableData(string definitionId, string id)
        {
            this.definitionId = definitionId;
            this.id = id;
        }
    }
}
