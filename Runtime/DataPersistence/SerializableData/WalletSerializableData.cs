using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of the Wallet.
    /// </summary>
    [Serializable]
    public struct WalletSerializableData
    {
        /// <summary>
        /// Get an empty instance of this class.
        /// </summary>
        public static WalletSerializableData Empty => new WalletSerializableData
        {
            balances = new BalanceSerializableData[0]
        };

        /// <summary>
        /// The list of balances
        /// </summary>
        public BalanceSerializableData[] balances;
    }
}
