using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Description for <see cref="BaseTransactionAsset"/>
    /// </summary>
    public abstract partial class BaseTransactionAsset : CatalogItemAsset
    {
        /// <inheritdoc cref="rewards"/>
        [SerializeField]
        internal TransactionExchangeDefinitionObject m_Rewards;


        /// <inheritdoc/>
        protected sealed override void AwakeDefinition()
        {
            if (m_Rewards is null)
            {
                m_Rewards = new TransactionExchangeDefinitionObject();
            }

            AwakeTransaction();
        }

        /// <summary>
        /// Overriden by inherited classes to initialize specific members.
        /// </summary>
        protected virtual void AwakeTransaction() { }

        /// <summary>
        /// The reward description of the transaction.
        /// </summary>
        public TransactionExchangeDefinitionObject rewards => m_Rewards;

        /// <inheritdoc />
        protected sealed override
            CatalogItemConfig ConfigureItem(CatalogBuilder builder)
        {
            var item = ConfigureTransaction(builder);
            var rewardConfig = m_Rewards.Configure();
            item.rewards = rewardConfig;
            return item;
        }

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the
        /// specific content of this transaction.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        /// <returns>The config object.</returns>
        protected abstract BaseTransactionConfig
            ConfigureTransaction(CatalogBuilder builder);
    }
}
