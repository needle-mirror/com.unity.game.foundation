using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Serializable data structure that contains the state of an inventory item.
    /// </summary>
    [Serializable]
    public class InventoryItemSerializableData
    {
        [SerializeField] string m_DefinitionId = null;
        [SerializeField] int m_Quantity = 0;

        /// <summary>
        /// The definition id of the inventory item
        /// </summary>
        public string definitionId
        {
            get { return m_DefinitionId; }
        }

        /// <summary>
        /// The quantity of the inventory item in the inventory.
        /// </summary>
        public int quantity
        {
            get { return m_Quantity; }
        }
        
        /// <summary>
        /// Basic constructor that takes in the inventory item definition id of the item and the quantity it have in this inventory.
        /// </summary>
        /// <param name="definitionId">The definition id of the inventory item</param>
        /// <param name="quantity">The quantity of this item contained in the inventory</param>
        public InventoryItemSerializableData(string definitionId, int quantity)
        {
            m_DefinitionId = definitionId;
            m_Quantity = quantity;
        }

        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public InventoryItemSerializableData()
        {
        }
    }
}