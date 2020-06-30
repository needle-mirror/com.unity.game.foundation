namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class StoreAssetUtility
    {
        /// <inheritdoc cref="StoreAsset.Editor_AddItem(BaseTransactionAsset, bool)"/>
        public static void AddTransaction(this StoreAsset @this, BaseTransactionAsset transaction, bool enabled = true)
            => @this.Editor_AddItem(transaction, enabled);

        /// <inheritdoc cref="StoreAsset.Editor_SetEnable(BaseTransactionAsset, bool)"/>
        public static void SetEnable(this StoreAsset @this, BaseTransactionAsset transaction, bool enabled = true)
            => @this.SetEnable(transaction, enabled);
    }
}
