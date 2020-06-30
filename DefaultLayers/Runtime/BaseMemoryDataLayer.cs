using UnityEngine.GameFoundation.Data;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
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
        protected GameFoundationData GetData()
        {
            var inventoryData = (this as IInventoryDataLayer).GetData();

            var walletData = (this as IWalletDataLayer).GetData();

            var data = new GameFoundationData
            {
                version = m_Version,
                inventoryManagerData = inventoryData,
                walletData = walletData
            };

            return data;
        }
    }
}
