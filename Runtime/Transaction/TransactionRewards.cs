using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes the rewards of a transaction process.
    /// </summary>
    public struct TransactionRewards
    {
        /// <summary>
        /// The items added.
        /// </summary>
        public IReadOnlyCollection<InventoryItem> items { get; internal set; }

        /// <summary>
        /// The currency balances added.
        /// </summary>
        public IReadOnlyCollection<CurrencyExchange> currencies { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionRewards"/>
        /// struct.
        /// </summary>
        /// <param name="items">The items of the transaction.</param>
        /// <param name="currencies">The currencies of th transaction</param>
        internal TransactionRewards(
            IReadOnlyCollection<InventoryItem> items,
            IReadOnlyCollection<CurrencyExchange> currencies)
        {
            this.items = items;
            this.currencies = currencies;
        }
    }
}
