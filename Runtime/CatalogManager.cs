namespace UnityEngine.GameFoundation
{
    public class CatalogManager
    {
        /// <summary>
        /// A reference to an inventory catalog
        /// </summary>
        public InventoryCatalog inventoryCatalog { get; internal set; }

        /// <summary>
        /// A reference to a store catalog
        /// </summary>
        public StoreCatalog storeCatalog { get; internal set; }

        /// <summary>
        /// A reference to a stat catalog
        /// </summary>
        public StatCatalog statCatalog { get; internal set; }

        /// <summary>
        /// A reference to a currency catalog
        /// </summary>
        public CurrencyCatalog currencyCatalog { get; internal set; }

        /// <summary>
        /// A reference to a transaction catalog
        /// </summary>
        public TransactionCatalog transactionCatalog { get; internal set; }
    }
}
