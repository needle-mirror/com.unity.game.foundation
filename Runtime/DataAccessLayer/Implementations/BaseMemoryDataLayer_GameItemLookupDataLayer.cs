using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    public partial class BaseMemoryDataLayer
    {
        IGameItemLookupDataLayer m_GameItemLookupDataLayer;

        /// <summary>
        /// Initializes the data layer for <see cref="GameItemLookup"/>.
        /// </summary>
        /// <param name="data">GameItemLookup's serializable data.</param>
        protected void InitializeGameItemLookupDataLayer(GameItemLookupSerializableData data)
        {
            m_GameItemLookupDataLayer = new GameItemLookupDataLayer(data);
        }

        /// <inheritdoc />
        GameItemLookupSerializableData IGameItemLookupDataLayer.GetData()
        {
            return m_GameItemLookupDataLayer.GetData();
        }

        /// <inheritdoc />
        void IGameItemLookupDataLayer.SetLastGameItemIdUsed(int value, Completer completer)
            => m_GameItemLookupDataLayer.SetLastGameItemIdUsed(value, completer);
    }
}
