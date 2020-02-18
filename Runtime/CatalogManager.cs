namespace UnityEngine.GameFoundation
{
    public class CatalogManager
    {
        /// <summary>
        /// A reference to a game item catalog
        /// </summary>
        public static GameItemCatalog gameItemCatalog { get; internal set; }

        /// <summary>
        /// A reference to an inventory catalog
        /// </summary>
        public static InventoryCatalog inventoryCatalog { get; internal set; }

        /// <summary>
        /// A reference to a stat catalog
        /// </summary>
        public static StatCatalog statCatalog { get; internal set; }
    }
}
