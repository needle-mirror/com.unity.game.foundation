using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    /// Serializable data structure that contains the state of the Wallet.
    /// </summary>
    [Serializable]
    public struct WalletData
    {
        /// <summary>
        /// Get an empty instance of this class.
        /// </summary>
        public static WalletData Empty => new WalletData
        {
            balances = new BalanceData[0]
        };

        /// <summary>
        /// The list of balances
        /// </summary>
        public BalanceData[] balances;
    }
}
