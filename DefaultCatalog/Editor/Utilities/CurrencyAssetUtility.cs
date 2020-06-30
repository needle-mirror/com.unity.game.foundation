namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class CurrencyAssetUtility
    {
        /// <inheritdoc cref="CurrencyAsset.Editor_SetInitialBalance(long)"/>
        public static void SetInitialBalance(this CurrencyAsset @this, long balance)
            => @this.Editor_SetInitialBalance(balance);

        /// <inheritdoc cref="CurrencyAsset.Editor_SetMaximumBalance(long)"/>
        public static void SetMaximumBalance(this CurrencyAsset @this, long balance)
            => @this.Editor_SetMaximumBalance(balance);

        /// <inheritdoc cref="CurrencyAsset.Editor_SetType(CurrencyType)"/>
        public static void SetType(this CurrencyAsset @this, CurrencyType type)
            => @this.Editor_SetType(type);
    }
}
