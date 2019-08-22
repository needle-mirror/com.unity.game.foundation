using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Static helper class for dealing with the Wallet inventory.  
    /// Many methods are simply shortcuts for 'InventoryManager.wallet.xxx'.
    /// </summary>
    public static class Wallet
    {
        /// <summary>
        /// Adds a new entry of the specified InventoryItemDefinition by id. Returns the new or existing InventoryItem.
        /// Returns the new InventoryItem when the InventoryItem didn't already exist in the Wallet and
        /// returns an existing InventoryItem when the InventoryItem was already in the Wallet.
        /// </summary>
        /// <param name="definitionId">The id of the InventoryItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this InventoryItem we are adding.</param>
        /// <returns>The new InventoryItem that was added, or null if id is invalid.</returns>
        public static InventoryItem AddItem(string definitionId, int quantity = 1)
        {
            return InventoryManager.wallet.AddItem(definitionId, quantity);
        }

        /// <summary>
        /// Adds more of the specified InventoryItemDefinition by hash. Returns the new (or existing) InventoryItem.
        /// Returns the new InventoryItem when the InventoryItem didn't already exist in the Wallet and
        /// returns an existing reference when to the InventoryItem when the InventoryItem was already in the Wallet.
        /// </summary>
        /// <param name="definitionHash">The id hash of the InventoryItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this InventoryItem we are adding.</param>
        /// <returns>The InventoryItem that was added.</returns>
        public static InventoryItem AddItem(int definitionHash, int quantity = 1)
        {
            return InventoryManager.wallet.AddItem(definitionHash, quantity);
        }

        /// <summary>
        /// Adds more of the specified InventoryItemDefinition. Returns the new (or existing) InventoryItem.
        /// Returns the new InventoryItem when the InventoryItem didn't already exist in the Wallet and
        /// returns an existing reference when to the InventoryItem when the InventoryItem was already in the Wallet.
        /// </summary>
        /// <param name="definition">The InventoryItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this InventoryItem we are adding.</param>
        /// <returns>The InventoryItem that was added.</returns>
        public static InventoryItem AddItem(InventoryItemDefinition definition, int quantity = 1)
        {
            return InventoryManager.wallet.AddItem(definition, quantity);
        }

        /// <summary>
        /// The InventoryDefinition of this Wallet. Determines the default InventoryItems and quantities.
        /// </summary>
        /// <returns>The InventoryDefinition of this Wallet.</returns>
        public static InventoryDefinition definition
        {
            get { return InventoryManager.wallet.definition; }
        }

        /// <summary>
        /// Helper property for easily accessing the id of the Wallet's InventoryDefinition.
        /// </summary>
        /// <returns>The id of the Wallet's InventoryDefinition</returns>
        public static string definitionId
        {
            get { return InventoryManager.wallet.definitionId; }
        }

        /// <summary>
        /// Helper property for easily accessing the Wallet's InventoryDefinition id hash.
        /// </summary>
        /// <returns>The Wallet's InventoryDefinition id hash.</returns>
        public static int definitionHash
        {
            get { return InventoryManager.wallet.definitionHash; }
        }

        /// <summary>
        /// This returns an enumerator for easily iterating through InventoryItems in the Wallet.
        /// </summary>
        /// <returns>An enumerator for easily iterating through InventoryItems in the Wallet.</returns>
        public static IEnumerable<InventoryItem> items
        {
            get { return InventoryManager.wallet.items; }
        }

        /// <summary>
        /// Gets an InventoryItem by id if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="id">The id of the InventoryItem to find.</param>
        /// <returns>The InventoryItem or null if not found.</returns>
        public static InventoryItem GetItem(string id)
        {
            return InventoryManager.wallet.GetItem(id);
        }

        /// <summary>
        /// Gets an InventoryItem by id hash if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="idHash">The id hash of the InventoryItem to find.</param>
        /// <returns>The InventoryItem or null if not found.</returns>
        public static InventoryItem GetItem(int idHash)
        {
            return InventoryManager.wallet.GetItem(idHash);
        }

        /// <summary>
        /// Gets an InventoryItem by id if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="definitionId">The id of the InventoryItemDefinition to find.</param>
        /// <returns>The InventoryItem or null if not found.</returns>
        public static InventoryItem GetItemByDefinition(string definitionId)
        {
            return InventoryManager.wallet.GetItem(definitionId);
        }

        /// <summary>
        /// This will look for an InventoryItem of the given InventoryItemDefinition, and return it if found.
        /// </summary>
        /// <param name="definition">The InventoryItemDefinition to find.</param>
        /// <returns>The InventoryItem or null if not found.</returns>
        public static InventoryItem GetItemByDefinition(InventoryItemDefinition definition)
        {
            return InventoryManager.wallet.GetItemByDefinition(definition);
        }

        /// <summary>
        /// Gets an InventoryItem by InventoryItemDefinition id hash if it is contained within, otherwise throws an exception.
        /// </summary>
        /// <param name="definitionHash">The hash of the InventoryItemDefinition to find.</param>
        /// <returns>The InventoryItem, if found.</returns>
        public static InventoryItem GetItemByDefinition(int definitionHash)
        {
            return InventoryManager.wallet.GetItemByDefinition(definitionHash);
        }

        /// <summary>
        /// This will return all InventoryItems that have the given Category through an enumerator.
        /// </summary>
        /// <param name="categoryId">The id of the Category we are checking for.</param>
        /// <returns>An enumerator for the InventoryItems that have the given Category.</returns>
        public static IEnumerable<InventoryItem> GetItemsByCategory(string categoryId)
        {
            return InventoryManager.wallet.GetItemsByCategory(categoryId);
        }

        /// <summary>
        /// This will return all InventoryItems that have the given Category through an enumerator.
        /// </summary>
        /// <param name="categoryDefinition">The CategoryDefinition we are checking for.</param>
        /// <returns>An enumerator for the InventoryItems that have the given Category.</returns>
        public static IEnumerable<InventoryItem> GetItemsByCategory(CategoryDefinition categoryDefinition)
        {
            return InventoryManager.wallet.GetItemsByCategory(categoryDefinition);
        }

        /// <summary>
        /// This will return all InventoryItems that have the given Category by CategoryDefinition id hash through an enumerator.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition to iterate.</param>
        /// <returns>An enumerator for the InventoryItems that have the given Category.</returns>
        public static IEnumerable<InventoryItem> GetAllByCategory(int categoryHash)
        {
            return InventoryManager.wallet.GetItemsByCategory(categoryHash);
        }

        /// <summary>
        /// Removes an entry of the specified InventoryItem by InventoryItemDefintion id.
        /// </summary>
        /// <param name="definitionId">The id of the InventoryItemDefinition we are removing.</param>
        public static void RemoveItem(string definitionId)
        {
            InventoryManager.wallet.RemoveItem(definitionId);
        }

        /// <summary>
        /// Removes an InventoryItem specified by its InventoryItemDefinition.
        /// </summary>
        /// <param name="definition">The InventoryItemDefinition to remove.</param>
        public static void RemoveItem(InventoryItemDefinition definition)
        {
            InventoryManager.wallet.RemoveItem(definition);
        }

        /// <summary>
        /// Removes an entry of the specified InventoryItemefinition by hash.
        /// </summary>
        /// <param name="definitionHash">The id hash of the InventoryItemDefinition to remove.</param>
        public static void RemoveItem(int definitionHash)
        {
            InventoryManager.wallet.RemoveItem(definitionHash);
        }

        /// <summary>
        /// This will remove all InventoryItems that have the given Category by CategoryDefinition id.
        /// </summary>
        /// <param name="categoryId">The id of the CategoryDefinition we are checking for.</param>
        public static void RemoveItemsByCategory(string categoryId)
        {
            InventoryManager.wallet.RemoveItemsByCategory(categoryId);
        }

        /// <summary>
        /// This will remove all InventoryItems that have the given Category.
        /// </summary>
        /// <param name="categoryDefinition">The CategoryDefinition to remove.</param>
        public static void RemoveItemsByCategory(CategoryDefinition categoryDefinition)
        {
            InventoryManager.wallet.RemoveItemsByCategory(categoryDefinition);
        }

        /// <summary>
        /// This will remove all InventoryItems that have the given Category by CategoryDefinition id hash.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition to remove.</param>
        public static void RemoveItemsByCategory(int categoryHash)
        {
            InventoryManager.wallet.RemoveItemsByCategory(categoryHash);
        }

        /// <summary>
        /// Removes all InventoryItems from the Wallet.
        /// </summary>
        public static void RemoveAll()
        {
            InventoryManager.wallet.RemoveAll();
        }

        /// <summary>
        /// Returns whether or not an InventoryItem exists within the Wallet.
        /// </summary>
        /// <param name="definitionId">The id of the InventoryItemDefinition to find.</param>
        /// <returns>True/False whether or not the InventoryItem is within the Wallet.</returns>
        public static bool ContainsItem(string definitionId)
        {
            return InventoryManager.wallet.ContainsItem(definitionId);
        }

        /// <summary>
        /// Returns whether or not an InventoryItem exists within the Wallet for specified InventoryItemDefinition.
        /// </summary>
        /// <param name="definition">The InventoryItemDefinition to find.</param>
        /// <returns>True/False whether or not the InventoryItem is within the Wallet.</returns>
        public static bool ContainsItem(InventoryItemDefinition definition)
        {
            return InventoryManager.wallet.ContainsItem(definition);
        }

        /// <summary>
        /// Returns whether or not an InventoryItem exists within the Wallet.
        /// </summary>
        /// <param name="definitionHash">The id hash of the InventoryItemDefinition we are checking for.</param>
        /// <returns>True/False whether or not the InventoryItem is within the Wallet.</returns>
        public static bool ContainsItem(int definitionHash)
        {
            return InventoryManager.wallet.ContainsItem(definitionHash);
        }

        /// <summary>
        /// Resets the contents of the Wallet.
        /// </summary>
        public static void Reset()
        {
            InventoryManager.wallet.Reset();
        }

        /// <summary>
        /// Sets the quantity of an InventoryItem by InventoryItemDefinition id.
        /// </summary>
        /// <param name="definitionId">The id of the InventoryItemDefinition we are checking for.</param>
        /// <param name="quantity">The new value we are setting.</param>
        public static void SetQuantity(string definitionId, int quantity)
        {
            InventoryManager.wallet.SetQuantity(definitionId, quantity);
        }

        /// <summary>
        /// Sets the quantity of an InventoryItem by InventoryItemDefinition.
        /// </summary>
        /// <param name="definition">The InventoryItemDefinition to set quantity for.</param>
        /// <param name="quantity">The new quantity value to set.</param>
        public static void SetQuantity(InventoryItemDefinition definition, int quantity)
        {
            InventoryManager.wallet.SetQuantity(definition, quantity);
        }

        /// <summary>
        /// Sets the quantity of the InventoryItem within this Wallet of the specified InventoryItemDefinition by id hash
        /// </summary>
        /// <param name="hash">The id hash of the InventoryItemDefinition to set quantity for.</param>
        /// <param name="quantity">The new quantity value to set.</param>
        public static void SetQuantity(int hash, int quantity)
        {
            InventoryManager.wallet.SetQuantity(hash, quantity);
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an InventoryItem is added to the Wallet.
        /// </summary>
        /// <returns>A CollectionItemEvent fired whenever an InventoryItem is added to the Wallet.</returns>
        public static Inventory.BaseCollectionItemEvent onItemAdded
        {
            get { return InventoryManager.wallet.onItemAdded; }
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an InventoryItem is about to be removed from the Wallet.
        /// </summary>
        /// <returns>A CollectionItemEvent fired whenever an InventoryItem is about to be removed from the Wallet.</returns>
        public static Inventory.BaseCollectionItemEvent onItemWillRemove
        {
            get { return InventoryManager.wallet.onItemWillRemove; }
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an InventoryItem is removed from the Wallet.
        /// </summary>
        /// <returns>A CollectionItemEvent fired whenever an InventoryItem is removed from the Wallet.</returns>
        public static Inventory.BaseCollectionItemEvent onItemRemoved
        {
            get { return InventoryManager.wallet.onItemRemoved; }
        }

        /// <summary>
        /// Callback for when an InventoryItem quantity has changed.
        /// </summary>
        /// <returns>A CollectionItemEvent fired when an InventoryItem quantity has changed.</returns>
        public static Inventory.BaseCollectionItemEvent onItemQuantityChanged
        {
            get { return InventoryManager.wallet.onItemQuantityChanged; }
        }

        /// <summary>
        /// Callback for when an InventoryItem quantity has gone above its minimum.
        /// </summary>
        /// <returns>The CollectionItemEvent fired when an InventoryItem quantity has gone above its minimum.</returns>
        public static Inventory.BaseCollectionItemEvent onItemQuantityOverflow
        {
            get { return InventoryManager.wallet.onItemQuantityOverflow; }
        }
    }
}
