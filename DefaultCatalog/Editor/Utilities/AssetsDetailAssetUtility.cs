using UnityEngine.GameFoundation.DefaultCatalog.Details;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class AssetsDetailAssetUtility
    {
        /// <inheritdoc cref="AssetsDetailAsset.Editor_AddAsset(string, string)"/>
        public static void AddAsset(this AssetsDetailAsset @this, string assetName, string resourcePath)
            => @this.Editor_AddAsset(assetName, resourcePath);
    }
}
