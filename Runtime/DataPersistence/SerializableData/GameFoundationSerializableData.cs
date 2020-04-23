using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of Game Foundation.
    /// </summary>
    [Serializable]
    public struct GameFoundationSerializableData
    {
        /// <summary>
        /// Get an empty instance of this class.
        /// </summary>
        public static GameFoundationSerializableData Empty => new GameFoundationSerializableData
        {
            inventoryManagerData = InventoryManagerSerializableData.Empty,
            statManagerData = StatManagerSerializableData.Empty,
            walletData = WalletSerializableData.Empty
        };

        /// <summary>
        /// The data of InventoryManager to be persisted.
        /// </summary>
        public InventoryManagerSerializableData inventoryManagerData;

        /// <summary>
        /// The data of StatManager to be persisted.
        /// </summary>
        public StatManagerSerializableData statManagerData;

        /// <summary>
        /// The data of Wallet to be persisted.
        /// </summary>
        public WalletSerializableData walletData;

        /// <summary>
        /// The version of of the save schematic
        /// </summary>
        public int version;
    }
}
