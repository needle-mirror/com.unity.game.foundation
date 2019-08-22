using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// An InventoryItem that goes into an Inventory. InventoryItems should only exist inside Inventories. 
    /// </summary>
    public class InventoryItem : BaseItem<InventoryItemDefinition, InventoryItem>
    {
        /// <summary>
        /// Basic constructor that takes in a reference to the InventoryItemDefinition that this is based on.
        /// </summary>
        /// <param name="definition">The InventoryItemDefinition this item is based on.</param>
        public InventoryItem(InventoryItemDefinition definition) : base(definition)
        {
        }

        /// <summary>
        /// Quantity contained in this Inventory for this InventoryItem.
        /// </summary>
        /// <returns>Quantity contained in this Inventory for this InventoryItem.</returns>
        public int quantity
        {
            get { return m_intValue; }
            internal set { m_intValue = value; }
        }
    }
}
