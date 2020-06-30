using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    /// Serializable data structure that contains the state of the currency balance.
    /// </summary>
    [Serializable]
    public struct BalanceData
    {
        /// <summary>
        /// The Key of the currency
        /// </summary>
        public string currencyKey;

        /// <summary>
        /// The balance of the currency
        /// </summary>
        public long balance;
    }
}
