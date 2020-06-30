using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    public abstract partial class BaseMemoryDataLayer
    {
        /// <summary>
        /// The static data of the data layer.
        /// </summary>
        internal GameFoundationDatabase database;

        /// <summary>
        /// Create a data layer using the given <paramref name="database"/> for
        /// static data.
        /// </summary>
        /// <param name="database">The static data provider.</param>
        protected BaseMemoryDataLayer(GameFoundationDatabase database = null)
        {
            if (database is null)
            {
                database = GameFoundationDatabaseSettings.database;
            }

            this.database = database;
        }

        /// <inheritdoc />
        void ICatalogConfigurator.Configure(CatalogBuilder builder)
            => (database as ICatalogConfigurator).Configure(builder);
    }
}
