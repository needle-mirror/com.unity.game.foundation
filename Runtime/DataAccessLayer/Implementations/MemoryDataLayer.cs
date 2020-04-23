using System;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Straightforward synchronous data layer that keep data in memory for the session only.
    /// </summary>
    public class MemoryDataLayer : BaseMemoryDataLayer
    {
        bool m_IsInitialized;

        /// <summary>
        /// The serialized data manipulated by this instance.
        /// </summary>
        GameFoundationSerializableData m_Data;

        /// <summary>
        /// Creates a data layer with no player data.
        /// </summary>
        /// <param name="database">Provides catalogs to the
        /// <see cref="CatalogManager" />.</param>
        public MemoryDataLayer(GameFoundationDatabase database = null)
            : base(database)
        {
            m_Data = this.database.CreateDefaultData();
        }

        /// <summary>
        /// Create a data layer with the given catalog provider that will handle
        /// the given data for the current game session only.
        /// </summary>
        /// <param name="database">
        /// Provides catalogs to the <see cref="CatalogManager" />.
        /// </param>
        /// <param name="data">GameFoundation's serializable data.</param>
        /// <exception cref="ArgumentNullException">
        /// If the given data contains invalid null values.
        /// </exception>
        public MemoryDataLayer(GameFoundationSerializableData data, GameFoundationDatabase database = null)
            : base(database)
        {
            if (data.inventoryManagerData.items == null)
                throw new ArgumentNullException(
                    $"{nameof(InventoryManagerSerializableData)}'s {nameof(InventoryManagerSerializableData.items)} mustn't be null.",
                    new NullReferenceException());

            if (data.statManagerData.items == null)
                throw new ArgumentNullException(
                    $"{nameof(StatManagerSerializableData)}'s {nameof(StatManagerSerializableData.items)} mustn't be null.",
                    new NullReferenceException());

            if (data.walletData.balances == null)
                throw new ArgumentNullException(
                    $"{nameof(WalletSerializableData)}'s {nameof(WalletSerializableData.balances)} mustn't be null.",
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
            InitializeStatDataLayer(m_Data.statManagerData);
            InitializeWalletDataLayer(m_Data.walletData, database.currencyCatalog);

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
