using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    public partial class BaseMemoryDataLayer
    {
        /// <summary>
        /// Provides <see cref="List{CurrencyExchangeObject}"/> instances;
        /// </summary>
        static readonly Pool<List<CurrencyExchangeObject>> s_CurrencyExchangesListPool =
            new Pool<List<CurrencyExchangeObject>>(
                () => new List<CurrencyExchangeObject>(),
                list => list.Clear());

        /// <summary>
        /// Provides <see cref="List{ItemExchangeDefinitionObject}"/> instances;
        /// </summary>
        static readonly Pool<List<ItemExchangeDefinitionObject>> s_ItemExchangesListPool =
            new Pool<List<ItemExchangeDefinitionObject>>(
                () => new List<ItemExchangeDefinitionObject>(),
                list => list.Clear());

        /// <summary>
        /// Provides <see cref="List{Exception}"/> instances;
        /// </summary>
        static readonly Pool<List<Exception>> s_ExceptionListPool =
            new Pool<List<Exception>>(
                () => new List<Exception>(),
                list => list.Clear());

        /// <summary>
        /// Provides <see cref="List{string}"/> instances;
        /// </summary>
        static readonly Pool<List<string>> s_StringListPool =
            new Pool<List<string>>(
                () => new List<string>(),
                list => list.Clear());

        /// <summary>
        /// Provides <see cref="List{InventoryItemSerializableData}"/> instances;
        /// </summary>
        static readonly Pool<List<InventoryItemSerializableData>> s_ItemDataListPool =
            new Pool<List<InventoryItemSerializableData>>(
                () => new List<InventoryItemSerializableData>(),
                list => list.Clear());

        /// <summary>
        /// Provides <see cref="Dictionary{TKey, TValue}"/> instances;
        /// </summary>
        static readonly Pool<Dictionary<string, long>> s_DictionaryStringLongPool =
            new Pool<Dictionary<string, long>>(
                () => new Dictionary<string, long>(),
                dic => dic.Clear());

        /// <summary>
        /// We cannot guarantee that the <paramref name="transactionExchange"/>
        /// contains a unique entry for each currency or item.
        /// This method consolidates the list to regroup the amounts per
        /// currency/item.
        /// </summary>
        /// <param name="transactionExchange">The transaction exchange object</param>
        /// <param name="currencies">The target currencies exchange map.</param>
        /// <param name="items">The target items exchange map.</param>
        static void ConsolidateTransactionExchange(
            TransactionExchangeDefinitionObject transactionExchange,
            Dictionary<string, long> currencies,
            Dictionary<string, long> items)
        {
            var currencyExchanges = s_CurrencyExchangesListPool.Get();
            var itemExchanges = s_ItemExchangesListPool.Get();
            try
            {
                // Consolidating the currency costs.
                transactionExchange.GetCurrencies(currencyExchanges);
                foreach (var currencyExchange in currencyExchanges)
                {
                    var currencyId = currencyExchange.currency.id;

                    var found = currencies.TryGetValue
                        (currencyId, out var amount);

                    amount = currencyExchange.amount + (found ? amount : 0);
                    currencies[currencyId] = amount;
                }

                // Consolidating the item costs.
                transactionExchange.GetItems(itemExchanges);
                foreach (var itemExchange in itemExchanges)
                {
                    var itemId = itemExchange.item.id;

                    var found = items.TryGetValue(itemId, out var amount);

                    amount = itemExchange.amount + (found ? amount : 0);
                    items[itemId] = amount;
                }
            }
            finally
            {
                s_CurrencyExchangesListPool.Release(currencyExchanges);
                s_ItemExchangesListPool.Release(itemExchanges);
            }
        }

        /// <summary>
        /// Checks the costs of a virtual transaction with the player's
        /// resources.
        /// </summary>
        /// <param name="currencies">The currencies to pay</param>
        /// <param name="items">The items to consume</param>
        /// <param name="exceptions">The target collection where the errors are
        /// added.</param>
        void VerifyCost(
            Dictionary<string, long> currencies,
            Dictionary<string, long> items,
            ICollection<Exception> exceptions)
        {
            // Checking balances

            foreach (var exchange in currencies)
            {
                var balance = m_WalletDataLayer.GetBalance(exchange.Key);
                if (balance < exchange.Value)
                {
                    var exception = new NotEnoughBalanceException
                        (exchange.Key, exchange.Value, balance);
                    exceptions.Add(exception);
                }
            }

            // Checking items count.

            foreach (var exchange in items)
            {
                var count =
                    m_InventoryDataLayer.GetItemsByDefinition(exchange.Key);

                if (count < exchange.Value)
                {
                    var exception = new NotEnoughItemOfDefinitionException
                        (exchange.Key, exchange.Value, count);

                    exceptions.Add(exception);
                }
            }
        }

        /// <summary>
        /// Verifies if the item counterparts provided buy the player matches
        /// the item cost requirements.
        /// </summary>
        /// <param name="counterparts">The items provided by te player.</param>
        /// <param name="requirements">The item cost requirements.</param>
        /// <param name="consumed">The collection where consumed items are
        /// added.</param>
        /// <param name="exceptions">The target collection where the errors are
        /// added.</param>
        void VerifyItemPayload(
            ICollection<string> counterparts,
            Dictionary<string, long> requirements,
            ICollection<InventoryItemSerializableData> consumed,
            ICollection<Exception> exceptions)
        {
            var inventory = m_InventoryDataLayer as InventoryDataLayer;

            var uniqueCounterparts = s_StringListPool.Get();
            try
            {
                // Get unique item ids
                foreach (var itemId in counterparts)
                {
                    if (!uniqueCounterparts.Contains(itemId))
                    {
                        uniqueCounterparts.Add(itemId);
                    }
                }

                foreach (var counterpartId in uniqueCounterparts)
                {
                    // Check if the item exists
                    var itemFound = inventory.m_Items.TryGetValue
                        (counterpartId, out var itemData);

                    if (!itemFound)
                    {
                        var exception =
                            new InventoryItemNotFoundException(counterpartId);

                        exceptions.Add(exception);
                        continue;
                    }

                    // Get the definition and decrement/delete the requirements.
                    var definitionId = itemData.definitionId;

                    var requirementFound =
                        requirements.TryGetValue(definitionId, out var count);

                    if (requirementFound)
                    {
                        count--;
                        if (count > 0)
                        {
                            requirements[definitionId] = count;
                        }
                        else if (count == 0)
                        {
                            requirements.Remove(definitionId);
                        }

                        consumed.Add(itemData);
                    }

                    // I've decided not the throw an error if an item of the
                    // counterparts is not necessary.
                    //else
                    //{
                    //    var exception =
                    //        new Exception($"Wrong item for the transaction");

                    //    exceptions.Add(exception);
                    //}
                }
            }
            finally
            {
                s_StringListPool.Release(uniqueCounterparts);
            }

            // At the moment, if they is still an entry in the requirements,
            // that means the item payload didn't cover the requirements.

            foreach (var requirement in requirements)
            {
                var exception = new Exception
                    ($"Missing {requirement.Key} ({requirement.Value})");

                exceptions.Add(exception);
            }
        }

        /// <summary>
        /// Applies the rewards of a transaction.
        /// </summary>
        /// <param name="transaction">The transaction whose rewards are
        /// applied.</param>
        /// <returns>The description of the reward.</returns>
        TransactionExchangeData ApplyRewards(BaseTransactionAsset transaction)
        {
            var result = new TransactionExchangeData();

            var itemDataList = s_ItemDataListPool.Get();
            try
            {
                // [3a] Increment the currencies
                var currencyExchanges = s_CurrencyExchangesListPool.Get();
                try
                {
                    transaction.rewards.GetCurrencies(currencyExchanges);

                    var count = currencyExchanges.Count;
                    result.currencies = new CurrencyExchangeData[count];
                    var index = 0;

                    foreach (var exchange in currencyExchanges)
                    {
                        var currencyId = exchange.currency.id;
                        var balance = exchange.amount;
                        m_WalletDataLayer.AdjustBalance(currencyId, balance);

                        result.currencies[index++] = new CurrencyExchangeData
                        {
                            currencyId = currencyId,
                            amount = balance
                        };
                    }
                }
                finally
                {
                    s_CurrencyExchangesListPool.Release(currencyExchanges);
                }


                // [3c] Create the new items
                var itemExchanges = s_ItemExchangesListPool.Get();
                try
                {
                    transaction.rewards.GetItems(itemExchanges);

                    foreach (var exchange in itemExchanges)
                    {
                        var definitionId = exchange.item.id;

                        for (var i = 0; i < exchange.amount; i++)
                        {
                            var itemId =
                                m_InventoryDataLayer.CreateItem(definitionId);

                            var itemData = new InventoryItemSerializableData
                            {
                                id = itemId,
                                definitionId = definitionId
                            };

                            itemDataList.Add(itemData);
                            m_StatDataLayer.InitStats(itemId);
                        }
                    }
                }
                finally
                {
                    s_ItemExchangesListPool.Release(itemExchanges);
                }

                result.items = itemDataList.ToArray();
            }
            finally
            {
                s_ItemDataListPool.Release(itemDataList);
            }

            return result;
        }

        /// <summary>
        /// Redeems an IAP.
        /// </summary>
        /// <param name="id">The identifier of the IAP Transaction to
        /// redeem.</param>
        /// <param name="completer">The completer of the caller of this
        /// method.</param>
        void RedeemIap(string id, Completer<TransactionExchangeData> completer)
        {
            try
            {
                var transactionCatalog = database.m_TransactionCatalog;

                // I get the transaction description form its id.

                var transaction =
                    transactionCatalog.FindItem(id) as IAPTransactionAsset;

                if (transaction is null)
                {
                    throw new KeyNotFoundException
                        ($"{nameof(IAPTransactionAsset)} {id} not found.");
                }

                var result = ApplyRewards(transaction);
                completer.Resolve(result);
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc/>
        void ITransactionDataLayer.MakeVirtualTransaction(
            string id,
            ICollection<string> counterparts,
            Completer<VirtualTransactionExchangeData> completer)
        {
            try
            {
                var transactionCatalog = database.m_TransactionCatalog;

                // [1] I get the transaction description form its id.

                var transaction =
                    transactionCatalog.FindItem(id) as VirtualTransactionAsset;

                if (transaction is null)
                {
                    throw new KeyNotFoundException
                        ($"{nameof(VirtualTransactionAsset)} {id} not found.");
                }


                // [2] I check the cost of this transaction to be sure that the
                //     player fulfills its requirements.

                var currencyExchanges = s_DictionaryStringLongPool.Get();
                var itemExchanges = s_DictionaryStringLongPool.Get();
                var consumed = s_ItemDataListPool.Get();

                try
                {
                    // [2a] I'm consolidating the costs in a dictionary so I'm
                    //      sure that I have only one amount for each currency
                    //      and each inventory item definition.

                    ConsolidateTransactionExchange
                        (transaction.costs, currencyExchanges, itemExchanges);


                    // [2b] Now I need to validate that the player fulfills the
                    //      requirements of this transaction.
                    //      That means:
                    //      - Check the wallet to validate that he has enough of
                    //        the required currencies
                    //      - Check the specified item ids to confirm their
                    //        existence in the player inventory, and their match
                    //        with the requirement of the transaction.

                    var exceptions = s_ExceptionListPool.Get();
                    try
                    {
                        VerifyCost
                            (currencyExchanges, itemExchanges, exceptions);

                        VerifyItemPayload
                            (counterparts, itemExchanges, consumed, exceptions);

                        // If I found some errors while checking the transaction
                        // requirements, I reject the completer with this list
                        // of errors.

                        if (exceptions.Count > 0)
                        {
                            throw new AggregateException(exceptions);
                        }
                    }
                    finally
                    {
                        s_ExceptionListPool.Release(exceptions);
                    }


                    // [3] Perform the transaction:
                    //     a. Consuming the currencies
                    //     b. Consuming the items
                    //     c. Apply rexards
                    //     d. Complete the promise

                    var result = new VirtualTransactionExchangeData();

                    // [3a] Consuming the currencies
                    {
                        var count = currencyExchanges.Count;
                        result.cost.currencies = new CurrencyExchangeData[count];
                        var index = 0;

                        foreach (var exchange in currencyExchanges)
                        {
                            var currencyId = exchange.Key;
                            var balance = -exchange.Value;
                            m_WalletDataLayer.AdjustBalance(currencyId, balance);

                            result.cost.currencies[index++] = new CurrencyExchangeData
                            {
                                currencyId = currencyId,
                                amount = balance
                            };
                        }
                    }


                    // [3b] Consuming the items
                    {
                        foreach (var itemData in consumed)
                        {
                            m_InventoryDataLayer.DeleteItem(itemData.id);
                        }
                        result.cost.items = consumed.ToArray();
                    }

                    // [3c] Applying the rewards
                    result.rewards = ApplyRewards(transaction);

                    // [3d] Resolving the promise
                    completer.Resolve(result);
                }
                finally
                {
                    s_DictionaryStringLongPool.Release(itemExchanges);
                    s_DictionaryStringLongPool.Release(currencyExchanges);
                    s_ItemDataListPool.Release(consumed);
                }
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc/>
        void ITransactionDataLayer.RedeemAppleIap
            (string id, string receipt, Completer<TransactionExchangeData> completer)

            => RedeemIap(id, completer);

        /// <inheritdoc/>
        void ITransactionDataLayer.RedeemGoogleIap(
            string id,
            string purchaseData,
            string purchaseDataSignature,
            Completer<TransactionExchangeData> completer)

            => RedeemIap(id, completer);
    }
}
