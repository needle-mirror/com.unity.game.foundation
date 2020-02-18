using System;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    public abstract partial class BaseMemoryDataLayer
    {
        IInventoryDataLayer m_InventoryDataLayer;

        /// <summary>
        /// Initializes the data layer for <see cref="InventoryManager"/>.
        /// </summary>
        /// <param name="data">InventoryManager's serializable data.</param>
        protected void InitializeInventoryDataLayer(InventoryManagerSerializableData data)
        {
            m_InventoryDataLayer = new InventoryDataLayer(data);
        }

        /// <inheritdoc />
        InventoryManagerSerializableData IInventoryDataLayer.GetData()
        {
            return m_InventoryDataLayer.GetData();
        }

        /// <inheritdoc />
        void IInventoryDataLayer.CreateInventory(
            string definitionId,
            string inventoryId,
            string displayName,
            int gameItemId,
            Completer completer)
        {
            m_InventoryDataLayer.CreateInventory(definitionId, inventoryId, displayName, gameItemId, completer);
        }

        /// <inheritdoc />
        void IInventoryDataLayer.DeleteInventory(string inventoryId, Completer completer)
        {
            m_InventoryDataLayer.DeleteInventory(inventoryId, completer);
        }

        /// <inheritdoc />
        void IInventoryDataLayer.DeleteItem(string inventoryId, string itemDefinitionId, Completer completer)
        {
            m_InventoryDataLayer.DeleteItem(inventoryId, itemDefinitionId, completer);
        }

        /// <inheritdoc />
        void IInventoryDataLayer.SetItemQuantity(
            string inventoryId,
            string itemDefinitionId,
            int quantity,
            int gameItemId,
            Completer completer)
        {
            m_InventoryDataLayer.SetItemQuantity(inventoryId, itemDefinitionId, quantity, gameItemId, completer);
        }
    }
}
