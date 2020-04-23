#if UNITY_EDITOR

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class IAPTransactionAsset
    {
        /// <inheritdoc/>
        internal override string Editor_AssetPrefix => "IAPTransaction";
    }
}

#endif