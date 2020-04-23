using System;
using System.Collections.Generic;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    public delegate void BalanceChangedHandler(Currency currency, long oldBalance, long newBalance);

    /// <summary>
    /// Manages the player currency balances.
    /// </summary>
    public static class WalletManager
    {
        /// <summary>
        /// The reference to the data layer dedicated to the balances
        /// management.
        /// </summary>
        internal static IWalletDataLayer s_DataLayer;

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
        public static event BalanceChangedHandler balanceChanged;

        /// <summary>
        /// Tells whether the wallet manager is initialized or not.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the wallet manager.
        /// </summary>
        /// <param name="dataLayer">A data layer object which will share its
        /// data and gets balance update notifications.</param>
        internal static void Initialize(IWalletDataLayer dataLayer)
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"{nameof(WalletManager)} is already initialized and cannot be initialized again.");
                return;
            }

            s_DataLayer = dataLayer;

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
            s_DataLayer = null;
            IsInitialized = false;
        }

        /// <summary>
        /// Initializes the player balances.
        /// For each currency available in the current catalog, it gets the
        /// persisted player balance, or set it to 0.
        /// </summary>
        static void InitializeData()
        {
            var data = s_DataLayer.GetData();
            var catalog = GameFoundation.catalogs.currencyCatalog;
            var currencies = catalog.m_Items;

            foreach (var currency in currencies.Values)
            {
                long balance = 0;

                foreach (var balanceEntry in data.balances)
                {
                    if (balanceEntry.currencyId == currency.id)
                    {
                        balance = balanceEntry.balance;
                        break;
                    }
                }

                m_Balances.Add(currency, balance);
            }
        }

        /// <summary>
        /// Gets the currency by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">Id of the currency to find.</param>
        /// <param name="paramName">The name of the currency id parameter, in
        /// the scope of the caller.</param>
        /// <returns>The currency matching with the specified
        /// <paramref name="id"/></returns>
        static Currency GetCurrency(string id, string paramName)
        {
            Tools.ThrowIfArgNullOrEmpty(id, paramName);

            var currency = GameFoundation.catalogs.currencyCatalog.FindItem(id);

            if (currency is null)
            {
                throw new ArgumentException($"{id} is not a valid currency", paramName);
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
        /// <param name="currencyId">The id of the currency you want the balance of.</param>
        /// <returns>The balance of the specified currency.</returns>
        public static long GetBalance(string currencyId)
        {
            var currency = GetCurrency(currencyId, nameof(currencyId));
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

            balanceChanged?.Invoke(currency, oldBalance, balance);

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

            s_DataLayer.SetBalance(currency.id, balance, Completer.None);

            return true;
        }

        // Sets the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool SetBalanceInternal(string currencyId, long balance)
        {
            var currency = GetCurrency(currencyId, nameof(currencyId));
            return SetBalanceInternal(currency, balance);
        }

        /// <summary>
        /// Sets the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currencyId">Id of the currency you want to set the balance.</param>
        /// <param name="balance">The amount to add to the balance.</param>
        /// <returns>true if the balance has been updated, false otherwise.</returns>
        public static bool SetBalance(string currencyId, long balance)
        {
            var currency = GetCurrency(currencyId, nameof(currencyId));
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

            balanceChanged?.Invoke(currency, oldBalance, newBalance);

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

            s_DataLayer.AddBalance(currency.id, balance, Completer<long>.None);

            return true;
        }

        // Increases the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool AddBalanceInternal(string currencyId, long balance)
        {
            var currency = GetCurrency(currencyId, nameof(currencyId));
            return AddBalanceInternal(currency, balance);
        }

        /// <summary>
        /// Increases the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currencyId">If of the currency you want to increase the balance.</param>
        /// <param name="balance">The amount to add to the balance.</param>
        /// <returns>true if the update is valid, false otherwise.</returns>
        public static bool AddBalance(string currencyId, long balance)
        {
            var currency = GetCurrency(currencyId, nameof(currencyId));
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

            balanceChanged?.Invoke(currency, oldBalance, newBalance);

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

            s_DataLayer.RemoveBalance(currency.id, balance, Completer<long>.None);

            return true;
        }

        // Decreases the balance of the specified Currency,
        // but does not sync with the data layer.
        internal static bool RemoveBalanceInternal(string currencyId, long balance)
        {
            var currency = GetCurrency(currencyId, nameof(currencyId));
            return RemoveBalanceInternal(currency, balance);
        }

        /// <summary>
        /// Decreases the balance of the specified <see cref="Currency"/>.
        /// </summary>
        /// <param name="currencyId">Id of the currency you want to decrease the balance.</param>
        /// <param name="balance">The amount to remove to the balance.</param>
        /// <returns>true if the update is valid, false otherwise.</returns>
        public static bool RemoveBalance(string currencyId, long balance)
        {
            var currency = GetCurrency(currencyId, nameof(currencyId));
            return RemoveBalance(currency, balance);
        }
    }
}
