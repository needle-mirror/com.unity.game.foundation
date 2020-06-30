using System;
using System.IO;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.DefaultLayers.Persistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    /// Data layer for GameFoundation using an <see cref="IDataPersistence"/> as its data source.
    /// </summary>
    public class PersistenceDataLayer : BaseMemoryDataLayer
    {
        /// <summary>
        /// The persistence object used by this data layer to save & load data.
        /// </summary>
        public IDataPersistence persistence { get; }

        /// <summary>
        /// The generator of the promises of this data layer.
        /// </summary>
        PromiseGenerator m_PromiseGenerator = new PromiseGenerator();

        /// <summary>
        /// Create a data layer with the given catalog provider
        /// that will use the given persistence object to save & load GameFoundation's data.
        /// </summary>
        /// <param name="database">
        /// Provides catalogs to the <see cref="CatalogManager" />.
        /// </param>
        /// <param name="persistence">
        /// Persistence used by this data layer.
        /// </param>
        /// <exception cref="ArgumentNullException">If the given persistence is null.</exception>
        public PersistenceDataLayer(IDataPersistence persistence, GameFoundationDatabase database = null)
            : base(database)
        {
            this.persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        }

        /// <inheritdoc />
        public override void Initialize(Completer completer)
        {
            void InitializeWith(GameFoundationData data)
            {
                InitializeInventoryDataLayer(data.inventoryManagerData, database.inventoryCatalog);
                InitializeWalletDataLayer(data.walletData, database.currencyCatalog);

                m_Version = data.version;

                completer.Resolve();
            }

            persistence.Load(
                InitializeWith,
                error =>
                {
                    switch (error)
                    {
                        case FileNotFoundException _:
                            InitializeWith(database.CreateDefaultData());
                            break;

                        default:
                            completer.Reject(error);
                            break;
                    }
                });
        }

        /// <summary>
        /// Save GameFoundation's data using the persistence object.
        /// </summary>
        /// <returns>
        /// Returns a <see cref="Deferred"/> to track the progression of the save process.
        /// </returns>
        public Deferred Save()
        {
            var data = GetData();

            m_PromiseGenerator.GetPromiseHandles(out var deferred, out var completer);

            persistence.Save(data, completer.Resolve, completer.Reject);

            return deferred;
        }
    }
}
