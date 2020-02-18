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
        /// The Id of the inventory the item belongs to.
        /// </summary>
        public string inventoryId;

        /// <summary>
        /// The definition Id of the inventory item
        /// </summary>
        public string definitionId;

        /// <summary>
        /// The quantity of the inventory item in the inventory.
        /// </summary>
        public int quantity;

        /// <summary>
        /// The GameItemId of the item use by GameItemLookup
        /// </summary>
        public int gameItemId;

        /// <summary>
        /// Basic constructor that takes in the inventory item definition Id of the item and the quantity it have in this inventory.
        /// </summary>
        /// <param name="inventoryId">The Id of the inventory this item belongs to</param>
        /// <param name="definitionId">The definition Id of the inventory item</param>
        /// <param name="quantity">The quantity of this item contained in the inventory</param>
        /// <param name="gameItemId">The GameItemId of the item use by GameItemLookup</param>
        public InventoryItemSerializableData(string inventoryId, string definitionId, int quantity, int gameItemId)
        {
            this.inventoryId = inventoryId;
            this.definitionId = definitionId;
            this.quantity = quantity;
            this.gameItemId = gameItemId;
        }
    }
}
