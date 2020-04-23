using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of the currency balance.
    /// </summary>
    [Serializable]
    public struct BalanceSerializableData
    {
        /// <summary>
        /// The ID of the currency
        /// </summary>
        public string currencyId;

        /// <summary>
        /// The balance of the currency
        /// </summary>
        public long balance;
    }
}
