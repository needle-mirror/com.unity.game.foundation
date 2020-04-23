using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Straightforward implementation of <see cref="IInventoryDataLayer"/>.
    /// </summary>
    class InventoryDataLayer : IInventoryDataLayer
    {
        /// <summary>
        /// Owner this <see cref="InventoryDataLayer"/> instance.
        /// </summary>
        BaseMemoryDataLayer m_Owner;
        /// <summary>
        /// Stores the data of all the item instances.
        /// </summary>
        internal Dictionary<string, InventoryItemSerializableData> m_Items;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryDataLayer"/>
        /// class with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="data">InventoryManager's serializable data.</param>
        public InventoryDataLayer(BaseMemoryDataLayer owner, InventoryManagerSerializableData data)
        {
            m_Owner = owner;
            m_Items = new Dictionary<string, InventoryItemSerializableData>();

            foreach (var item in data.items)
            {
                m_Items.Add(item.id, item);
            }
        }

        /// <summary>
        /// Tells whether or not an item exists with the specified
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <returns>True if an item exists with the specified
        /// <paramref name="id"/>, false otherwise.</returns>
        internal bool Contains(string id) => m_Items.ContainsKey(id);

        /// <summary>
        /// Gets the item of a given type.
        /// </summary>
        /// <param name="id">The identifier of the
        /// <see cref="InventoryItemDefinition"/>.</param>
        /// <param name="target">The target collection the items are added
        /// to.</param>
        /// <returns>The numnber of items added.</returns>
        internal int GetItemsByDefinition
            (string id, ICollection<string> target = null)
        {
            Tools.ThrowIfArgNullOrEmpty(id, nameof(id));

            var count = 0;
            target?.Clear();

            foreach (var item in m_Items.Values)
            {
                if (item.definitionId == id)
                {
                    count++;
                    target?.Add(item.id);
                }
            }

            return count;
        }

        /// <summary>
        /// Creates a new item.
        /// </summary>
        /// <param name="definitionId">The definition of the item to create</param>
        /// <returns>The id of the newly created item</returns>
        internal string CreateItem(string definitionId)
        {
            var itemId = Guid.NewGuid().ToString();
            var data = new InventoryItemSerializableData
            {
                id = itemId,
                definitionId = definitionId
            };
            
            m_Items.Add(itemId, data);
            m_Owner.m_StatDataLayer.InitStats(itemId);

            return itemId;
        }

        /// <summary>
        /// Deletes an item.
        /// </summary>
        /// <param name="id">Identifier of the item to delete.</param>
        /// <returns><c>true</c> if deleted, <c>false</c> otherwise.</returns>
        internal bool DeleteItem(string id) => m_Items.Remove(id);

        /// <inheritdoc />
        InventoryManagerSerializableData IInventoryDataLayer.GetData()
        {
            var items = new InventoryItemSerializableData[m_Items.Count];
            m_Items.Values.CopyTo(items, 0);
            
            var data = new InventoryManagerSerializableData
            {
                items = items
            };

            return data;
        }

        /// <inheritdoc/>
        void IInventoryDataLayer.CreateItem(string definitionId, string itemId, Completer completer)
        {
            if (m_Items.ContainsKey(itemId))
            {
                var error = new ArgumentException
                    ($"An Item with the id \"{itemId}\" already exists.");

                completer.Reject(error);
                return;
            }

            var item = new InventoryItemSerializableData
            {
                definitionId = definitionId,
                id = itemId
            };
            m_Items.Add(item.id, item);

            completer.Resolve();
        }

        /// <inheritdoc />
        void IInventoryDataLayer.DeleteItem(string itemId, Completer completer)
        {
            var found = m_Items.ContainsKey(itemId);
            
            if (!found)
            {
                //Requesting deletion of an item in a non existing inventory is a silent error.
                completer.Resolve();
                return;
            }

            m_Items.Remove(itemId);
            completer.Resolve();
        }
    }
}
