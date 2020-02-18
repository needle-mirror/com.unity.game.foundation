using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
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
            var inventoryData = m_InventoryDataLayer.GetData();

            var lookupData = m_GameItemLookupDataLayer.GetData();

            var statData = m_StatDataLayer.GetData();

            var data = new GameFoundationSerializableData
            {
                version = m_Version,
                statManagerData = statData,
                inventoryManagerData = inventoryData,
                gameItemLookupData = lookupData
            };

            return data;
        }
    }
}
