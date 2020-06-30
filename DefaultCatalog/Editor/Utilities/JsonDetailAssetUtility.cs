using System;
using UnityEngine.GameFoundation.DefaultCatalog.Details;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    [Obsolete]
    public static class JsonDetailAssetUtility
    {
        /// <inheritdoc cref="JsonDetailAsset.Editor_SetJsonData(string)"/>
        public static void SetJsonData(this JsonDetailAsset @this, string jsonData)
            => @this.Editor_SetJsonData(jsonData);
    }
}
