using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// A BaseCollection contains data about a certain types of Items at runtime. 
    /// For example, an Inventory is a BaseCollection of InventoryItems, and
    /// the Inventory can be saved and loaded as part of a savegame system.
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this Collection uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this Collection uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this Collection uses.</typeparam>
    /// <typeparam name="T4">The type of Items this Collection uses.</typeparam>
    public abstract class BaseCollection<T1, T2, T3, T4> : GameItem
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T3, T4>
        where T4 : BaseItem<T3, T4>
    {
        protected BaseCollection(T1 definition, string id = "") : base(definition, id)
        {
            // save off definition used for this Collection
            m_Definition = definition;

            // iterate all default Items in the Collection's CollectionDefinition (if any) and add them to the Collection
            AddAllDefaultItems();
        }

        /// <summary>
        /// This is a UnityEvent that takes in a single Collection as the parameter.
        /// </summary>
        public class BaseCollectionEvent : UnityEvent<T2> {}

        /// <summary>
        /// This is a UnityEvent that takes in a single Item as the parameter.
        /// </summary>
        public class BaseCollectionItemEvent : UnityEvent<T4> {}

        protected BaseCollectionEvent m_OnCollectionReset = new BaseCollectionEvent();
        protected BaseCollectionItemEvent m_OnItemAdded = new BaseCollectionItemEvent();
        protected BaseCollectionItemEvent m_OnItemWillRemove = new BaseCollectionItemEvent();
        protected BaseCollectionItemEvent m_OnItemRemoved = new BaseCollectionItemEvent();
        protected BaseCollectionItemEvent m_OnItemQuantityChanged = new BaseCollectionItemEvent();
        protected BaseCollectionItemEvent m_OnItemQuantityOverflow = new BaseCollectionItemEvent();

        /// <summary>
        /// Fired whenever a Collection is reset.
        /// </summary>
        /// <returns>BaseCollectionEvent for Collection reset.</returns>
        public BaseCollectionEvent onCollectionReset
        {
            get { return m_OnCollectionReset; }
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an Item is added to this Collection.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item added.</returns>
        public BaseCollectionItemEvent onItemAdded
        {
            get { return m_OnItemAdded; }
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an Item is about to be removed from this Collection.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item about to be removed.</returns>
        public BaseCollectionItemEvent onItemWillRemove
        {
            get { return m_OnItemWillRemove; }
        }

        /// <summary>
        /// This is a callback that will be invoked whenever an Item is removed from this Collection.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item added.</returns>
        public BaseCollectionItemEvent onItemRemoved
        {
            get { return m_OnItemRemoved; }
        }

        /// <summary>
        /// Callback for when an Item intValue has changed.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item quantity changed.</returns>
        public BaseCollectionItemEvent onItemQuantityChanged
        {
            get { return m_OnItemQuantityChanged; }
        }

        /// <summary>
        /// Callback for when an Item intValue has gone above its maximum.
        /// </summary>
        /// <returns>BaseCollectionItemEvent for Item overflow (quantity too large).</returns>
        public BaseCollectionItemEvent onItemQuantityOverflow
        {
            get { return m_OnItemQuantityOverflow; }
        }

        [SerializeField]
        protected new T1 m_Definition;

        /// <summary>
        /// The CollectionDefinition of this Collection which determines the default Items and quantities.
        /// </summary>
        /// <returns>CollectionDefinition for this Collection.</returns>
        public new T1 definition
        {
            get { return m_Definition; }
        }

        /// <summary>
        /// Helper property for easily accessing the id of this Collection's CollectionDefinition's id.
        /// </summary>
        /// <returns>CollectionDefinition id string for this Collection.</returns>
        public string definitionId
        {
            get { return m_Definition?.id; }
        }

        /// <summary>
        /// Helper property for easily accessing the id hash of this Collection's CollectionDefinition's id hash.
        /// </summary>
        /// <returns>CollectionDefinition id hash for this Collection.</returns>
        public int definitionHash
        {
            get { return m_Definition != null ? m_Definition.hash : 0; }
        }

        protected Dictionary<int, T4> m_ItemsInCollection = new Dictionary<int, T4>();

        /// <summary>
        /// Enumerator for easily iterating through Items in this Collectino.
        /// </summary>
        /// <returns>An enumerator for easily iterating through Items in this Collection.</returns>
        public IEnumerable<T4> items
        {
            get { return m_ItemsInCollection.Values; }
        }

        /// <summary>
        /// Overload for the square brack operator to access Items by ItemDefinition id string.
        /// </summary>
        /// <param name="definitionId">The id of the ItemDefinition we are searching for.</param>
        /// <returns>Specified Item by ItemDefinition id string.</returns>
        public T4 this[string definitionId]
        {
            get { return GetItemByDefinition(definitionId); }
        }

        /// <summary>
        /// Overload for the square brack operator to access Items by ItemDefinition id hash.
        /// </summary>
        /// <param name="definitionHash">The id hash of the ItemDefinition we are searching for.</param>
        /// <returns>Specified Item by ItemDefinition id hash.</returns>
        public T4 this[int definitionHash]
        {
            get { return GetItemByDefinition(definitionHash); }
        }

        /// <summary>
        /// Overload for the square brack operator to access Items by ItemDefinition
        /// </summary>
        /// <param name="definition">The Item we are searching for</param>
        /// <returns>Specified Item by ItemDefinition.</returns>
        public T4 this[T3 definition]
        {
            get { return GetItemByDefinition(definition); }
        }

        /// <summary>
        /// Adds a new entry of the specified ItemDefinition as an Inventory Item to this collection. Returns the new (or existing) reference to the instance.
        /// Returns the new reference to the instance when the Item didn't already exist in the Collection and
        /// returns an existing reference when to the instance when the Item was already in the Collection.
        /// </summary>
        /// <param name="definition">The ItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this instance we are adding.</param>
        /// <returns>The reference to the instance that was added, or null if ItemDefinition is invalid.</returns>
        public T4 AddItem(T3 definition, int quantity = 1)
        {
            if (definition == null)
            {
                Debug.LogWarning("Null definition given, this will not be added to the collection.");
                return null;
            }
            
            return AddItem(definition.hash, quantity);
        }

        /// <summary>
        /// Adds a new entry of the specified ItemDefinition by id as an Inventory Item to this collection. Returns the new (or existing) reference to the instance.
        /// Returns the new reference to the instance when the Item didn't already exist in the Collection and
        /// returns an existing reference when to the instance when the Item was already in the Collection.
        /// </summary>
        /// <param name="definitionId">The id of the ItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this instance we are adding.</param>
        /// <returns>The reference to the instance that was added, or null if definitionId is invalid.</returns>
        public T4 AddItem(string definitionId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(definitionId))
            {
                Debug.LogWarning("Null or empty id given, this will not be added to the collection.");
                return null;
            }
            
            return AddItem(Tools.StringToHash(definitionId), quantity);
        }

        /// <summary>
        /// Adds a new entry of the specified ItemDefinition by hash an Inventory Item to this collection. Returns the new (or existing) reference to the instance.
        /// Returns the new reference to the instance when the Item didn't already exist in the Collection and
        /// returns an existing reference when to the instance when the Item was already in the Collection.
        /// </summary>
        /// <param name="definitionHash">The hash of the ItemDefinition we are adding.</param>
        /// <param name="quantity">How many of this instance we are adding.</param>
        /// <returns>The reference to the instance that was added.</returns>
        public virtual T4 AddItem(int definitionHash, int quantity = 1)
        {
            T4 itemToReturn;
            
            if (ContainsItem(definitionHash))
            {
                m_ItemsInCollection[definitionHash].intValue += quantity;
                
                itemToReturn = m_ItemsInCollection[definitionHash];
            }
            else
            {
                BaseItemDefinition<T3, T4> itemDefinition = GetItemDefinition(definitionHash);
                if (itemDefinition == null)
                {
                    return null;
                }
                itemToReturn = itemDefinition.CreateItem();
                itemToReturn.intValue = quantity;
                m_ItemsInCollection.Add(definitionHash, itemToReturn);
            }

            onItemQuantityChanged.Invoke(itemToReturn);
            onItemAdded.Invoke(itemToReturn);
            // TODO: Check if intValue is overflowing and call OnItemQuantityOverflow Event, also contrain quantity
            
            return itemToReturn;
        }

        protected abstract BaseItemDefinition<T3, T4> GetItemDefinition(int definitionHash);

        /// <summary>
        /// Gets the instance of the requested Item by ItemDefinition id string if it is contained within, otherwise throws an exception.
        /// </summary>
        /// <param name="definitionId">The id of the ItemDefinition we are getting.</param>
        /// <returns>The reference to the Item instance.</returns>
        public T4 GetItemByDefinition(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
                return null;
            
            return GetItemByDefinition(Tools.StringToHash(definitionId));
        }

        /// <summary>
        /// Gets the instance of the requested Item by ItemDefinition id hash if it is contained within, otherwise throws an exception.
        /// </summary>
        /// <param name="definitionHash">The id hash of the ItemDefinition we are getting.</param>
        /// <returns>The reference to the Item instance.</returns>
        public T4 GetItemByDefinition(int definitionHash)
        {
            if (!m_ItemsInCollection.ContainsKey(definitionHash))
            {
                return null;
            }
            
            return m_ItemsInCollection[definitionHash];
        }

        /// <summary>
        /// This will look for an Item of the given ItemDefinition, and return the instance reference if it is found, otherwise throws an exception.
        /// </summary>
        /// <param name="definition">The ItemDefinition we are getting.</param>
        /// <returns>The reference to the Item instance.</returns>
        public T4 GetItemByDefinition(T3 definition)
        {
            if (definition == null)
            {
                return null;
            }
            
            return GetItemByDefinition(definition.hash);
        }

        /// <summary>
        /// Gets the instance of the requested Item by id if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="id">The id of the ItemDefinition we are getting.</param>
        /// <returns>The reference to the Item instance or null if not found.</returns>
        public T4 GetItem(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            
            return GetItem(Tools.StringToHash(id));
        }

        /// <summary>
        /// Gets the instance of the requested Item by ItemDefinition id hash if it is contained within, otherwise returns null.
        /// </summary>
        /// <param name="hash">The id hash of the ItemDefinition to return.</param>
        /// <returns>The reference to the Item instance or null if not found.</returns>
        public T4 GetItem(int hash)
        {
            foreach(var item in m_ItemsInCollection)
            {
                if (item.Value.hash == hash)
                {
                    return item.Value;
                }
            }
            
            return null;
        }

        /// <summary>
        /// This will return all Items that have the given Category (by CategoryDefinition id string) through an enumerator.
        /// </summary>
        /// <param name="categoryId">The id of the Category we are checking for.</param>
        /// <returns>An enumerator for the Items that have the given Category.</returns>
        public IEnumerable<T4> GetItemsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                return null;
            
            return GetItemsByCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// This will return all Items that have the given Category by CategoryDefinition id hash through an enumerator.
        /// </summary>
        /// <param name="categoryHash">The id hash of the Category we are checking for.</param>
        /// <returns>An enumerator for the Items that have the given Category.</returns>
        public IEnumerable<T4> GetItemsByCategory(int categoryHash)
        {
            foreach (var keyItemPair in m_ItemsInCollection)
            {
                var definition = GetItemDefinition(keyItemPair.Key);

                if (definition == null)
                    continue;

                foreach (var category in definition.categories)
                {
                    if (category.hash == categoryHash)
                    {
                        yield return keyItemPair.Value;
                    }
                }
            }
        }

        /// <summary>
        /// This will return all Items that have the given Category through an enumerator.
        /// </summary>
        /// <param name="category">The CategoryDefinition we are checking for.</param>
        /// <returns>An enumerator for the Items that have the given Category.</returns>
        public IEnumerable<T4> GetItemsByCategory(CategoryDefinition category)
        {
            if (category == null)
                return null;
            
            return GetItemsByCategory(category.hash);
        }

        /// <summary>
        /// Remove quantity amount of items from item with ItemDefinition definition.
        /// If the amount would leave the affected item with a non positive intValue it is removed from the collection
        /// </summary>
        /// <param name="definition">Item Definition reference of the item we are decrementing quantity or removing from the collection.</param>
        /// <param name="quantity">Proposed amount to decrement intValue</param>
        /// <returns>Whether the item has been removed from the collection</returns>
        public bool RemoveItem(T4 definition, int quantity = 1)
        {
            if (definition == null)
                return false;
            
            return RemoveItem(definition.hash, quantity);
        }

        /// <summary>
        /// Remove quantity amount of items from item with ItemDefinition id definitionId.
        /// If the amount would leave the affected item with a non positive intValue it is removed from the collection
        /// </summary>
        /// <param name="definitionId">Item Definition id of the item we are decrementing quantity or removing from the collection.</param>
        /// <param name="quantity">Proposed amount to decrement intValue</param>
        /// <returns>Whether the item has been removed from the collection</returns>
        public bool RemoveItem(string definitionId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(definitionId))
                return false;
            
            return RemoveItem(GetItem(definitionId), quantity);
        }

        /// <summary>
        /// Remove quantity amount of items from InventoryItem with ItemDefinition hash definitionHash.
        /// If the amount would leave the affected item with a non positive intValue it is removed from the collection
        /// </summary>
        /// <param name="definitionHash">Item Definition hash of the item we are decrementing quantity or removing from the collection.</param>
        /// <param name="quantity">Proposed amount to decrement intValue</param>
        /// <returns>Whether the item has been removed from the collection</returns>
        public bool RemoveItem(int definitionHash, int quantity = 1)
        {
            T4 item = GetItem(definitionHash);
            if (item != null)
            {
                if (item.intValue - quantity <= 0)
                {
                    return RemoveItem(definitionHash);
                }
                SetIntValue(definitionHash, item.intValue - quantity);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Removes an InventoryItem entry of the specified Item by ItemDefinition id.
        /// </summary>
        /// <param name="definitionId">The id of the ItemDefinition we are removing.</param>
        /// <returns>True if item was removed from the collection.</returns>
        public bool RemoveItem(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
                return false;
            
            return RemoveItem(Tools.StringToHash(definitionId));
        }
        
        /// <summary>
        /// Removes an InventoryItem entry of the specified ItemDefinition.
        /// </summary>
        /// <param name="definition">The ItemDefinition we are removing.</param>
        /// <returns>True if item was removed from the collection.</returns>
        public bool RemoveItem(T3 definition)
        {
            if (definition == null)
                return false;
            
            return RemoveItem(definition.hash);
        }

        /// <summary>
        /// Removes an InventoryItem entry of the specified ItemDefinition by id hash.
        /// </summary>
        /// <param name="definitionHash">The id hash of the ItemDefinition we are removing.</param>
        /// <returns>True if item was removed from the collection.</returns>
        public bool RemoveItem(int definitionHash)
        {
            T4 item = GetItem(definitionHash);
            if (item != null)
            {
                onItemWillRemove.Invoke(item);
                bool removed = m_ItemsInCollection.Remove(definitionHash);
                if (removed)
                { 
                    onItemRemoved.Invoke(item);
                }
                return removed;
            }
            return false;
        }

        /// <summary>
        /// This will remove all Items that have the given Category (by  CategoryDefinition id string).
        /// </summary>
        /// <param name="categoryId">The id of the CategoryDefinition to be removed.</param>
        /// <returns>The amount of items that were removed.</returns>
        public int RemoveItemsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                return 0;
            
            return RemoveItemsByCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// This will remove all Items that have the given Category (by  CategoryDefinition id hash).
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition to be removed.</param>
        /// <returns>The amount of items that were removed.</returns>
        public int RemoveItemsByCategory(int categoryHash)
        {
            List<int> toRemove = new List<int>();       //TODO: algorithm allocates list--refactor to avoid new list, if possible. UPDATE: After some research, I believe a list will be necessary for a reasonable solution.
            foreach (var keyItemPair in m_ItemsInCollection)
            {
                var definition = GetItemDefinition(keyItemPair.Key);

                if (definition == null)
                    continue;
                
                foreach (var category in definition.categories)
                {
                    if (category.hash == categoryHash)
                    {
                        toRemove.Add(definition.hash);
                    }
                }
            }

            foreach (var item in toRemove)
            {
                RemoveItem(item);
            }

            return toRemove.Count;
        }

        /// <summary>
        /// This will remove all Items that have the given Category by CategoryDefinition.
        /// </summary>
        /// <param name="category">The CategoryDefinition to be removed.</param>
        /// <returns>The amount of items that were removed.</returns>
        public int RemoveItemsByCategory(CategoryDefinition category)
        {
            if (category == null)
            {
                return 0;
            }
            
            return RemoveItemsByCategory(category.hash);
        }

        /// <summary>
        /// Removes all Items from this Collection.
        /// </summary>
        /// <returns>The amount of items that were removed.</returns>
        public int RemoveAll()
        {
            if (m_ItemsInCollection.Count == 0)
            {
                return 0;
            }

            var itemsToRemove = new T4[m_ItemsInCollection.Count];

            // save off all Items in Collection in case event causes dictionary to change
            // this is safer and allows firing did-remove events after dictionary has been cleared
            int itemOn = 0;
            foreach (var item in m_ItemsInCollection.Values)
            {
                itemsToRemove[itemOn++] = item;
            }

            // fire 'will remove' events for all Items Collection
            foreach (var item in itemsToRemove)
            {
                onItemWillRemove.Invoke(item);
            }

            // clear all Items
            m_ItemsInCollection.Clear();

            // fire 'removed' events for all Items just removed
            foreach (var item in itemsToRemove)
            {
                onItemRemoved.Invoke(item);
            }

            return itemsToRemove.Length;
        }

        /// <summary>
        /// Returns whether or not an instance of the given ItemDefinition (by id) exists within this Collection.
        /// </summary>
        /// <param name="definitionId">The id of the ItemDefinition we are checking for.</param>
        /// <returns>True/False whether or not the instance is within this Collection.</returns>
        public bool ContainsItem(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))	
                return false;
            
            return ContainsItem(Tools.StringToHash(definitionId));
        }

        /// <summary>
        /// Returns whether or not an instance of the given ItemDefinition id hash exists within this Collection.
        /// </summary>
        /// <param name="definitionHash">The id hash of the ItemDefinition we are checking for.</param>
        /// <returns>True/False whether or not the instance is within this Collection.</returns>
        public bool ContainsItem(int definitionHash)
        {
            return m_ItemsInCollection.ContainsKey(definitionHash);
        }

        /// <summary>
        /// Returns whether or not an instance of the given ItemDefinition exists within this Collection.
        /// </summary>
        /// <param name="definition">The ItemDefinition we are checking for.</param>
        /// <returns>True/False whether or not the instance is within this Collection.</returns>
        public bool ContainsItem(T3 definition)
        {
            if (definition == null)
            {
                return false;
            }
            
            return ContainsItem(definition.hash);
        }

        /// <summary>
        /// Sets the int value of the instance within this Collection of the specified ItemDefinition by id
        /// This method can be used to set the quantity of an Item to a negative number.
        /// </summary>
        /// <param name="definitionId">The id of the ItemDefinition we are checking for.</param>
        /// <param name="value">The new value we are setting. Can be negative or positive.</param>
        /// <exception cref="ArgumentException">If the given ItemDefinition string is null or empty.</exception>
        protected void SetIntValue(string definitionId, int value)
        {
            if (string.IsNullOrEmpty(definitionId))
            {
                throw new ArgumentException("The given definition id was null, cannot set int value.");
            }
            
            SetIntValue(Tools.StringToHash(definitionId), value);
        }

        /// <summary>
        /// Sets the int value of the instance within this Collection of the specified ItemDefinition by id hash.
        /// This method can be used to set the quantity of an Item to a negative number.
        /// </summary>
        /// <param name="definitionHash">The id hash of the ItemDefinition we are checking for.</param>
        /// <param name="value">The new value we are setting. Can be negative or positive.</param>
        /// <exception cref="ArgumentException">If the given id hash is not a valid entry.</exception>
        protected void SetIntValue(int definitionHash, int value)
        {
            if (!ContainsItem(definitionHash))
            {
                throw new ArgumentException("The given definition hash was not found, cannot set int value.");
            }

            var item = GetItemByDefinition(definitionHash);
            item.intValue = value;
            onItemQuantityChanged.Invoke(item);
            // TODO: Check if intValue is overflowing and call OnItemQuantityOverflow Event
        }

        /// <summary>
        /// Sets the int value of the instance within this Collection of the specified ItemDefinition
        /// This method can be used to set the quantity of an Item to a negative number.
        /// </summary>
        /// <param name="definition">The ItemDefinition we are checking for.</param>
        /// <param name="value">The new value we are setting. Can be negative or positive.</param>
        /// <exception cref="ArgumentException">If the given ItemDefinition is null.</exception>
        protected void SetIntValue(T3 definition, int value)
        {
            if (definition == null)
            {
                throw new ArgumentException("The given definition was null, cannot set int value.");
            }

            SetIntValue(definition.hash, value);
        }

        /// <summary>
        /// Resets the contents of this Collection based on the CollectionDefinition.
        /// </summary>
        public void Reset()
        {
            // remove all existing Items from the Collection
            RemoveAll();

            // fire reset event
            onCollectionReset.Invoke((T2)this);

            // iterate all default Items in the CollectionDefinition (if there are any) and add them to the Collection
            AddAllDefaultItems();
        }

        // iterate all default Items in the CollectionDefinition (if there are any) and add them to the Collection
        protected void AddAllDefaultItems()
        {
            if (m_Definition != null && m_Definition.defaultItems != null)
            { 
                foreach (var defaultItem in m_Definition.defaultItems)
                {
                    var definition = GetItemDefinition(defaultItem.definitionHash);
                    if (definition == null)
                        continue;

                    var newItem = definition.CreateItem();
                    newItem.intValue = defaultItem.quantity;
                    m_ItemsInCollection.Add(definition.hash, newItem);
                }
            }
        }
    }
}
