using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Serializable data structure that contains the state of an inventory.
    /// </summary>
    [Serializable]
    public class InventorySerializableData
    {
        [SerializeField] string m_DefinitionId = null;
        [SerializeField] string m_InventoryId = null;
        [SerializeField] InventoryItemSerializableData[] m_Items = null;

        /// <summary>
        /// The definition id of the inventory
        /// </summary>
        public string definitionId
        {
            get { return m_DefinitionId; }
        }
        
        /// <summary>
        /// The inventory id of the inventory
        /// </summary>
        public string inventoryId
        {
            get { return m_InventoryId; }
        }

        /// <summary>
        /// The items this inventory contains
        /// </summary>
        public InventoryItemSerializableData[] items
        {
            get { return m_Items; }
        }

        /// <summary>
        /// Basic constructor that takes in an inventory definition id and an array of InventoryItemData of all inventory items contained in the inventory.
        /// </summary>
        /// <param name="definitionId">The definition id of the inventory</param>
        /// <param name="items">The inventory items contained in the inventory</param>
        public InventorySerializableData(string definitionId, string inventoryId, InventoryItemSerializableData[] items)
        {
            m_DefinitionId = definitionId;
            m_InventoryId = inventoryId;
            m_Items = items;
        }

        /// <summary>
        /// Default constructor for serialization purpose
        /// </summary>
        public InventorySerializableData()
        {
        }
    }
}