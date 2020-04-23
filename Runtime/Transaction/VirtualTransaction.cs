using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Describes the Virtual transaction info, and outcome.
    /// </summary>
    public sealed class VirtualTransaction : BaseTransaction
    {
        /// <summary>
        /// 
        /// </summary>
        public TransactionExchangeDefinition costs { get; internal set; }

        /// <summary>
        ///     Test whether the costs can be met be the currencies in the wallet and/or the items in the inventory.
        /// </summary>
        /// <param name="exceptions">
        ///     A collection to populate with all exceptions generated.
        /// </param>
        public void VerifyCost(ICollection<System.Exception> exceptions)
        {
            exceptions?.Clear();

            foreach (var currencyExchange in costs.m_Currencies)
            {
                var balanceInWallet = WalletManager.GetBalance(currencyExchange.currency.id);

                if (balanceInWallet >= currencyExchange.amount) continue;

                exceptions?.Add(new NotEnoughBalanceException
                    (currencyExchange.currency.id, currencyExchange.amount, balanceInWallet));
            }

            foreach (var itemExchange in costs.m_Items)
            {
                var items = InventoryManager.FindItemsByDefinition(itemExchange.item);

                if (items.Length >= itemExchange.amount) continue;

                exceptions?.Add(new NotEnoughItemOfDefinitionException
                    (itemExchange.item.id, itemExchange.amount, items.Length));
            }
        }

        /// <summary>
        ///     Gets a list of the first InventoryItem ids from the inventory that satisfy the cost of a transaction.
        /// </summary>
        /// <param name="costItemIds">
        ///     An existing collection of strings to populate (must be non-null and must be empty).
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if the collection passed in is not empty.
        /// </exception>
        /// <exception cref="NotEnoughItemOfDefinitionException">
        ///     Thrown if the cost can't be met. (You should use VerifyCost before using this method.)
        /// </exception>
        public void AutoFillCostItemIds(ICollection<string> costItemIds)
        {
            if (costItemIds == null || costItemIds.Count > 0)
            {
                throw new System.ArgumentException("Cannot pass a null or non-empty collection into FindFirstValidItemsForCost.");
            }

            foreach (var itemExchange in costs.m_Items)
            {
                var items = InventoryManager.FindItemsByDefinition(itemExchange.item);

                if (items.Length < itemExchange.amount)
                {
                    // this method is not for validation
                    // you should verify costs before calling this method
                    throw new NotEnoughItemOfDefinitionException
                        (itemExchange.item.id, itemExchange.amount, items.Length);
                }

                for (var i = 0; i < itemExchange.amount; i++)
                {
                    costItemIds.Add(items[i].id);
                }
            }
        }
    }
}
