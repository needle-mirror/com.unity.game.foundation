using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    public partial class BaseMemoryDataLayer
    {
        /// <summary>
        /// The part of the data layer dedicaated to the stat management.
        /// </summary>
        internal StatDataLayer m_StatDataLayer;

        /// <summary>
        /// Initializes the data layer for <see cref="StatManager"/>.
        /// </summary>
        /// <param name="data">StatManager's serializable data.</param>
        protected void InitializeStatDataLayer(StatManagerSerializableData data)
        {
            m_StatDataLayer = new StatDataLayer(this, data);
        }

        /// <inheritdoc />
        StatManagerSerializableData IStatDataLayer.GetData()
            => (m_StatDataLayer as IStatDataLayer).GetData();

        /// <inheritdoc />
        StatValue IStatDataLayer.GetStatValue
            (string gameItemId, string statDefinitionId)

            => (m_StatDataLayer as IStatDataLayer)
                .GetStatValue(gameItemId, statDefinitionId);

        /// <inheritdoc />
        void IStatDataLayer.SetStatValue(
            string gameItemId,
            string statDefinitionId,
            StatValue value,
            Completer completer)

            => (m_StatDataLayer as IStatDataLayer)
                .SetStatValue(gameItemId, statDefinitionId, value, completer);

        /// <inheritdoc />
        void IStatDataLayer.DeleteStatValue
            (string gameItemId, string statDefinitionId, Completer completer)

            => (m_StatDataLayer as IStatDataLayer)
                .DeleteStatValue(gameItemId, statDefinitionId, completer);
    }
}
