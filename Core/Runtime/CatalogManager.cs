namespace UnityEngine.GameFoundation
{
    public class CatalogManager
    {
        /// <summary>
        /// A reference to a tag catalog
        /// </summary>
        public TagCatalog tagCatalog { get; internal set; }

        /// <summary>
        /// A reference to an inventory catalog
        /// </summary>
        public InventoryCatalog inventoryCatalog { get; internal set; }

        /// <summary>
        /// A reference to a store catalog
        /// </summary>
        public StoreCatalog storeCatalog { get; internal set; }

        /// <summary>
        /// A reference to a currency catalog
        /// </summary>
        public CurrencyCatalog currencyCatalog { get; internal set; }

        /// <summary>
        /// A reference to a transaction catalog
        /// </summary>
        public TransactionCatalog transactionCatalog { get; internal set; }

        /// <summary>
        /// A reference to a game parameters catalog
        /// </summary>
        public GameParameterCatalog gameParameterCatalog { get; internal set; }

        /// <summary>
        /// Removes tag from all catalogs.
        /// </summary>
        /// <param name="tagKey">string key to tag to remove from all catalogs.</param>
        internal virtual void OnRemoveTag(string tagKey)
        {
            inventoryCatalog.OnRemoveTag(tagKey);
            storeCatalog.OnRemoveTag(tagKey);
            currencyCatalog.OnRemoveTag(tagKey);
            transactionCatalog.OnRemoveTag(tagKey);
        }
    }
}
