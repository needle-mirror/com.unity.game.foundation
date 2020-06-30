using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public abstract partial class BaseMemoryDataLayer
    {
        /// <summary>
        /// The part of the data layer dedicated to the inventory management.
        /// </summary>
        InventoryDataLayer m_InventoryDataLayer;

        /// <summary>
        /// Initializes the data layer for <see cref="InventoryManager"/>.
        /// </summary>
        /// <param name="data">
        /// InventoryManager's serializable data.
        /// </param>
        /// <param name="catalog">
        /// The catalog used as source of truth.
        /// </param>
        protected void InitializeInventoryDataLayer(InventoryManagerData data, InventoryCatalogAsset catalog)
        {
            m_InventoryDataLayer = new InventoryDataLayer(data, catalog);
        }

        /// <inheritdoc />
        InventoryManagerData IInventoryDataLayer.GetData()
        {
            return (m_InventoryDataLayer as IInventoryDataLayer).GetData();
        }

        /// <inheritdoc />
        void IInventoryDataLayer.CreateItem(string definitionKey, string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).CreateItem(definitionKey, itemId, completer);
        }

        /// <inheritdoc />
        void IInventoryDataLayer.DeleteItem(string itemId, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).DeleteItem(itemId, completer);
        }

        /// <inheritdoc />
        Property IInventoryDataLayer.GetPropertyValue(string itemId, string propertyKey)
        {
            return (m_InventoryDataLayer as IInventoryDataLayer).GetPropertyValue(itemId, propertyKey);
        }

        /// <inheritdoc />
        bool IInventoryDataLayer.TryGetPropertyValue(string itemId, string propertyKey, out Property propertyValue)
        {
            return (m_InventoryDataLayer as IInventoryDataLayer).TryGetPropertyValue(itemId, propertyKey, out propertyValue);
        }

        /// <inheritdoc />
        void IInventoryDataLayer.SetPropertyValue(string itemId, string propertyKey, Property value, Completer completer)
        {
            (m_InventoryDataLayer as IInventoryDataLayer).SetPropertyValue(itemId, propertyKey, value, completer);
        }
    }
}
