#if UNITY_EDITOR

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class StoreCatalogAsset
    {
        /// <summary>
        /// Makes this catalog listen the transaction catalog in order to clean
        /// the store in case a transaction is removed.
        /// </summary>
        void Editor_InitializeCatalog()
        {
            var transactionCatalog = database.transactionCatalog;
            transactionCatalog.editor_ItemRemoved += Editor_OnTransactionRemoved;
        }

        /// <summary>
        /// Cleans the <see cref="StoreAsset"/> instances referencing the
        /// <paramref name="item"/> instance.
        /// </summary>
        /// <param name="catalog">The catalog from which the
        /// <paramref name="item"/> has be removed.</param>
        /// <param name="item">The removed item.</param>
        void Editor_OnTransactionRemoved(
            SingleCollectionCatalogAsset<BaseTransactionAsset> catalog,
            BaseTransactionAsset item)
        {
            foreach (var store in m_Items)
            {
                store.Editor_HandleTransactionRemoved(item);
            }
        }
    }
}

#endif
