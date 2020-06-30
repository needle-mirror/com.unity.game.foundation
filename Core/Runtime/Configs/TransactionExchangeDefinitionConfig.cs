using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    /// Configurator for a <see cref="TransactionExchangeDefinition"/> data.
    /// </summary>
    public class TransactionExchangeDefinitionConfig
    {
        /// <summary>
        /// The list of <see cref="CurrencyExchange"/> the
        /// <see cref="TransactionRewards"/> will contain.
        /// </summary>
        public readonly List<CurrencyExchangeDefinitionConfig> currencies =
            new List<CurrencyExchangeDefinitionConfig>();

        /// <summary>
        /// The list of <see cref="ItemExchangeDefinition"/> the
        /// <see cref="TransactionRewards"/> will contain.
        /// </summary>
        public readonly List<ItemExchangeDefinitionConfig> items =
            new List<ItemExchangeDefinitionConfig>();

        /// <summary>
        /// Checks the configuration and builds the
        /// <see cref="TransactionRewards"/> data.
        /// </summary>
        internal TransactionExchangeDefinition Compile()
        {
            var transactionExchange = new TransactionExchangeDefinition
            {
                m_Currencies = new CurrencyExchangeDefinition[currencies.Count],
                m_Items = new ItemExchangeDefinition[items.Count]
            };

            for (var i = 0; i < currencies.Count; i++)
            {
                var exchangeConfig = currencies[i];
                Tools.ThrowIfArgNull(exchangeConfig, nameof(currencies), i);

                var exchange = exchangeConfig.Compile();
                transactionExchange.m_Currencies[i] = exchange;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var exchangeConfig = items[i];
                Tools.ThrowIfArgNull(exchangeConfig, nameof(items), i);

                var exchange = exchangeConfig.Compile();
                transactionExchange.m_Items[i] = exchange;
            }

            return transactionExchange;
        }

        /// <summary>
        /// Resolves the possible references the
        /// <paramref name="transactionExchange"/> may contain.
        /// </summary>
        /// <param name="builder">The builder where the references can be
        /// found.</param>
        internal void Link
            (CatalogBuilder builder, TransactionExchangeDefinition transactionExchange)
        {
            Tools.ThrowIfArgNull(transactionExchange, nameof(transactionExchange));

            for (var i = 0; i < currencies.Count; i++)
            {
                var exchangeConfig = currencies[i];
                Tools.ThrowIfArgNull(exchangeConfig, nameof(currencies), i);

                var currencyExchanges = transactionExchange.m_Currencies;
                Tools.ThrowIfArgNull(currencyExchanges, nameof(transactionExchange.m_Currencies));

                exchangeConfig.Link(builder, ref transactionExchange.m_Currencies[i]);
            }

            for (var i = 0; i < items.Count; i++)
            {
                var exchangeConfig = items[i];
                exchangeConfig.Link(builder, ref transactionExchange.m_Items[i]);
            }
        }
    }
}
