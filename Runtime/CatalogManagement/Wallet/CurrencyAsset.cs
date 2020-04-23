using System;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Stores the information of a currency in a <see cref="ScriptableObject"/>.
    /// </summary>
    public partial class CurrencyAsset : CatalogItemAsset
    {
        /// <summary>
        /// The balance provided to a new player.
        /// </summary>
        [Tooltip("The balance given to a new player")]
        [SerializeField]
        internal long m_InitialBalance;

        /// <summary>
        /// The maximum balance a player can have.
        /// </summary>
        [Tooltip("The maximum balance a player can have")]
        [SerializeField]
        internal long m_MaximumBalance;

        /// <summary>
        /// Tells whether the currency is Soft or Hard.
        /// This value has no impact on the behaviour of the currency, and is
        /// just set for the matter of analytics.
        /// All currencies are handled the same way in Game Foundation.
        /// </summary>
        [SerializeField]
        internal CurrencyType m_Type;


        /// <inheritdoc cref="m_InitialBalance">
        public long initialBalance => m_InitialBalance;

        /// <inheritdoc cref="m_MaximumBalance">
        public long maximumBalance => m_MaximumBalance;

        /// <inheritdoc cref="m_Type">
        public CurrencyType type => m_Type;


        /// <inheritdoc />
        protected override
            CatalogItemConfig ConfigureItem(CatalogBuilder builder)
        {
            var config = builder.Create<CurrencyConfig>(id);
            config.maximumBalance = m_MaximumBalance;
            config.type = m_Type;
            return config;
        }
    }
}
