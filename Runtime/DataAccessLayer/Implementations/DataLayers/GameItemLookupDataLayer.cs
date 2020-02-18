using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Straightforward implementation of <see cref="IGameItemLookupDataLayer"/>.
    /// </summary>
    class GameItemLookupDataLayer : IGameItemLookupDataLayer
    {
        /// <summary>
        /// Stores the latest game item id used
        /// </summary>
        int m_LastGameItemIdUsed;

        /// <summary>
        /// Create a new <see cref="GameItemLookupDataLayer"/> with the given data.
        /// </summary>
        /// <param name="data">GameItemLookup's serializable data.</param>
        public GameItemLookupDataLayer(GameItemLookupSerializableData data)
        {
            m_LastGameItemIdUsed = data.lastGameItemIdUsed;
        }

        /// <inheritdoc />
        GameItemLookupSerializableData IGameItemLookupDataLayer.GetData()
        {
            return new GameItemLookupSerializableData
            {
                lastGameItemIdUsed = m_LastGameItemIdUsed
            };
        }

        /// <inheritdoc />
        void IGameItemLookupDataLayer.SetLastGameItemIdUsed(int value, Completer completer)
        {
            m_LastGameItemIdUsed = value;
            completer.Resolve();
        }
    }
}
