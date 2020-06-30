namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Catalog for <see cref="BaseTransactionAsset"/>.
    /// </summary>
    public sealed partial class TransactionCatalogAsset
        : SingleCollectionCatalogAsset<BaseTransactionAsset>
    {
        /// <inheritdoc/>
        protected override void InitializeSingleCollectionCatalog()
        {
#if UNITY_EDITOR
            Editor_InitializeCatalog();
#endif
        }
    }
}
