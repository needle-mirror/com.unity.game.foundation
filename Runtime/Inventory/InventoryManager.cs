using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DataPersistence;
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
        [ThreadStatic]
        static List<InventoryItem> ts_TempItemList;

        static List<InventoryItem> s_TempItemList
        {
            get
            {
                if (ts_TempItemList is null) ts_TempItemList = new List<InventoryItem>();
                return ts_TempItemList;
            }
        }

        /// <summary>
        /// Fills the given list with all items in the manager.
        /// Note: this returns the current state of all items.  To ensure
        /// that there are no invalid or duplicate entries, the list will always 
        /// be cleared and 'recycled' (i.e. updated) with current data.
        /// </summary>
        /// <param name="target">The collection to clear and fill with updated data.</param>
        /// <returns>The number of items copied</returns>
        public static int GetItems(ICollection<InventoryItem> target = null)
        {
            ThrowIfNotInitialized();
            return Tools.Copy(m_Items.Values, target);
        }

        /// <summary> 
        /// Returns an array of all items in the manager.
        /// </summary> 
        /// <returns>An array of all items in the manager.</returns>
        public static InventoryItem[] GetItems()
        {
            ThrowIfNotInitialized();
            return Tools.ToArray(m_Items.Values);
        }

        /// <summary> 
        /// Returns an item with the Id wanted.
        /// </summary>
        /// /// <param name="id">The Id of the item wanted.</param>
        /// <returns>The item with the Id wanted.</returns>
        internal static InventoryItem FindItemInternal(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));
            m_Items.TryGetValue(id, out var item);
            return item;
        }

        /// <summary> 
        /// Returns an item with the Id wanted.
        /// </summary>
        /// /// <param name="id">The Id of the item wanted.</param>
        /// <returns>The item with the Id wanted.</returns>
        public static InventoryItem FindItem(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));
            ThrowIfNotInitialized();
            m_Items.TryGetValue(id, out var item);
            return item;
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="category"/>.
        /// </summary>
        /// <param name="category">The category used to filter the items.
        /// </param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of items copied</returns>
        static int FindItemsByCategoryInternal
            (Category category, ICollection<InventoryItem> target = null)
        {
            target?.Clear();

            var count = 0;
            foreach (var item in m_Items.Values)
            {
                if (item.definition.HasCategory(category))
                {
                    count++;
                    target?.Add(item);
                }
            }

            return count;
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="category"/>.
        /// </summary>
        /// <param name="category">The category used to filter the items.
        /// </param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of items copied</returns>
        public static int FindItemsByCategory
            (Category category, ICollection<InventoryItem> target = null)
        {
            Tools.ThrowIfArgNull(category, nameof(category));
            return FindItemsByCategoryInternal(category, target);
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="category"/>.
        /// </summary>
        /// <param name="category">The category used to filter the items.
        /// </param>
        /// <returns>The filtered items.</returns>
        public static InventoryItem[] FindItemsByCategory(Category category)
        {
            Tools.ThrowIfArgNull(category, nameof(category));

            try
            {
                FindItemsByCategoryInternal(category, s_TempItemList);
                return s_TempItemList.ToArray();
            }
            finally
            {
                s_TempItemList.Clear();
            }
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="category"/>.
        /// </summary>
        /// <param name="categoryId">The inventory id used to filter the items.
        /// </param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of items copied</returns>
        public static int FindItemsByCategory
            (string categoryId, ICollection<InventoryItem> target = null)
        {
            Tools.ThrowIfArgNull(categoryId, nameof(categoryId));

            var catalog = GameFoundation.catalogs.inventoryCatalog;
            var category = catalog.GetCategoryOrDie(categoryId, nameof(categoryId));

            return FindItemsByCategoryInternal(category, target);
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="category"/>.
        /// </summary>
        /// <param name="categoryId">The inventory id used to filter the items.
        /// </param>
        /// <returns>The filtered items.</returns>
        public static InventoryItem[] FindItemsByCategory(string categoryId)
        {
            Tools.ThrowIfArgNull(categoryId, nameof(categoryId));

            var catalog = GameFoundation.catalogs.inventoryCatalog;
            var category = catalog.GetCategoryOrDie(categoryId, nameof(categoryId));

            try
            {
                FindItemsByCategoryInternal(category, s_TempItemList);
                return s_TempItemList.ToArray();
            }
            finally
            {
                s_TempItemList.Clear();
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
            foreach (var item in m_Items.Values)
            {
                if (item.definition == definition)
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
        public static int FindItemsByDefinition
            (InventoryItemDefinition definition, ICollection<InventoryItem> target = null)
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
        public static InventoryItem[] FindItemsByDefinition
            (InventoryItemDefinition definition)
        {
            Tools.ThrowIfArgNull(definition, nameof(definition));

            try
            {
                FindItemsByDefinitionInternal(definition, s_TempItemList);
                return s_TempItemList.ToArray();
            }
            finally
            {
                s_TempItemList.Clear();
            }
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="definition"/>.
        /// </summary>
        /// <param name="definitionId">The id of the <see cref="InventoryItemDefinition"/>
        /// used to filter the items</param>
        /// <param name="target">The target collection where filtered items are
        /// copied</param>
        /// <returns>The number of filtered items.</returns>
        public static int FindItemsByDefinition
            (string definitionId, ICollection<InventoryItem> target = null)
        {
            Tools.ThrowIfArgNull(definitionId, nameof(definitionId));

            var definition = Tools.GetInventoryItemDefinitionOrDie
                (definitionId, nameof(definitionId));

            return FindItemsByDefinitionInternal(definition, target);
        }

        /// <summary>
        /// Gets items filtered with the specified <paramref name="definition"/>.
        /// </summary>
        /// <param name="definitionId">The id of the <see cref="InventoryItemDefinition"/>
        /// used to filter the items</param>
        /// <returns>The filtered items.</returns>
        public static InventoryItem[] FindItemsByDefinition(string definitionId)
        {
            Tools.ThrowIfArgNull(definitionId, nameof(definitionId));

            var definition = Tools.GetInventoryItemDefinitionOrDie
                (definitionId, nameof(definitionId));

            try
            {
                FindItemsByDefinitionInternal(definition, s_TempItemList);
                return s_TempItemList.ToArray();
            }
            finally
            {
                s_TempItemList.Clear();
            }
        }

        private static Dictionary<string, InventoryItem> m_Items = null;

        static IInventoryDataLayer s_DataLayer;

        internal static void Initialize(IInventoryDataLayer dataLayer)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("InventoryManager is already initialized and cannot be initialized again.");
                return;
            }

            s_DataLayer = dataLayer;

            try
            {
                InitializeData();

                m_IsInitialized = true;
            }
            catch (Exception)
            {
                Uninitialize();

                throw;
            }
        }

        static void InitializeData()
        {
            var inventoryManagerData = s_DataLayer.GetData();

            m_Items = new Dictionary<string, InventoryItem>();

            if (inventoryManagerData.items.Length > 0)
            {
                FillFromItemsData(inventoryManagerData);
                return;
            }
        }

        internal static void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            RemoveAllItemsInternal();
            m_Items = null;

            m_IsInitialized = false;
            s_DataLayer = null;
        }

        internal static void FillFromItemsData(InventoryManagerSerializableData managerData)
        {
            foreach (var itemData in managerData.items)
            {
                if (string.IsNullOrEmpty(itemData.definitionId) ||
                    string.IsNullOrEmpty(itemData.id))
                {
                    continue;
                }

                var itemDefinition = catalog.FindItem(itemData.definitionId);
                if (itemDefinition == null)
                {
                    continue;
                }

                //TODO
                var item = new InventoryItem(itemDefinition, itemData.id);

                m_Items.Add(item.id, item);
            }
        }

        /// <summary>
        /// Returns the current initialization state of the InventoryManager.
        /// </summary>
        /// <returns>The current initialization state of the InventoryManager.</returns>
        public static bool IsInitialized
        {
            get { return m_IsInitialized; }
        }

        private static bool m_IsInitialized = false;

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
        /// This is the InventoryCatalog the InventoryManager uses.
        /// </summary>
        /// <returns>The InventoryCatalog the InventoryManager uses.</returns>
        public static InventoryCatalog catalog => GameFoundation.catalogs.inventoryCatalog;

        /// <summary>
        /// Fired whenever a new Inventory Item is added.
        /// </summary>
        /// <returns>The Inventory Manager Event fired whenever a new Inventory Item is added.</returns>
        public static event GameItemEventHandler itemAdded;

        /// <summary>
        /// Fired whenever an Inventory Item is removed.
        /// </summary>
        /// <returns>The Inventory Manager Event fired whenever an Inventory Item is removed.</returns>
        public static event GameItemEventHandler itemRemoved;

        static void onItemAdded(GameItem item) => itemAdded?.Invoke(item);

        static void onItemRemoved(GameItem item) => itemRemoved?.Invoke(item);

        // Deletes an item from the manager, but does not sync to the data layer.
        internal static bool RemoveItemInternal(InventoryItem item)
        {
            Tools.ThrowIfArgNull(item, nameof(item));

            if (item.discarded) return false;

            if (!m_Items.Remove(item.id))
            {
                return false;
            }

            // notification and analytics need to happen before the item is disposed
            NotificationSystem.FireNotification(NotificationType.Destroyed, item);

            onItemRemoved(item);
            item.onRemoved();

            // immediately discard all game items to remove all their stats and the items themselves from GameItemLookup
            item.Discard();

            return true;
        }

        // This method will remove the given Item, but will not sync with the data layer.
        internal static bool RemoveItemInternal(string itemId)
        {
            Tools.ThrowIfArgNull(itemId, nameof(itemId));
            ThrowIfNotInitialized();

            if (!m_Items.TryGetValue(itemId, out var item))
            {
                return false;
            }

            return RemoveItemInternal(item);
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

            bool itemWasRemoved = RemoveItemInternal(item);

            if (itemWasRemoved)
            {
                SyncDeleteItem(item.id);
            }

            return itemWasRemoved;
        }

        /// <summary>
        /// This method will remove the Item with the given Item Hash.
        /// </summary>
        /// <param name="itemId">GameItemId Item instance we want to remove.</param>
        /// <returns>Whether or not the Item was successfully removed.</returns>
        public static bool RemoveItem(string itemId)
        {
            Tools.ThrowIfArgNull(itemId, nameof(itemId));
            ThrowIfNotInitialized();

            var found = m_Items.TryGetValue(itemId, out var item);
            if (!found)
            {
                return false;
            }

            return RemoveItem(item);
        }

        /// This method will remove the Item that uses the given ItemDefinition, but will not sync to the data layer.
        internal static int RemoveItemsByDefinitionInternal(InventoryItemDefinition itemDefinition)
        {
            Tools.ThrowIfArgNull(itemDefinition, nameof(itemDefinition));
            ThrowIfNotInitialized();

            var removedCount = 0;
            try
            {
                foreach (var item in m_Items.Values)
                {
                    if (item.definition == itemDefinition)
                    {
                        s_TempItemList.Add(item);
                    }
                }

                removedCount = s_TempItemList.Count;

                foreach (var item in s_TempItemList)
                {
                    RemoveItemInternal(item);
                }
            }
            finally
            {
                s_TempItemList.Clear();
            }

            return removedCount;
        }

        /// <summary>
        /// This method will remove the Item that uses the given ItemDefinition.
        /// </summary>
        /// <param name="itemDefinition">The InventoryItemDefinition we want to
        /// remove.</param>
        /// <returns>The amount of items that were removed.</returns>
        public static int RemoveItemsByDefinition
            (InventoryItemDefinition itemDefinition)
        {
            Tools.ThrowIfArgNull(itemDefinition, nameof(itemDefinition));
            ThrowIfNotInitialized();

            var removedCount = 0;
            try
            {
                foreach (var item in m_Items.Values)
                {
                    if (item.definition == itemDefinition)
                    {
                        s_TempItemList.Add(item);
                    }
                }

                removedCount = s_TempItemList.Count;

                foreach (var item in s_TempItemList)
                {
                    RemoveItem(item);
                }
            }
            finally
            {
                s_TempItemList.Clear();
            }

            return removedCount;
        }

        // This method will remove the Item that uses the InventoryItemDefinition with the given Hash, but will not sync to the data layer.
        internal static int RemoveItemsByDefinitionInternal(string definitionId)
        {
            ThrowIfNotInitialized();

            var catalogItem = Tools.GetInventoryItemDefinitionOrDie(definitionId, nameof(definitionId));

            return RemoveItemsByDefinitionInternal(catalogItem);
        }

        /// <summary>
        /// This method will remove the Item that uses the InventoryItemDefinition with the given Hash.
        /// </summary>
        /// <param name="definitionId">The Id of the InventoryItemDefinition we want to remove.</param>
        /// <returns>The amount of items that were removed.</returns>
        public static int RemoveItemsByDefinition(string definitionId)
        {
            ThrowIfNotInitialized();

            var catalogItem = Tools.GetInventoryItemDefinitionOrDie
                (definitionId, nameof(definitionId));

            return RemoveItemsByDefinition(catalogItem);
        }

        // Removes all the items from the player inventory, but does not sync to the data layer.
        internal static int RemoveAllItemsInternal()
        {
            var count = m_Items.Count;

            try
            {
                Tools.Copy(m_Items.Values, s_TempItemList);

                foreach (var item in s_TempItemList)
                {
                    RemoveItemInternal(item);
                }
            }
            finally
            {
                s_TempItemList.Clear();
            }

            return count;
        }

        /// <summary>
        /// Removes all the items from the player inventory.
        /// </summary>
        /// <returns>The number of items removed.</returns>
        public static int RemoveAllItems()
        {
            var count = m_Items.Count;

            try
            {
                Tools.Copy(m_Items.Values, s_TempItemList);

                foreach (var item in s_TempItemList)
                {
                    RemoveItem(item);
                }
            }
            finally
            {
                s_TempItemList.Clear();
            }

            return count;
        }

        /// <summary>
        /// This will create a new <see cref="InventoryItem"/> based on
        /// the <see cref="InventoryItemDefinition"/> matching the given id.
        /// It will not sync with the data layer.
        /// </summary>
        /// <param name="definitionId">
        /// The Id of the <see cref="InventoryItemDefinition"/> to assign to this Item.
        /// </param>
        /// <param name="id">
        /// The id to give to the created item.
        /// If <c>null</c>, it will be automatically generated.
        /// </param>
        /// <returns>
        /// The newly created item based on specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        internal static InventoryItem CreateItemInternal(string definitionId, string id = null)
        {
            ThrowIfNotInitialized();

            var catalogItem = Tools.GetInventoryItemDefinitionOrDie(definitionId, nameof(definitionId));

            return CreateItemInternal(catalogItem, id);
        }

        /// <summary>
        /// This will create a new <see cref="InventoryItem"/> based on
        /// the <see cref="InventoryItemDefinition"/> matching the given id.
        /// </summary>
        /// <param name="definitionId">
        /// The Id of the <see cref="InventoryItemDefinition"/> to assign to this Item.
        /// </param>
        /// <returns>
        /// The newly created item based on specified <see cref="InventoryItemDefinition"/>.
        /// </returns>
        public static InventoryItem CreateItem(string definitionId)
        {
            ThrowIfNotInitialized();

            var catalogItem = Tools.GetInventoryItemDefinitionOrDie(definitionId, nameof(definitionId));

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
            m_Items.Add(newItem.id, newItem);

            newItem.Initialize();

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
            SyncCreateItem(newItem.definition.id, newItem.id);

            return newItem;
        }

        /// <summary>
        /// Synchronizes the creation of the item with the data layer.
        /// </summary>
        /// <param name="definitionId">The Id of the InventoryItemDefinition to assign this Item.</param>
        /// <param name="id">The Id this item will have.</param>
        internal static void SyncCreateItem(string definitionId, string id)
            => s_DataLayer.CreateItem(definitionId, id, Completer.None);

        /// <summary>
        /// Synchronizes the removal of item from an inventory with the data layer
        /// </summary>
        /// <param name="itemId">The Id of the item to delete.</param>
        internal static void SyncDeleteItem(string itemId)
            => s_DataLayer.DeleteItem(itemId, Completer.None);
    }
}
