using System;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Straightforward synchronous data layer that keep data in memory for the session only.
    /// </summary>
    public class MemoryDataLayer : BaseMemoryDataLayer
    {
        bool m_IsInitialized;

        GameFoundationSerializableData m_Data;

        /// <summary>
        /// Create a data layer with empty data.
        /// </summary>
        public MemoryDataLayer()
        {
            m_Data = GameFoundationSerializableData.Empty;
        }

        /// <summary>
        /// Create a data layer that will handle the given data for the current game session only.
        /// </summary>
        /// <param name="data">GameFoundation's serializable data.</param>
        /// <exception cref="ArgumentNullException">
        /// If the given data contains invalid null values.
        /// </exception>
        public MemoryDataLayer(GameFoundationSerializableData data)
        {
            if (data.inventoryManagerData.inventories == null)
                throw new ArgumentNullException(
                    $"{nameof(InventoryManagerSerializableData)}'s {nameof(InventoryManagerSerializableData.inventories)} mustn't be null.",
                    new NullReferenceException());

            if (data.inventoryManagerData.items == null)
                throw new ArgumentNullException(
                    $"{nameof(InventoryManagerSerializableData)}'s {nameof(InventoryManagerSerializableData.items)} mustn't be null.",
                    new NullReferenceException());

            if (data.statManagerData.statDictionaries == null)
                throw new ArgumentNullException(
                    $"{nameof(StatManagerSerializableData)}'s {nameof(StatManagerSerializableData.statDictionaries)} mustn't be null.",
                    new NullReferenceException());

            m_Data = data;
        }

        /// <inheritdoc />
        public override void Initialize(Completer completer)
        {
            if (m_IsInitialized)
            {
                //Re-initializing an object already initialized is a silent error.
                completer.Resolve();

                return;
            }

            InitializeInventoryDataLayer(m_Data.inventoryManagerData);
            InitializeGameItemLookupDataLayer(m_Data.gameItemLookupData);
            InitializeStatDataLayer(m_Data.statManagerData);

            m_Version = m_Data.version;

            m_IsInitialized = true;

            //Reset data to loose references to the child objects.
            m_Data = new GameFoundationSerializableData();

            completer.Resolve();
        }

        /// <summary>
        /// Gets all the data from Game Foundation
        /// </summary>
        public GameFoundationSerializableData GetLayerData()
        {
            return GetData();
        }
    }
}
