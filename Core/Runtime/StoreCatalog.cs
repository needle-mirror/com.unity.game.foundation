namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Definitions for StoreDefinitions.
    /// The Catalog serves as a way to find references to Definitions, as needed.
    /// </summary>
    /// <inheritdoc/>
    public class StoreCatalog : SingleCollectionCatalog<Store>
    {
        internal static readonly string k_MainStoreDefinitionId = "main";
        internal static readonly string k_MainStoreDefinitionName = "Main";
    }
}
