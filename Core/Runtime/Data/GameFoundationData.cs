using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    /// Serializable data structure that contains the state of Game Foundation.
    /// </summary>
    [Serializable]
    public struct GameFoundationData
    {
        /// <summary>
        /// Get an empty instance of this class.
        /// </summary>
        public static GameFoundationData Empty => new GameFoundationData
        {
            inventoryManagerData = new InventoryManagerData(),
            walletData = WalletData.Empty
        };

        /// <summary>
        /// The data of InventoryManager to be persisted.
        /// </summary>
        public InventoryManagerData inventoryManagerData;

        /// <summary>
        /// The data of Wallet to be persisted.
        /// </summary>
        public WalletData walletData;

        /// <summary>
        /// The version of of the save schematic
        /// </summary>
        public int version;
    }
}
