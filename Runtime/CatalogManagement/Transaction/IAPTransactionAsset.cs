using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Deascription for a <see cref="IAPTransaction"/>.
    /// </summary>
    public partial class IAPTransactionAsset : BaseTransactionAsset
    {
        /// <summary>
        /// The product ID for the Apple platform
        /// </summary>
        [SerializeField]
        internal string m_AppleId;

        /// <summary>
        /// The product ID for the Google platform
        /// </summary>
        [SerializeField]
        internal string m_GoogleId;

        /// <inheritdoc/>
        public string appleId => m_AppleId;

        /// <inheritdoc/>
        public string googleId => m_GoogleId;


        /// <summary>
        /// The product ID for the platform store
        /// </summary>
        public string productId =>
#if UNITY_IOS
            m_AppleId;
#elif UNITY_ANDROID
            m_GoogleId;
#else
            m_AppleId;                                              //TODO: defaulting to apple to permit unit testing--need better solution.
#endif

        /// <inheritdoc />
        protected override BaseTransactionConfig
            ConfigureTransaction(CatalogBuilder builder)
        {
            var item = builder.Create<IAPTransactionConfig>(id);
            item.productId = productId;
            return item;
        }
    }
}
