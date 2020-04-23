#if UNITY_EDITOR

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class InventoryItemDefinitionAsset
    {
        /// <inheritdoc/>
        internal override string Editor_AssetPrefix => "Item";

        protected override void OnItemDestroy()
        {
            if (catalog is null) return;

            //Debug.Log($"Removing {displayName} ({GetType().Name}) from {catalog}");
            (catalog as InventoryCatalogAsset).Editor_RemoveItem(this);
        }
    }
}

#endif
