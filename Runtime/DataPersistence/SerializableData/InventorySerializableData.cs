using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of an inventory.
    /// </summary>
    [Serializable]
    public struct InventorySerializableData
    {
        /// <summary>
        /// The definition Id of the inventory
        /// </summary>
        public string definitionId;

        /// <summary>
        /// The inventory id of the inventory
        /// </summary>
        public string Id;

        /// <summary>
        /// The friendly name of the inventory that will be displayed
        /// </summary>
        public string displayName;

        /// <summary>
        /// The GameItemId of the item use by GameItemLookup
        /// </summary>
        public int gameItemId;
    }
}
