using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Base for the memory data layers.
    /// </summary>
    public abstract partial class BaseMemoryDataLayer : IDataAccessLayer
    {
        protected int m_Version;

        /// <inheritdoc />
        public abstract void Initialize(Completer completer);

        /// <summary>
        /// Gets all the data from Game Foundation (for persistence)
        /// </summary>
        protected GameFoundationSerializableData GetData()
        {
            var inventoryData = (this as IInventoryDataLayer).GetData();

            var statData = (this as IStatDataLayer).GetData();

            var walletData = (this as IWalletDataLayer).GetData();

            var data = new GameFoundationSerializableData
            {
                version = m_Version,
                statManagerData = statData,
                inventoryManagerData = inventoryData,
                walletData = walletData
            };

            return data;
        }
    }
}
