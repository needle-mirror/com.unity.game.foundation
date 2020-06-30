namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public static class IapTransactionAssetUtility
    {
        /// <summary>
        /// Sets the Apple Product ID associated to this <see cref="IAPTransactionAsset"/> instance.
        /// </summary>
        /// <param name="this">The <see cref="IAPTransactionAsset"/> instance to set the Apple Product ID to.</param>
        /// <param name="id">The product ID to assign to this <see cref="IAPTransactionAsset"/> instance.</param>
        public static void SetAppleId(this IAPTransactionAsset @this, string id)
            => @this.m_AppleId = id;

        /// <summary>
        /// Sets the Google Product ID associated to this <see cref="IAPTransactionAsset"/> instance.
        /// </summary>
        /// <param name="this">The <see cref="IAPTransactionAsset"/> instance to set the Google Product ID to.</param>
        /// <param name="id">The product ID to assign to this <see cref="IAPTransactionAsset"/> instance.</param>
        public static void SetGoogleId(this IAPTransactionAsset @this, string id)
            => @this.m_GoogleId = id;
    }
}
