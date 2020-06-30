namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class BaseTransactionAssetUtility
    {
        /// <inheritdoc cref="BaseTransactionAsset.Editor_AddReward(CurrencyAsset, long)"/>
        public static void AddReward(this BaseTransactionAsset @this, CurrencyAsset currency, long amount)
            => @this.Editor_AddReward(currency, amount);

        /// <inheritdoc cref="BaseTransactionAsset.Editor_AddReward(InventoryItemDefinitionAsset, long)"/>
        public static void AddReward(this BaseTransactionAsset @this, InventoryItemDefinitionAsset item, long amount)
            => @this.Editor_AddReward(item, amount);
    }
}
