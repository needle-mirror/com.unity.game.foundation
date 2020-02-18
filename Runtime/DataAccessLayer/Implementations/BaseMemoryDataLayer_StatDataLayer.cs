using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    public partial class BaseMemoryDataLayer
    {
        IStatDataLayer m_StatDataLayer;

        /// <summary>
        /// Initializes the data layer for <see cref="StatManager"/>.
        /// </summary>
        /// <param name="data">StatManager's serializable data.</param>
        protected void InitializeStatDataLayer(StatManagerSerializableData data)
        {
            m_StatDataLayer = new StatDataLayer(data);
        }

        /// <inheritdoc />
        StatManagerSerializableData IStatDataLayer.GetData()
            => m_StatDataLayer.GetData();

        /// <inheritdoc />
        void IStatDataLayer.SetStatValue<T>(int gameItemId, string statDefinitionId, T value, T defaultValue, Completer completer)
            => m_StatDataLayer.SetStatValue(gameItemId, statDefinitionId, value, defaultValue, completer);

        /// <inheritdoc />
        void IStatDataLayer.DeleteStatValue<T>(int gameItemId, string statDefinitionId, Completer completer)
            => m_StatDataLayer.DeleteStatValue<T>(gameItemId, statDefinitionId, completer);
    }
}
