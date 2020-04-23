using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Definition for the inventory items
    /// </summary>
    public sealed partial class InventoryItemDefinitionAsset : CatalogItemAsset
    {
        /// <inheritdoc />
        protected sealed override
            CatalogItemConfig ConfigureItem(CatalogBuilder builder)
        {
            var item = builder.Create<InventoryItemDefinitionConfig>(id);
            return item;
        }
    }
}
