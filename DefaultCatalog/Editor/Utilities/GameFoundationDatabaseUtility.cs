namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class GameFoundationDatabaseUtility
    {
        /// <inheritdoc cref="GameFoundationDatabase.Editor_CreateInventoryItem(string)"/>
        public static InventoryItemDefinitionAsset CreateInventoryItem(this GameFoundationDatabase @this, string key)
            => @this.Editor_CreateInventoryItem(key);

        /// <inheritdoc cref="GameFoundationDatabase.Editor_CreateTag(string)"/>
        public static TagAsset CreateTag(this GameFoundationDatabase @this, string id)
            => @this.Editor_CreateTag(id);

        /// <inheritdoc cref="GameFoundationDatabase.Editor_CreateCurrency(string)"/>
        public static CurrencyAsset CreateCurrency(this GameFoundationDatabase @this, string key)
            => @this.Editor_CreateCurrency(key);

        /// <inheritdoc cref="GameFoundationDatabase.Editor_CreateStore(string)"/>
        public static StoreAsset CreateStore(this GameFoundationDatabase @this, string key)
            => @this.Editor_CreateStore(key);

        /// <inheritdoc cref="GameFoundationDatabase.Editor_CreateVirtualTransaction(string)"/>
        public static VirtualTransactionAsset CreateVirtualTransaction(this GameFoundationDatabase @this, string key)
            => @this.Editor_CreateVirtualTransaction(key);

        /// <inheritdoc cref="GameFoundationDatabase.Editor_CreateIapTransaction(string)"/>
        public static IAPTransactionAsset CreateIapTransaction(this GameFoundationDatabase @this, string key)
            => @this.Editor_CreateIapTransaction(key);

        /// <inheritdoc cref="GameFoundationDatabase.Editor_Save(string)"/>
        public static void Save(this GameFoundationDatabase @this, string path)
            => @this.Editor_Save(path);
    }
}
