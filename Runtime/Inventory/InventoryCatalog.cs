using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Definitions for InventoryItemDefinitions and InventoryDefinitions.
    /// The Catalog serves as a way to find references to Definitions, as needed.
    /// </summary>
    /// <inheritdoc/>
    public class InventoryCatalog : BaseCatalog<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        internal static readonly string k_MainInventoryDefinitionId = "main";
        internal static readonly string k_MainInventoryDefinitionName = "Main";
        internal static readonly string k_WalletInventoryDefinitionId = "wallet";
        internal static readonly string k_WalletInventoryDefinitionName = "Wallet";

        /// <summary>
        /// Constructor to build an InventoryCatalog object.
        /// </summary>
        /// <param name="itemDefinitions">The list of InventoryItemDefinitions that will be available in this catalog for runtime instance instantiation. If null value is passed in an empty list will be created.</param>
        /// <param name="collectionDefinitions">The list of InventoryDefinitions that will be available in this catalog for runtime instance instantiation. If null value is passed in an empty list will be created.</param>
        /// <param name="defaultCollectionDefinitions">The list of DefaultCollectionDefinitions that are the collections in this catalog that get automatically instantiated. If null value is passed in an empty list will be created.</param>
        /// <param name="categories">The list of CategoryDefinitions that are the possible categories which could be applied to items in this catalog. If null value is passed in an empty list will be created.</param>
        internal InventoryCatalog(List<InventoryItemDefinition> itemDefinitions,  List<InventoryDefinition> collectionDefinitions, List<DefaultCollectionDefinition> defaultCollectionDefinitions, List<CategoryDefinition> categories)
            : base(itemDefinitions,  collectionDefinitions, defaultCollectionDefinitions, categories)
        {
        }
    }
}
