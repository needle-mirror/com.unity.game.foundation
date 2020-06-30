using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Description for a <see cref="TransactionExchangeDefinition"/>
    /// </summary>
    [Serializable]
    public class TransactionExchangeDefinitionObject
    {
        /// <summary>
        /// The list of <see cref="ItemExchangeDefinitionObject"/>.
        /// </summary>
        [SerializeField]
        internal List<ItemExchangeDefinitionObject> m_Items;

        /// <summary>
        /// The list of <see cref="CurrencyExchangeObject"/>.
        /// </summary>
        [SerializeField]
        internal List<CurrencyExchangeObject> m_Currencies;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TransactionExchangeDefinitionObject"/> class.
        /// </summary>
        internal TransactionExchangeDefinitionObject()
        {
            m_Items = new List<ItemExchangeDefinitionObject>();
            m_Currencies = new List<CurrencyExchangeObject>();
        }

        /// <summary>
        /// Adds the item exchanges to the given <paramref name="target"/>
        /// collection.
        /// </summary>
        /// <param name="target">The target collection where the item exchange
        /// are added.</param>
        /// <returns>The number of item added.</returns>
        public int GetItems(ICollection<ItemExchangeDefinitionObject> target)
        {
            GFTools.ThrowIfArgNull(target, nameof(target));
            return GFTools.Copy(m_Items, target);
        }

        /// <summary>
        /// Gets a <see cref="ItemExchangeDefinitionObject"/> by its index.
        /// </summary>
        /// <param name="index">The index of the item exchange.</param>
        /// <returns>The item exchange.</returns>
        public ItemExchangeDefinitionObject GetItem(int index)
        {
            GFTools.ThrowIfOutOfRange(index, 0, m_Items.Count, nameof(index));
            return m_Items[index];
        }

        /// <summary>
        /// Gets the currency exchanges to the given <paramref name="target"/>
        /// collection.
        /// </summary>
        /// <param name="target">The target collection where the currency
        /// exchange are added.</param>
        /// <returns>The number of currencies added.</returns>
        public int GetCurrencies(ICollection<CurrencyExchangeObject> target)
        {
            GFTools.ThrowIfArgNull(target, nameof(target));
            return GFTools.Copy(m_Currencies, target);
        }

        /// <summary>
        /// Gets a currency exchange by its index.
        /// </summary>
        /// <param name="index">TYhe index of the currency exchange.</param>
        /// <returns>The currency exchange.</returns>
        public CurrencyExchangeObject GetCurrency(int index)
        {
            GFTools.ThrowIfOutOfRange(index, 0, m_Currencies.Count, nameof(index));
            return m_Currencies[index];
        }

        /// <summary>
        /// Creates a configuration for a <see cref="TransactionExchangeDefinition"/>
        /// </summary>
        /// <returns>The configuration.</returns>
        internal TransactionExchangeDefinitionConfig Configure()
        {
            var config = new TransactionExchangeDefinitionConfig();

            foreach (var currencyExchangeAsset in m_Currencies)
            {
                var currencyConfig = currencyExchangeAsset.Configure();
                config.currencies.Add(currencyConfig);
            }

            foreach (var itemExchangeAsset in m_Items)
            {
                var itemConfig = itemExchangeAsset.Configure();
                config.items.Add(itemConfig);
            }

            return config;
        }
    }
}
