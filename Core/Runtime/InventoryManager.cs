using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Manages all Inventories. Can subscribe to events relevant to Inventories and create and remove them here.
    /// InventoryManager also owns the main and wallet Inventories, as well as all other Inventories of InventoryItems.
    /// The InventoryManager can create Inventories from InventoryDefinitions or default Inventories as needed.
    /// </summary>
    public static class InventoryManager
    {
        /// <summary>
        /// Event fired whenever a new Inventory Item is added.
        /// </summary>
        public static event Action<InventoryItem> itemAdded;

        /// <summary>
        /// Event fired whenever an Inventory Item is removed.
        /// </summary>
        public static event Action<InventoryItem> itemRemoved;

        [ThreadStatic]
        static List<InventoryItem> s_TempItemList;

        static List<InventoryItem> tempItemList
        {
            get
            {
                if (s_TempItemList is null)
                {
                    s_TempItemList = new List<InventoryItem>();
                }
                return s_TempItemList;
            }
        }

        /// <summary>
        /// All items currently managed by this manager.
        /// </summary>
        static ItemLookup s_Items;

        /// <summary>
        /// Current initialization state of this manager.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Accessor to GameFoundation's current DAL.
        /// </summary>
        static IInventoryDataLayer dataLayer => GameFoundation.dataLayer;

        /// <summary>
        /// This is the InventoryCatalog the InventoryManager uses.
        /// </summary>
        /// <returns>
        /// The InventoryCatalog the InventoryManager uses.
        /// </returns>
        public static InventoryCatalog catalog => GameFoundation.catalogs.inventoryCatalog;

        /// <summary>
        /// Initialize this manager using GameFoundation's
        /// <see cref="GameFoundation.dataLayer"/> and <see cref="GameFoundation.catalogs"/>.
        /// </summary>
        internal static void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("InventoryManager is already initialized and cannot be initialized again.");
                return;
            }

            try
            {
                InitializeData();

                IsInitialized = true;
            }
            catch (Exception)
            {
                Uninitialize();

                throw;
            }
        }

        /// <summary>
        /// Parse GameFoundation's data layer's data to fill this manager.
        /// </summary>
        static void InitializeData()
        {
            var inventoryManagerData = dataLayer.GetData();

            s_Items = new ItemLookup(inventoryManagerData.items.Length);

            foreach (var itemData in inventoryManagerData.items)
            {
                if (string.IsNullOrEmpty(itemData.definitionKey) ||
                    string.IsNullOrEmpty(itemData.id))
                {
                    continue;
                }

                var itemDefinition = catalog.FindItem(itemData.definitionKey);
                if (itemDefinition == null)
                {
                    continue;
                }

                var item = new InventoryItem(itemDefinition, itemData.id);
                s_Items.Add(item);
            }
        }

        /// <summary>
        /// Uninitialize this manager by discarding all managed items
        /// and resetting all fields.
        /// </summary>
        internal static void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            RemoveAllItemsInternal();
            s_Items = null;
            s_TempItemList = null;

            IsInitialized = false;
        }

        /// <summary>
        /// Fills the given list with all items in the manager.
        /// Note: this returns the current state of all items.  To ensure
        /// that there are no invalid or duplicate entries, the list will always 
        /// be cleared and 'recycled' (i.e. updated) with current data.
        /// </summary>
        /// <param name="target">The collection to clear and fill with updated data.</param>
        /// <returns>The number of items copied</returns>
        public static int GetItems(ICollection<InventoryItem> target)
        {
            ThrowIfNotInitialized();
            return Tools.Copy(s_Items.Values, target);
        }

        /// <summary> 
        /// Returns an array of all items in the manager.
        /// </summary> 
        /// <returns>An array of all items in the manager.</returns>
        public static InventoryItem[] GetItems()
        {
            ThrowIfNotInitialized();
            return Tools.ToArray(s_Items.Values);
        }

        /// <summary> 
        /// Returns an item with the Id wanted.
        /// </summary>
        /// <param name="id">
        /// The Id of the item wanted.
        /// </param>
        /// <returns>
        /// Return the item with the given <paramref name="id"/> if it exists;
        /// return null otherwise.
        /// </returns>
        public static InventoryItem FindItem(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));
            ThrowIfNotInitialized();
            s_Items.TryGetValue(id, out var item);

            return item;
        }

        /// <summary> 
        /// Get the item with the given <paramref name="instanceId"/>.
        /// </summary>
        /// <param name="instanceId">
        /// Instance id of the item to look for.
        /// </param>
        /// <returns>
        /// Return the item with the given <paramref name="instanceId"/> if it exists;
        /// return null otherwise.
        /// </returns>
        internal static InventoryItem FindItem(int instanceId)
        {
            s_Items.TryGetValue(instanceId, out var item);

            return item;
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">The tag used to filter the items.
        /// </param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of items copied</returns>
        static int FindItemsByTagInternal(Tag tag, ICollection<InventoryItem> target = null)
        {
            target?.Clear();

            var count = 0;
            foreach (var item in s_Items.Values)
            {
                if (item.definition.HasTag(tag))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">The tag used to filter the items.
        /// </param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of items copied</returns>
        public static int FindItemsByTag(Tag tag, ICollection<InventoryItem> target)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            return FindItemsByTagInternal(tag, target);
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">The tag used to filter the items.
        /// </param>
        /// <returns>The filtered items.</returns>
        public static InventoryItem[] FindItemsByTag(Tag tag)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));

            try
            {
                FindItemsByTagInternal(tag, tempItemList);
                return tempItemList.ToArray();
            }
            finally
            {
                tempItemList.Clear();
            }
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="key">The identifier of the tag used to filter the items.
        /// </param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of items copied</returns>
        public static int FindItemsByTag(string key, ICollection<InventoryItem> target)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            var tag = GameFoundation.catalogs.tagCatalog.GetTagOrDie(key, nameof(key));

            return FindItemsByTagInternal(tag, target);
        }

        /// <summary>
        /// Gets items filtered items with the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="key">The tag identifier used to filter the items.
        /// </param>
        /// <returns>The filtered items.</returns>
        public static InventoryItem[] FindItemsByTag(string key)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            var tag = GameFoundation.catalogs.tagCatalog.GetTagOrDie(key, nameof(key));

            try
            {
                FindItemsByTagInternal(tag, tempItemList);
                return tempItemList.ToArray();
            }
            finally
            {
                tempItemList.Clear();
            }
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">The <see cref="InventoryItemDefinition"/>
        /// used to filter the items</param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of filtered items.</returns>
        static int FindItemsByDefinitionInternal
            (InventoryItemDefinition definition, ICollection<InventoryItem> target = null)
        {
            target?.Clear();

            var count = 0;
            foreach (var item in s_Items.Values)
            {
                if (ReferenceEquals(item.definition, definition))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">The <see cref="InventoryItemDefinition"/>
        /// used to filter the items</param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of filtered items.</returns>
        public static int FindItemsByDefinition(InventoryItemDefinition definition, ICollection<InventoryItem> target)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));
            return FindItemsByDefinitionInternal(definition, target);
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">The <see cref="InventoryItemDefinition"/>
        /// used to filter the items</param>
        /// <returns>The filtered items.</returns>
        public static InventoryItem[] FindItemsByDefinition(InventoryItemDefinition definition)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));

            try
            {
                FindItemsByDefinitionInternal(definition, tempItemList);
                return tempItemList.ToArray();
            }
            finally
            {
                tempItemList.Clear();
            }
        }

        /// <summary>
        /// Get all items created from the definition with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The identifier of the <see cref="InventoryItemDefinition"/>
        /// used to filter the items</param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of filtered items.</returns>
        public static int FindItemsByDefinition(string key, ICollection<InventoryItem> target)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            var definition = Tools.GetInventoryItemDefinitionOrDie
                (key, nameof(key));

            return FindItemsByDefinitionInternal(definition, target);
        }

        /// <summary>
        /// Get all items created from the definition with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The identifier of the <see cref="InventoryItemDefinition"/>
        /// used to filter the items</param>
        /// <returns>The filtered items.</returns>
        public static InventoryItem[] FindItemsByDefinition(string key)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            var definition = Tools.GetInventoryItemDefinitionOrDie
                (key, nameof(key));

            try
            {
                FindItemsByDefinitionInternal(definition, tempItemList);
                return tempItemList.ToArray();
            }
            finally
            {
                tempItemList.Clear();
            }
        }

        /// <summary>
        /// Throws an exception if the InventoryManager has not been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if not initialized.</exception>
        public static void ThrowIfNotInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Error: GameFoundation.Initialize() must be called before the InventoryManager is used.");
            }
        }

        /// <summary>
        /// Delete the given <paramref name="item"/> but does not synchronize with the data layer.
        /// </summary>
        internal static bool RemoveItemInternal(InventoryItem item)
        {
            Tools.ThrowIfArgNull(item, nameof(item));

            if (item.hasBeenDiscarded)
                return false;

            if (!s_Items.Remove(item.instanceId))
            {
                return false;
            }

            //Events need to happen before the item is discarded for them to access to the item's data.
            NotificationSystem.FireNotification(NotificationType.Destroyed, item);

            itemRemoved?.Invoke(item);

            //Cleanup the removed item to make sure it can't be used anymore.
            item.Discard();

            return true;
        }

        /// <summary>
        /// Delete the item with the given <paramref name="itemId"/> but does not synchronize with the data layer.
        /// </summary>
        internal static bool RemoveItemInternal(string itemId)
        {
            Tools.ThrowIfArgNull(itemId, nameof(itemId));
            ThrowIfNotInitialized();

            var found = s_Items.TryGetValue(itemId, out var item);
            return found && RemoveItemInternal(item);
        }

        /// <summary>
        /// This method will remove the given Item.
        /// </summary>
        /// <param name="item">The Item instance we want to remove.</param>
        /// <returns>Whether or not the Item was successfully removed.</returns>
        public static bool RemoveItem(InventoryItem item)
        {
            Tools.ThrowIfArgNull(item, nameof(item));
            ThrowIfNotInitialized();

            //Store item's id since it will be discarded.
            var itemId = item.id;
            var itemWasRemoved = RemoveItemInternal(item);

            if (itemWasRemoved)
            {
                SyncDeleteItem(itemId);
            }

            return itemWasRemoved;
        }

        /// <summary>
        /// This method will remove the Item with the given Item Hash.
        /// </summary>
        /// <param name="itemId">
        /// Id of the item we want to remove.
        /// </param>
        /// <returns>Whether or not the Item was successfully removed.</returns>
        public static bool RemoveItem(string itemId)
        {
            Tools.ThrowIfArgNull(itemId, nameof(itemId));
            ThrowIfNotInitialized();

            var found = s_Items.TryGetValue(itemId, out var item);
            return found && RemoveItem(item);
        }

        /// <summary>
        /// Delete all items created from the given <paramref name="itemDefinition"/>
        /// but does not synchronize with the data layer.
        /// </summary>
        internal static int RemoveItemsByDefinitionInternal(InventoryItemDefinition itemDefinition)
        {
            Tools.ThrowIfArgNull(itemDefinition, nameof(itemDefinition));
            ThrowIfNotInitialized();

            var removedCount = 0;
            try
            {
                foreach (var item in s_Items.Values)
                {
                    if (ReferenceEquals(item.definition, itemDefinition))
                    {
                        tempItemList.Add(item);
                    }
                }

                removedCount = tempItemList.Count;

                foreach (var item in tempItemList)
                {
                    RemoveItemInternal(item);
                }
            }
            finally
            {
                tempItemList.Clear();
            }

            return removedCount;
        }

        /// <summary>
        /// This method will remove the Item that uses the given ItemDefinition.
        /// </summary>
        /// <param name="itemDefinition">The InventoryItemDefinition we want to
        /// remove.</param>
        /// <returns>The amount of items that were removed.</returns>
        public static int RemoveItemsByDefinition(InventoryItemDefinition itemDefinition)
        {
            Tools.ThrowIfArgNull(itemDefinition, nameof(itemDefinition));
            ThrowIfNotInitialized();

            var removedCount = 0;
            try
            {
                foreach (var item in s_Items.Values)
                {
                    if (ReferenceEquals(item.definition, itemDefinition))
                    {
                        tempItemList.Add(item);
                    }
                }

                removedCount = tempItemList.Count;

                foreach (var item in tempItemList)
                {
                    RemoveItem(item);
                }
            }
            finally
            {
                tempItemList.Clear();
            }

            return removedCount;
        }

        /// <summary>
        /// Delete all items created from the definition with the given <paramref name="definitionKey"/>
        /// but does not synchronize with the data layer.
        /// </summary>
        internal static int RemoveItemsByDefinitionInternal(string definitionKey)
        {
            ThrowIfNotInitialized();

            var catalogItem = Tools.GetInventoryItemDefinitionOrDie(definitionKey, nameof(definitionKey));

            return RemoveItemsByDefinitionInternal(catalogItem);
        }

        /// <summary>
        /// This method will remove the Item that uses the InventoryItemDefinition with the given Hash.
        /// </summary>
        /// <param name="key">The Identifier of the InventoryItemDefinition we want to remove.</param>
        /// <returns>The amount of items that were removed.</returns>
        public static int RemoveItemsByDefinition(string key)
        {
            ThrowIfNotInitialized();

            var catalogItem = Tools.GetInventoryItemDefinitionOrDie(key, nameof(key));

            return RemoveItemsByDefinition(catalogItem);
        }

        /// <summary>
        /// Delete all items but does not synchronize with the data layer.
        /// </summary>
        /// <returns>
        /// Return the number of deleted items.
        /// </returns>
        internal static int RemoveAllItemsInternal()
        {
            var count = s_Items.Count;

            try
            {
                Tools.Copy(s_Items.Values, tempItemList);

                foreach (var item in tempItemList)
                {
                    RemoveItemInternal(item);
                }
            }
            finally
            {
                tempItemList.Clear();
            }

            return count;
        }

        /// <summary>
        /// Removes all the items from the player inventory.
        /// </summary>
        /// <returns>The number of items removed.</returns>
        public static int RemoveAllItems()
        {
            var count = s_Items.Count;

            try
            {
                Tools.Copy(s_Items.Values, tempItemList);

                foreach (var item in tempItemList)
                {
                    RemoveItem(item);
                }
            }
            finally
            {
                tempItemList.Clear();
            }

            return count;
        }

        /// <summary>
        /// This will create a new <see cref="InventoryItem"/> based on
        /// the <see cref="InventoryItemDefinition"/> matching the given <paramref name="key"/>.
        /// It will not sync with the data layer.
        /// </summary>
        /// <param name="key">
        /// The Identifier of the <see cref="InventoryItemDefinition"/> to assign to this Item.
        /// </param>
        /// <param name="id">
        /// The id to give to the created item.
        /// If <c>null</c>, it will be automatically generated.
        /// </param>
        /// <returns>
        /// The newly created item based on specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        internal static InventoryItem CreateItemInternal(string key, string id = null)
        {
            ThrowIfNotInitialized();

            var catalogItem = Tools.GetInventoryItemDefinitionOrDie(key, nameof(key));

            return CreateItemInternal(catalogItem, id);
        }

        /// <summary>
        /// This will create a new <see cref="InventoryItem"/> based on
        /// the <see cref="InventoryItemDefinition"/> matching the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The Identifier of the <see cref="InventoryItemDefinition"/> to assign to this Item.
        /// </param>
        /// <returns>
        /// The newly created item based on specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        public static InventoryItem CreateItem(string key)
        {
            ThrowIfNotInitialized();

            var catalogItem = Tools.GetInventoryItemDefinitionOrDie(key, nameof(key));

            return CreateItem(catalogItem);
        }

        /// <summary>
        /// This will create a new <see cref="InventoryItem"/> based on
        /// the given <see cref="InventoryItemDefinition"/>.
        /// It will not sync with the data layer.
        /// </summary>
        /// <param name="itemDefinition">
        /// The <see cref="InventoryItemDefinition"/> to assign to this Item.
        /// </param>
        /// <param name="id">
        /// The id to give to the created item.
        /// If <c>null</c>, it will be automatically generated.
        /// </param>
        /// <returns>
        /// The newly created item based on specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        internal static InventoryItem CreateItemInternal(InventoryItemDefinition itemDefinition, string id = null)
        {
            Tools.ThrowIfArgNull(itemDefinition, nameof(itemDefinition));

            var newItem = new InventoryItem(itemDefinition, id);
            s_Items.Add(newItem);

            itemAdded?.Invoke(newItem); 

            NotificationSystem.FireNotification(NotificationType.Created, newItem);

            return newItem;
        }

        /// <summary>
        /// This will create a new <see cref="InventoryItem"/> based on
        /// the given <see cref="InventoryItemDefinition"/>.
        /// </summary>
        /// <param name="itemDefinition">
        /// The <see cref="InventoryItemDefinition"/> to assign to this Item.
        /// </param>
        /// <returns>
        /// The newly created item based on specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        public static InventoryItem CreateItem(InventoryItemDefinition itemDefinition)
        {
            var newItem = CreateItemInternal(itemDefinition);

            // The inventory must be synchronized after its constructor is called,
            // for the game item id to be properly updated.
            SyncCreateItem(newItem.definition.key, newItem.id);

            return newItem;
        }

        /// <summary>
        /// Synchronizes the creation of the item with the data layer.
        /// </summary>
        /// <param name="key">The Identifier of the InventoryItemDefinition to assign this Item.</param>
        /// <param name="id">The Identifier this item will have.</param>
        internal static void SyncCreateItem(string key, string id)
            => dataLayer.CreateItem(key, id, Completer.None);

        /// <summary>
        /// Synchronizes the removal of item from an inventory with the data layer
        /// </summary>
        /// <param name="itemId">The Identifier of the item to delete.</param>
        internal static void SyncDeleteItem(string itemId)
            => dataLayer.DeleteItem(itemId, Completer.None);
    }
}
