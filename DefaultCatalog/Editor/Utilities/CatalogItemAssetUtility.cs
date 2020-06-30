namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class CatalogItemAssetUtility
    {
        /// <inheritdoc cref="CatalogItemAsset.Editor_SetDisplayName(string)"/>
        public static void SetDisplayName(this CatalogItemAsset @this, string displayName)
            => @this.Editor_SetDisplayName(displayName);

        /// <inheritdoc cref="CatalogItemAsset.Editor_AddStaticProperty(string, Property)"/>
        public static bool AddStaticProperty(this CatalogItemAsset @this, string name, Property value)
            => @this.Editor_AddStaticProperty(name, value);

        /// <inheritdoc cref="CatalogItemAsset.Editor_AddTag(TagAsset)"/>
        public static void AddTag(this CatalogItemAsset @this, TagAsset tag) => @this.Editor_AddTag(tag);

        /// <inheritdoc cref="CatalogItemAsset.Editor_RemoveTag(TagAsset)"/>
        public static void RemoveTag(this CatalogItemAsset @this, TagAsset tag) => @this.Editor_RemoveTag(tag);

        /// <inheritdoc cref="CatalogItemAsset.Editor_AddDetail{TDetail}"/>
        public static TDetailAsset AddDetail<TDetailAsset>(this CatalogItemAsset @this)
            where TDetailAsset : BaseDetailAsset
            => @this.Editor_AddDetail<TDetailAsset>();
    }
}
