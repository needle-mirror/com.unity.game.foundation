#if UNITY_EDITOR

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class TransactionCatalogAsset
    {
        /// <inheritdoc/>
        void Editor_InitializeCatalog()
        {
            var itemCatalog = database.inventoryCatalog;
            itemCatalog.editor_ItemRemoved += Editor_OnInventoryItemRemoved;

            var currencyCatalog = database.currencyCatalog;
            currencyCatalog.editor_ItemRemoved += Editor_OnCurrencyRemoved;
        }

        /// <summary>
        /// Called when a currency is removed from the
        /// <see cref="GameFoundationDatabase"/>.
        /// </summary>
        /// <param name="catalog">The currency catalog</param>
        /// <param name="currency">The removed currency</param>
        void Editor_OnCurrencyRemoved(
            SingleCollectionCatalogAsset<CurrencyAsset> catalog,
            CurrencyAsset currency)
        {
            foreach (var transaction in m_Items)
            {
                transaction.Editor_OnCurrencyRemoved(currency);
            }
        }

        /// <summary>
        /// Called when an item is removed from the
        /// <see cref="GameFoundationDatabase"/>.
        /// </summary>
        /// <param name="catalog">The item catalog</param>
        /// <param name="item">The removed item</param>
        void Editor_OnInventoryItemRemoved(
            SingleCollectionCatalogAsset<InventoryItemDefinitionAsset> catalog,
            InventoryItemDefinitionAsset item)
        {
            foreach (var transaction in m_Items)
            {
                transaction.Editor_OnItemRemoved(item);
            }
        }
    }
}

#endif
