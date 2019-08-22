using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contains a runtime list of InventoryItems as well as details which effect this Inventory.
    /// </summary>
    public class Inventory : BaseCollection<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        /// <summary>
        /// This is a reference to the InventoryManager's main Inventory.
        /// </summary>
        /// <returns>A reference to the InventoryManager's main Inventory.</returns>
        public static Inventory main
        {
            get { return InventoryManager.main; }
        }

        /// <summary>
        /// Default constructor for creating an Inventory.  Creates an empty Inventory.
        /// </summary>
        public Inventory()
            : base(null, null)
        {
            m_ItemsInCollection = new Dictionary<int, InventoryItem>();
        }

        /// <summary>
        /// Constructor for an Inventory from the specified InventoryDefinition.
        /// </summary>
        /// <param name="inventoryCollectionDefinition">The InventoryDefinition this Inventory is based off of.</param>
        /// <param name="collectionId">The id this Inventory will use.</param>
        public Inventory(InventoryDefinition inventoryCollectionDefinition, string collectionId = null) 
            : base(inventoryCollectionDefinition, collectionId)
        {
            m_ItemsInCollection = new Dictionary<int, InventoryItem>();

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("Inventory can only be alphanumeric with optional dashes or underscores.");
            }
            
            // iterate all default items in the Collection's definition (if there are any) and add them to the Collection
            AddAllDefaultItems();
        }

        /// <summary>
        /// Sets the quantity of the specified InventoryItem by InventoryItemDefinition id in this Inventory.
        /// </summary>
        /// <param name="definitionId">The id of the InventoryItemDefinition to set quantity for.</param>
        /// <param name="value">The new quantity for specified InventoryItemDefinition.</param>
        public void SetQuantity(string definitionId, int value)
        {
            SetIntValue(definitionId, value);
        }

        /// <summary>
        /// Sets the quantity of the specified InventoryItem by InventoryItemDefinition id hash in this Inventory.
        /// </summary>
        /// <param name="definitionHash">The id hash of the InventoryItemDefinition to set.</param>
        /// <param name="value">The new quantity for the specified InventoryItemDefinition.</param>
        public void SetQuantity(int definitionHash, int value)
        {
            SetIntValue(definitionHash, value);
        }

        /// <summary>
        /// Sets the quantity of the InventoryItem instance within this Inventory based on specified InventoryItemDefinition.
        /// </summary>
        /// <param name="definition">The InventoryItemDefinition to set quantity for in this Inventory.</param>
        /// <param name="value">The new value for quantity for this InventoryItem.</param>
        public void SetQuantity(InventoryItemDefinition definition, int value)
        {
            SetIntValue(definition, value);
        }

        protected override BaseItemDefinition<InventoryItemDefinition, InventoryItem> GetItemDefinition(int definitionHash)
        {
            return InventoryManager.catalog == null ? null : InventoryManager.catalog.GetItemDefinition(definitionHash);
        }

        /// <summary>
        /// Adds a new entry of the specified InventoryItemDefinition by id hash. Returns the new (or existing) reference to the instance.
        /// Returns the new reference to the instance when the item didn't already exist in the Inventory and
        /// returns an existing reference when to the instance when the Item was already in the Inventory.
        /// Items added to the wallet Inventory must contain an attached CurrencyDetailsDefinition.
        /// </summary>
        /// <param name="definitionHash">The id hash of the InventoryItemDefinition to set.</param>
        /// <param name="value">The new quantity for the specified InventoryItemDefinition.</param>
        /// <returns>The new or existing instance of the InventoryItem.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override InventoryItem AddItem(int definitionHash, int value = 1)
        {
            if (m_Hash.Equals(InventoryManager.k_WalletInventoryHash))
            {
                if (GetItemDefinition(definitionHash)?.GetDetailsDefinition<CurrencyDetailsDefinition>() == null)
                {
                    throw new InvalidOperationException(
                        "ItemDefinition must have a CurrencyDetailsDefinition DetailsDefinition. Adding a non-currency item to the wallet Inventory is invalid.");
                }
            }
            return base.AddItem(definitionHash, value);
        }

        /// <summary>
        /// Returns a summary string for this Inventory.
        /// </summary>
        /// <returns>Summary string for this Inventory.</returns>
        public override string ToString()
        {
            return $"Inventory(Id: '{m_Id}' DisplayName: '{m_DisplayName}' Definition: '{m_Definition.id}' Count: {m_ItemsInCollection?.Count}";
        }
    }
}
