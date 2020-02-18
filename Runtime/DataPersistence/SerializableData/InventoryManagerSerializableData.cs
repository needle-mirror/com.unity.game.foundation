using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of the InventoryManager.
    /// </summary>
    [Serializable]
    public struct InventoryManagerSerializableData
    {
        /// <summary>
        /// Get an empty instance of this class.
        /// </summary>
        public static InventoryManagerSerializableData Empty => new InventoryManagerSerializableData
        {
            inventories = new InventorySerializableData[0],
            items = new InventoryItemSerializableData[0]
        };

        public InventorySerializableData[] inventories;
        public InventoryItemSerializableData[] items;
    }
}
