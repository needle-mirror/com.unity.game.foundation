using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    /// Serializable data structure that contains the state of the
    /// <see cref="InventoryItem"/> instances.
    /// </summary>
    [Serializable]
    public struct InventoryManagerData
    {
        public InventoryItemData[] items;
    }
}
