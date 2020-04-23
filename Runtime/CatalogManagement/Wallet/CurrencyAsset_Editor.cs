#if UNITY_EDITOR

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class CurrencyAsset
    {
        /// <inheritdoc/>
        internal override string Editor_AssetPrefix => "Currency";

        /// <summary>
        /// Removes the deleted currency from the catalog.
        /// </summary>
        protected override void OnItemDestroy()
        {
            if (catalog is null) return;

            //Debug.Log($"Removing {displayName} ({GetType().Name}) from {catalog}");
            (catalog as CurrencyCatalogAsset).Editor_RemoveItem(this);
        }
    }
}

#endif
