namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class VirtualTransactionAssetUtility
    {
        /// <inheritdoc cref="VirtualTransactionAsset.Editor_AddCost(CurrencyAsset, long)"/>
        public static void AddCost(this VirtualTransactionAsset @this, CurrencyAsset currency, long amount)
            => @this.Editor_AddCost(currency, amount);

        /// <inheritdoc cref="VirtualTransactionAsset.Editor_AddCost(InventoryItemDefinitionAsset, long)"/>
        public static void AddCost(this VirtualTransactionAsset @this, InventoryItemDefinitionAsset item, long amount)
            => @this.Editor_AddCost(item, amount);
    }
}
