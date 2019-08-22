using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes preset values and rules for a Collection by using a CollectionDefinition.
    /// During runtime, it may be useful to refer back to the CollectionDefinition for the presets and rules, 
    /// but the values cannot be changed at runtime (your system may, for example, bypass the presets, 
    /// or calculate new values on the fly with modifiers).
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T4">The type of Items this CollectionDefinition uses.</typeparam>
    public abstract class BaseCollectionDefinition<T1, T2, T3, T4> : GameItemDefinition
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T3, T4>
        where T4 : BaseItem<T3, T4>
    {
        protected BaseCollectionDefinition()
        {
        }

        [SerializeField]
        protected List<DefaultItem> m_DefaultItems = new List<DefaultItem>();

        /// <summary>
        /// Iterator for iterating through the DefaultItems.
        /// </summary>
        /// <returns>An iterator for iterating through the DefaultItems.</returns>
        public IEnumerable<DefaultItem> defaultItems
        {
            get { return m_DefaultItems; }
        }

        /// <summary>
        /// Adds the given DefaultItem to this CollectionDefinition.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem to add.</param>
        /// <param name="quantity">Quantity of Items to add (defaults to 0 which creates the Item with zero quantity).</param>
        /// <returns>Whether or not the adding was successful.</returns>
        public virtual bool AddDefaultItem(T3 defaultItem, int quantity = 0)
        {
            if (defaultItem == null)
                return false;
            
            return AddDefaultItem(new DefaultItem(defaultItem.hash, quantity));
        }

        /// <summary>
        /// Adds the given DefaultItem to this CollectionDefinition.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        public virtual bool AddDefaultItem(DefaultItem defaultItem)
        {
            Tools.ThrowIfPlayMode("Cannot add a defaultItem to a CollectionDefinition while in play mode.");
            
            if (m_DefaultItems.Contains(defaultItem))
            {
                return false;
            }

            m_DefaultItems.Add(defaultItem);

            return true;
        }

        /// <summary>
        /// Returns the DefaultItem at the given index.
        /// </summary>
        /// <param name="index">The index we are checking for.</param>
        /// <returns>The DefaultItem at the requested index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of range.</exception>
        public DefaultItem GetDefaultItem(int index)
        {
            if (index < 0 || index >= m_DefaultItems.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return m_DefaultItems[index];
        }

        /// <summary>
        /// Returns the index of the requested DefaultItem, or -1 if it's not within this CollectionDefinition.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem we are checking.</param>
        /// <returns>The index of the requested DefaultItem.</returns>
        public int GetIndexOfDefaultItem(DefaultItem defaultItem)
        {
            return m_DefaultItems.IndexOf(defaultItem);
        }

        /// <summary>
        /// Sets the default quantity of the Item specified.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem we are changing quantity of.</param>
        /// <param name="quantity">The quantity to change to.</param>
        /// <returns>Bool of whether changing quantity was successful.</returns>
        public bool SetDefaultItemQuantity(DefaultItem defaultItem, int quantity)
        {
            Tools.ThrowIfPlayMode("Cannot set DefaultItem quantity while in play mode.");

            int index = GetIndexOfDefaultItem(defaultItem);
            if (index < 0 || index >= m_DefaultItems.Count)
            {
                return false;
            }

            defaultItem.quantity = quantity;
            m_DefaultItems[index] = defaultItem;

            return true;
        }

        /// <summary>
        /// Removes the specified DefaultItem from this CollectionDefinition's list of DefaultItems.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveDefaultItem(DefaultItem defaultItem)
        {
            Tools.ThrowIfPlayMode("Cannot remove a DefaultItem from a CollectionDefinition while in play mode.");

            return m_DefaultItems.Remove(defaultItem);
        }

        /// <summary>
        /// Swaps the locations of the DefaultItems in the DefaultItems list.
        /// </summary>
        /// <param name="defaultItem1">The first DefaultItem to swap.</param>
        /// <param name="defaultItem2">The second DefaultItem to swap.</param>
        public void SwapDefaultItemsListOrder(DefaultItem defaultItem1, DefaultItem defaultItem2)
        {
            Tools.ThrowIfPlayMode("Cannot swap DefaultItems order while in play mode.");

            int index1 = GetIndexOfDefaultItem(defaultItem1);
            int index2 = GetIndexOfDefaultItem(defaultItem2);

            m_DefaultItems[index1] = defaultItem2;
            m_DefaultItems[index2] = defaultItem1;
        }

        /// <summary>
        /// Checks whether a given ItemDefinition is present in the list of DefaultItems.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition that is being checked for presence in DefaultItems list.</param>
        /// <returns>Whether or not the ItemDefinition exists in the list.</returns>
        public bool ContainsDefaultItem(InventoryItemDefinition itemDefinition)
        {
            return ContainsDefaultItem(itemDefinition.hash);
        }

        /// <summary>
        /// Checks whether a given ItemDefinition id is present in the list of DefaultItems.
        /// </summary>
        /// <param name="itemDefinitionId">The ItemDefinition's id hash that is being checked for presence in DefaultItems list.</param>
        /// <returns>Whether or not the Item exists in the list.</returns>
        public bool ContainsDefaultItem(string itemDefinitionId)
        {
            return ContainsDefaultItem(Tools.StringToHash(itemDefinitionId));
        }

        /// <summary>
        /// Checks whether a given ItemDefinition's id hash is present in the list of DefaultItems.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition hash that is being checked for presence in DefaultItems list.</param>
        /// <returns>Whether or not the Item exists in the DefaultItems list.</returns>
        public bool ContainsDefaultItem(int itemDefinitionHash)
        {
            return m_DefaultItems.FindIndex(item => item.definitionHash == itemDefinitionHash) >= 0;
        }

        /// <summary>
        /// Returns the number of DefaultItems within this CollectionDefinition.
        /// </summary>
        /// <returns>The number of DefaultItems within this CollectionDefinition.</returns>
        public int defaultItemCount
        {
            get { return m_DefaultItems.Count; }
        }

        /// <summary>
        /// Spawns an instance of a Collection that is based off of this CollectionDefinition.
        /// </summary>
        /// <returns>The reference to the newly created Collection.</returns>
        internal abstract T2 CreateCollection(string collectionId, string displayName);
    }
}
