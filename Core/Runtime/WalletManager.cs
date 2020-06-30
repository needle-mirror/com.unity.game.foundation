using System;
using System.Collections.Generic;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Manages the player currency balances.
    /// </summary>
    public static class WalletManager
    {
        /// <summary>
        /// Accessor to GameFoundation's current DAL.
        /// </summary>
        static IWalletDataLayer dataLayer => GameFoundation.dataLayer;

        /// <summary>
        /// The cached balances.
        /// This dictionary is the one used when getting the balances so there
        /// is no need to ask the data layer.
        /// </summary>
        internal static Dictionary<Currency, long> m_Balances = new Dictionary<Currency, long>();

        /// <summary>
        /// Triggered every time a balance is modified, whether added, removed,
        /// or set.
        /// </summary>
        public static event Action<BalanceChangedEventArgs> balanceChanged;

        /// <summary>
        /// Tells whether the wallet manager is initialized or not.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the wallet manager.
        /// </summary>
        internal static void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"{nameof(WalletManager)} is already initialized and cannot be initialized again.");
                return;
            }

            try
            {
                InitializeData();
                IsInitialized = true;
            }
            catch (Exception)
            {
                Uninitialize();

                throw;
            }
        }

        /// <summary>
        /// Resets the wallet manager.
        /// </summary>
        internal static void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            m_Balances.Clear();
            IsInitialized = false;
        }

        /// <summary>
        /// Initializes the player balances.
        /// For each currency available in the current catalog, it gets the
        /// persisted player balance, or set it to 0.
        /// </summary>
        static void InitializeData()
        {
            var data = dataLayer.GetData();
            var catalog = GameFoundation.catalogs.currencyCatalog;
            var currencies = catalog.m_Items;

            foreach (var currency in currencies.Values)
            {
                long balance = 0;

                foreach (var balanceEntry in data.balances)
                {
                    if (balanceEntry.currencyKey == currency.key)
                    {
                        balance = balanceEntry.balance;
                        break;
                    }
                }

                m_Balances.Add(currency, balance);
            }
        }

        /// <summary>
        /// Gets the currency by its <paramref name="key"/>
        /// </summary>
        /// <param name="key">Identifier of the currency to find.</param>
        /// <param name="paramName">The name of the currency key parameter, in
        /// the scope of the caller.</param>
        /// <returns>The currency matching with the specified
        /// <paramref name="key"/></returns>
        static Currency GetCurrency(string key, string paramName)
        {
            Tools.ThrowIfArgNullOrEmpty(key, paramName);

            var currency = GameFoundation.catalogs.currencyCatalog.FindItem(key);

            if (currency is null)
            {
                throw new ArgumentException($"{key} is not a valid currency", paramName);
            }

            return currency;
        }

        /// <summary>
        /// Gets the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currency">The currency you want the balance of.</param>
        /// <returns>The balance of the specified currency.</returns>
        public static long GetBalance(Currency currency)
        {
            Tools.ThrowIfArgNull(currency, nameof(currency));
            return m_Balances[currency];
        }

        /// <summary>
        /// Gets the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currencyKey">The identifier of the currency you want the balance of.</param>
        /// <returns>The balance of the specified currency.</returns>
        public static long GetBalance(string currencyKey)
        {
            var currency = GetCurrency(currencyKey, nameof(currencyKey));
            return GetBalance(currency);
        }

        // Sets the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool SetBalanceInternal(Currency currency, long balance)
        {
            Tools.ThrowIfArgNull(currency, nameof(currency));
            Tools.ThrowIfArgNegative(balance, nameof(balance));

            if (!m_Balances.TryGetValue(currency, out var oldBalance))
            {
                throw new ArgumentException($"{currency.displayName} not found", nameof(currency));
            }

            if (currency.maximumBalance != 0 && balance > currency.maximumBalance)
            {
                return false;
            }

            if (oldBalance == balance)
            {
                return true;
            }

            m_Balances[currency] = balance;

            var eventArgs = new BalanceChangedEventArgs(currency, oldBalance, balance);
            balanceChanged?.Invoke(eventArgs);

            return true;
        }

        /// <summary>
        /// Sets the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currency">The currency you want to set the balance.</param>
        /// <param name="balance">The amount to add to the balance.</param>
        /// <returns>true if the balance has been updated, false otherwise.</returns>
        public static bool SetBalance(Currency currency, long balance)
        {
            if (!SetBalanceInternal(currency, balance)) return false;

            dataLayer.SetBalance(currency.key, balance, Completer.None);

            return true;
        }

        // Sets the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool SetBalanceInternal(string currencyKey, long balance)
        {
            var currency = GetCurrency(currencyKey, nameof(currencyKey));
            return SetBalanceInternal(currency, balance);
        }

        /// <summary>
        /// Sets the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currencyKey">Identifier of the currency you want to set the balance.</param>
        /// <param name="balance">The amount to add to the balance.</param>
        /// <returns>true if the balance has been updated, false otherwise.</returns>
        public static bool SetBalance(string currencyKey, long balance)
        {
            var currency = GetCurrency(currencyKey, nameof(currencyKey));
            return SetBalance(currency, balance);
        }

        // Increases the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool AddBalanceInternal(Currency currency, long balance)
        {
            Tools.ThrowIfArgNull(currency, nameof(currency));
            Tools.ThrowIfArgNegative(balance, nameof(balance));

            if (!m_Balances.TryGetValue(currency, out var oldBalance))
            {
                throw new ArgumentException($"{currency.displayName} not found", nameof(currency));
            }

            if (balance == 0)
            {
                return true;
            }

            var newBalance = oldBalance + balance;

            if (currency.maximumBalance != 0 && newBalance > currency.maximumBalance)
            {
                return false;
            }

            m_Balances[currency] = newBalance;

            var eventArgs = new BalanceChangedEventArgs(currency, oldBalance, newBalance);
            balanceChanged?.Invoke(eventArgs);

            return true;
        }

        /// <summary>
        /// Increases the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currency">The currency you want to increase the balance.</param>
        /// <param name="balance">The amount to add to the balance.</param>
        /// <returns>true if the update is valid, false otherwise.</returns>
        public static bool AddBalance(Currency currency, long balance)
        {
            if (!AddBalanceInternal(currency, balance))
            {
                return false;
            }

            dataLayer.AddBalance(currency.key, balance, Completer<long>.None);

            return true;
        }

        // Increases the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool AddBalanceInternal(string currencyKey, long balance)
        {
            var currency = GetCurrency(currencyKey, nameof(currencyKey));
            return AddBalanceInternal(currency, balance);
        }

        /// <summary>
        /// Increases the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currencyKey">Identifier of the currency you want to increase the balance.</param>
        /// <param name="balance">The amount to add to the balance.</param>
        /// <returns>true if the update is valid, false otherwise.</returns>
        public static bool AddBalance(string currencyKey, long balance)
        {
            var currency = GetCurrency(currencyKey, nameof(currencyKey));
            return AddBalance(currency, balance);
        }

        // Decreases the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool RemoveBalanceInternal(Currency currency, long balance)
        {
            Tools.ThrowIfArgNull(currency, nameof(currency));
            Tools.ThrowIfArgNegative(balance, nameof(balance));

            if (!m_Balances.TryGetValue(currency, out var oldBalance))
            {
                throw new ArgumentException($"{currency.displayName} not found", nameof(currency));
            }

            if (balance == 0)
            {
                return true;
            }

            var newBalance = oldBalance - balance;

            if (newBalance < 0)
            {
                return false;
            }

            m_Balances[currency] = newBalance;

            var eventArgs = new BalanceChangedEventArgs(currency, oldBalance, newBalance);
            balanceChanged?.Invoke(eventArgs);

            return true;
        }

        /// <summary>
        /// Decreases the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currency">The currency you want to decrease the balance.</param>
        /// <param name="balance">The amount to remove to the balance.</param>
        /// <returns>true if the update is valid, false otherwise.</returns>
        public static bool RemoveBalance(Currency currency, long balance)
        {
            if (!RemoveBalanceInternal(currency, balance))
            {
                return false;
            }

            dataLayer.RemoveBalance(currency.key, balance, Completer<long>.None);

            return true;
        }

        // Decreases the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool RemoveBalanceInternal(string currencyKey, long balance)
        {
            var currency = GetCurrency(currencyKey, nameof(currencyKey));
            return RemoveBalanceInternal(currency, balance);
        }

        /// <summary>
        /// Decreases the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currencyKey">Identifier of the currency you want to decrease the balance.</param>
        /// <param name="balance">The amount to remove to the balance.</param>
        /// <returns>true if the update is valid, false otherwise.</returns>
        public static bool RemoveBalance(string currencyKey, long balance)
        {
            var currency = GetCurrency(currencyKey, nameof(currencyKey));
            return RemoveBalance(currency, balance);
        }
    }
}
