namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class InventoryItemDefinitionAssetUtility
    {
        /// <inheritdoc cref="InventoryItemDefinitionAsset.Editor_AddProperty(string, Property)"/>
        public static bool AddProperty(this InventoryItemDefinitionAsset @this, string name, Property defaultValue)
            => @this.Editor_AddProperty(name, defaultValue);
    }
}
