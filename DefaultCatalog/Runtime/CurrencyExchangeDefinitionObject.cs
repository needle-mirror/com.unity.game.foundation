using System;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Description for a <see cref="CurrencyExchange"/>
    /// </summary>
    [Serializable]
    public class CurrencyExchangeObject
    {
        /// <inheritdoc cref="currency"/>
        [SerializeField]
        internal CurrencyAsset m_Currency;

        /// <inheritdoc cref="amount"/>
        [SerializeField]
        internal long m_Amount;

        /// <summary>
        /// The currency of the exchange.
        /// </summary>
        public CurrencyAsset currency => m_Currency;

        /// <summary>
        /// The amount of currency
        /// </summary>
        public long amount => m_Amount;

        /// <summary>
        /// Creates a <see cref="CurrencyExchangeDefinitionConfig"/> from the data of this
        /// <see cref="CurrencyExchangeObject"/>.
        /// </summary>
        /// <returns>The config.</returns>
        internal CurrencyExchangeDefinitionConfig Configure() =>
            new CurrencyExchangeDefinitionConfig
            {
                currency = m_Currency.key,
                amount = m_Amount
            };
    }
}
