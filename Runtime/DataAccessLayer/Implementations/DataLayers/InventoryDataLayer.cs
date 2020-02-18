using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Straightforward implementation of <see cref="IInventoryDataLayer"/>.
    /// </summary>
    class InventoryDataLayer : IInventoryDataLayer
    {
        public class Inventory
        {
            public InventorySerializableData data;

            public Dictionary<string, InventoryItemSerializableData> items =
                new Dictionary<string, InventoryItemSerializableData>();
        }

        Dictionary<string, Inventory> m_Inventories;

        /// <summary>
        /// Create a new <see cref="InventoryDataLayer"/> with the given data.
        /// </summary>
        /// <param name="data">InventoryManager's serializable data.</param>
        public InventoryDataLayer(InventoryManagerSerializableData data)
        {
            m_Inventories = new Dictionary<string, Inventory>();

            var inventories = data.inventories;
            if (inventories == null) return;

            var items = data.items;

            foreach (var inventoryData in inventories)
            {
                var inventory = new Inventory
                {
                    data = inventoryData
                };

                m_Inventories.Add(inventory.data.Id, inventory);

                if (items == null) continue;

                foreach (var itemData in items)
                {
                    if (itemData.inventoryId == inventoryData.Id)
                        inventory.items.Add(itemData.definitionId, itemData);
                }
            }
        }

        /// <inheritdoc />
        InventoryManagerSerializableData IInventoryDataLayer.GetData()
        {
            var inventories = new List<InventorySerializableData>();
            var items = new List<InventoryItemSerializableData>();

            foreach (var inventory in m_Inventories.Values)
            {
                inventories.Add(inventory.data);
                items.AddRange(inventory.items.Values);
            }

            var data = new InventoryManagerSerializableData
            {
                inventories = inventories.ToArray(),
                items = items.ToArray()
            };

            return data;
        }

        /// <inheritdoc />
        void IInventoryDataLayer.CreateInventory(
            string definitionId,
            string inventoryId,
            string displayName,
            int gameItemId,
            Completer completer)
        {
            if (m_Inventories.ContainsKey(inventoryId))
            {
                var error = new ArgumentException(
                    $"An inventory with the id \"{inventoryId}\" already exists.");

                completer.Reject(error);
                return;
            }

            var inventoryData = new InventorySerializableData
            {
                Id = inventoryId,
                definitionId = definitionId,
                displayName = displayName,
                gameItemId = gameItemId
            };

            var inventory = new Inventory
            {
                data = inventoryData,
                items = new Dictionary<string, InventoryItemSerializableData>()
            };

            m_Inventories.Add(inventoryId, inventory);

            completer.Resolve();
        }

        /// <inheritdoc />
        void IInventoryDataLayer.DeleteInventory(string inventoryId, Completer completer)
        {
            m_Inventories.Remove(inventoryId);

            // The deletion of a none existing inventory is considered a valid
            // operation so we resolve instead of rejecting the response.
            completer.Resolve();
        }

        /// <inheritdoc />
        void IInventoryDataLayer.SetItemQuantity(
            string inventoryId,
            string itemDefinitionId,
            int quantity,
            int gameItemId,
            Completer completer)
        {
            var found = m_Inventories.TryGetValue(inventoryId, out Inventory inventory);

            if (!found)
            {
                var error = new KeyNotFoundException($"There is no inventory with the id \"{inventoryId}\".");

                completer.Reject(error);
                return;
            }

            found = inventory.items.TryGetValue(itemDefinitionId, out InventoryItemSerializableData itemData);

            if (found)
            {
                itemData.quantity = quantity;
                inventory.items[itemDefinitionId] = itemData;
            }
            else
            {
                itemData = new InventoryItemSerializableData(inventoryId, itemDefinitionId, quantity, gameItemId);
                inventory.items.Add(itemDefinitionId, itemData);
            }

            completer.Resolve();
        }

        /// <inheritdoc />
        void IInventoryDataLayer.DeleteItem(string inventoryId, string itemDefinitionId, Completer completer)
        {
            var found = m_Inventories.TryGetValue(inventoryId, out Inventory inventory);

            if (!found)
            {
                //Requesting deletion of an item in a non existing inventory is a silent error.
                completer.Resolve();

                return;
            }

            inventory.items.Remove(itemDefinitionId);

            //Deleting an Item that isn't in the inventory is considered valid 
            completer.Resolve();
        }
    }
}
