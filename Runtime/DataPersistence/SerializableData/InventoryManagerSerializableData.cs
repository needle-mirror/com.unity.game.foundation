using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of the
    /// <see cref="InventoryItem"/> instances.
    /// </summary>
    [Serializable]
    public struct InventoryManagerSerializableData
    {
        /// <summary>
        /// Get an empty instance of this class.
        /// </summary>
        public static InventoryManagerSerializableData Empty => new InventoryManagerSerializableData
        {
            items = new InventoryItemSerializableData[0]
        };

        public InventoryItemSerializableData[] items;
    }
}
