using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    /// Hidden classes which focuses on dealing with the wallet for the <see cref="BaseMemoryDataLayer"/> object.
    /// </summary>
    class WalletDataLayer : IWalletDataLayer
    {
        /// <summary>
        /// Stores the balance for each <see cref="Currency"/>
        /// </summary>
        readonly Dictionary<string, long> m_Balances;

        /// <summary>
        /// Initializes a new <see cref="WalletDataLayer"/> instance.
        /// </summary>
        /// <param name="data">The data to initialize the <see cref="WalletDataLayer"/> object with.</param>
        /// <param name="catalog">The currency catalog used as source of truth.</param>
        public WalletDataLayer(WalletData data, CurrencyCatalogAsset catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            m_Balances = new Dictionary<string, long>();

            var currencies = catalog.m_Items;
            var balances = data.balances;

            foreach (var currency in currencies)
            {
                var found = false;

                // On ChilliConnect, existing players wallet are not updated
                // when a new currency is added, so we don't add any missing
                // currency with their initial balance.
                long balance = 0;
                if (balances != null)
                {
                    foreach (var balanceData in balances)
                    {
                        found = balanceData.currencyKey == currency.key;

                        if (found)
                        {
                            balance = balanceData.balance;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    Debug.LogWarning($"Data should have a balance for currency {currency.key}");
                }

                m_Balances.Add(currency.key, balance);
            }
        }

        /// <summary>
        /// Tries to get the balance of the specified
        /// <paramref name="currencyKey"/>.
        /// </summary>
        /// <param name="currencyKey">The identifier of the currency to get the balance
        /// of.</param>
        /// <param name="balance">if found, it stores the balance of the
        /// requested currency.</param>
        /// <returns><c>true</c> is found, <c>false</c> otherwise.</returns>
        public bool TryGetBalance(string currencyKey, out long balance)
            => m_Balances.TryGetValue(currencyKey, out balance);

        /// <summary>
        /// Checks if the specified <paramref name="balance"/> fits with the
        /// constraints of the currency.
        /// </summary>
        /// <param name="currencyKey">The identifier of the currency to check the
        /// balance for.</param>
        /// <param name="balance">The candidate balance to the specific
        /// currency</param>
        static void CheckBalance(string currencyKey, long balance)
        {
            if (balance < 0)
            {
                throw new OverflowException("Not enough currency");
            }

            var catalog = GameFoundation.catalogs.currencyCatalog;

            var currency = catalog.FindItem(currencyKey);
            if (currency is null)
            {
                throw new Exception($"Currency {currencyKey} not found");
            }

            var maximum = currency.maximumBalance;

            if (maximum != 0 && balance > maximum)
            {
                throw new OverflowException
                    ($"{balance} exceeds the limits ({maximum})");
            }
        }

        /// <summary>
        /// Add or remove the <paramref name="amount"/> to/from the existing
        /// balance of the currency given by its <paramref name="currencyKey"/>.
        /// </summary>
        /// <param name="currencyKey">The identifier of currency to adjust.</param>
        /// <param name="amount">The amount to add (if positive) or remove
        /// (if negative).</param>
        /// <returns>The up-to-date value of the balance.</returns>
        public long AdjustBalance(string currencyKey, long amount)
        {
            m_Balances.TryGetValue(currencyKey, out var oldBalance);
            var newBalance = oldBalance + amount;

            CheckBalance(currencyKey, newBalance);

            m_Balances[currencyKey] = newBalance;

            return newBalance;
        }

        /// <summary>
        /// Sets the balance of the currency given by its
        /// <paramref name="currencyKey"/> to the given
        /// <paramref name="balance"/>.
        /// </summary>
        /// <param name="currencyKey">The identifier of the currency to set.</param>
        /// <param name="balance">The new balance.</param>
        public void SetBalance(string currencyKey, long balance)
        {
            CheckBalance(currencyKey, balance);
            m_Balances[currencyKey] = balance;
        }

        public long GetBalance(string currencyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(currencyKey, nameof(currencyKey));

            var found = m_Balances.TryGetValue(currencyKey, out var balance);
            if (!found)
            {
                throw new ArgumentException($"Currency {currencyKey} Not found", nameof(currencyKey));
            }

            return balance;
        }

        /// <inheritdoc />
        WalletData IWalletDataLayer.GetData()
        {
            var data = new WalletData
            {
                balances = new BalanceData[m_Balances.Count]
            };
            var index = 0;
            foreach (var balanceEntry in m_Balances)
            {
                data.balances[index++] = new BalanceData
                {
                    currencyKey = balanceEntry.Key,
                    balance = balanceEntry.Value
                };
            }

            return data;
        }

        /// <inheritdoc />
        void IWalletDataLayer.AddBalance(string currencyKey, long balance, Completer<long> completer)
        {
            // Check balance validity
            {
                var isRejected = Tools.RejectIfArgNegative(balance, nameof(balance), completer);
                if (isRejected)
                    return;
            }

            try
            {
                var newBalance = AdjustBalance(currencyKey, balance);
                completer.Resolve(newBalance);
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc />
        void IWalletDataLayer.RemoveBalance(string currencyKey, long balance, Completer<long> completer)
        {
            // Check balance validity
            {
                var isRejected = Tools.RejectIfArgNegative(balance, nameof(balance), completer);
                if (isRejected)
                    return;
            }

            try
            {
                var newBalance = AdjustBalance(currencyKey, -balance);
                completer.Resolve(newBalance);
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }

        /// <inheritdoc />
        void IWalletDataLayer.SetBalance(string currencyKey, long balance, Completer completer)
        {
            try
            {
                SetBalance(currencyKey, balance);
                completer.Resolve();
            }
            catch (Exception e)
            {
                completer.Reject(e);
            }
        }
    }
}
