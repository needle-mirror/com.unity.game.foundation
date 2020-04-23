using System;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    /// Configurator for a <see cref="CurrencyExchangeDefinition"/>
    /// </summary>
    public struct CurrencyExchangeDefinitionConfig
    {
        /// <summary>
        /// The identifer of the currency.
        /// </summary>
        public string currency;

        /// <summary>
        /// The amount of the currency.
        /// </summary>
        public long amount;

        /// <summary>
        /// Checks the configuration and builds the
        /// <see cref="CurrencyExchangeDefinition"/> data.
        /// </summary>
        /// <returns>The <see cref="CurrencyExchangeDefinition"/> data.</returns>
        internal CurrencyExchangeDefinition Compile()
        {
            Tools.ThrowIfArgNullOrEmpty(currency, nameof(currency));
            Tools.ThrowIfArgNegative(amount, nameof(amount));

            var exchange = new CurrencyExchangeDefinition();
            exchange.amount = amount;
            return exchange;
        }

        /// <summary>
        /// Resolves the possible links this
        /// <see cref="CurrencyExchangeDefinition"/> may contain.
        /// </summary>
        /// <param name="builder">The builder where the references can be
        /// found.</param>
        /// <param name="exchange">The <see cref="CurrencyExchangeDefinition"/> to
        /// link.</param>
        internal void Link(CatalogBuilder builder, ref CurrencyExchangeDefinition exchange)
        {
            var catalogItem = builder.GetItemOrDie(currency);

            if (!(catalogItem is CurrencyConfig currencyConfig))
            {
                throw new Exception
                    ($"{nameof(CatalogItemConfig)} {currency} is not a valid {nameof(CurrencyConfig)}");
            }

            exchange.currency = currencyConfig.runtimeItem;
        }
    }
}
