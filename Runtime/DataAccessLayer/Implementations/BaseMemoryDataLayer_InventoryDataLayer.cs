using System;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    public abstract partial class BaseMemoryDataLayer
    {
        /// <summary>
        /// The part of the data layer dedicated to the inventory management.
        /// </summary>
        internal InventoryDataLayer m_InventoryDataLayer;

        /// <summary>
        /// Initializes the data layer for <see cref="InventoryManager"/>.
        /// </summary>
        /// <param name="data">InventoryManager's serializable data.</param>
        protected void InitializeInventoryDataLayer(InventoryManagerSerializableData data)
        {
            m_InventoryDataLayer = new InventoryDataLayer(this, data);
        }

        /// <inheritdoc />
        InventoryManagerSerializableData IInventoryDataLayer.GetData()
        {
            return (m_InventoryDataLayer as IInventoryDataLayer).GetData();
        }

        void IInventoryDataLayer.CreateItem(string definitionId, string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).CreateItem(definitionId, itemId, completer);
        }

        /// <inheritdoc />
        void IInventoryDataLayer.DeleteItem(string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).DeleteItem(itemId, completer);
        }
    }
}
