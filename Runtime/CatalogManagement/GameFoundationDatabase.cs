using UnityEngine.GameFoundation.DataPersistence;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// This consolidates all catalogs into one asset.
    /// </summary>
    public partial class GameFoundationDatabase : ScriptableObject, ICatalogConfigurator
    {
        /// <inheritdoc cref="inventoryCatalog"/>
        [SerializeField]
        internal InventoryCatalogAsset m_InventoryCatalog;

        /// <summary>
        /// A reference to an inventory catalog
        /// </summary>
        public InventoryCatalogAsset inventoryCatalog => m_InventoryCatalog;

        /// <inheritdoc cref="storeCatalog"/>
        [SerializeField]
        internal StoreCatalogAsset m_StoreCatalog;

        /// <summary>
        /// A reference to an Store catalog
        /// </summary>
        public StoreCatalogAsset storeCatalog => m_StoreCatalog;

        /// <inheritdoc cref="statCatalog"/>
        [SerializeField]
        internal StatCatalogAsset m_StatCatalog;

        /// <summary>
        /// A reference to a stat catalog
        /// </summary>
        public StatCatalogAsset statCatalog => m_StatCatalog;

        /// <inheritdoc cref="currencyCatalog"/>
        [SerializeField]
        internal CurrencyCatalogAsset m_CurrencyCatalog;

        /// <summary>
        /// A reference to an Currency catalog
        /// </summary>
        public CurrencyCatalogAsset currencyCatalog => m_CurrencyCatalog;

        /// <inheritdoc cref="transactionCatalog"/>
        [SerializeField]
        internal TransactionCatalogAsset m_TransactionCatalog;

        /// <summary>
        /// A reference to an Transaction catalog
        /// </summary>
        public TransactionCatalogAsset transactionCatalog => m_TransactionCatalog;

        /// <summary>
        /// Creates a default data structure for a new player.
        /// </summary>
        /// <returns></returns>
        public GameFoundationSerializableData CreateDefaultData()
        {
            var currencies = m_CurrencyCatalog.m_Items;

            var data = new GameFoundationSerializableData
            {
                inventoryManagerData = new InventoryManagerSerializableData
                {
                    items = new InventoryItemSerializableData[0]
                },
                statManagerData = new StatManagerSerializableData
                {
                    items = new StatItemSerializableData[0]
                },
                walletData = new WalletSerializableData
                {
                    balances = new BalanceSerializableData [currencies.Count]
                }
            };

            for (var i = 0; i < currencies.Count; i++)
            {
                var currency = currencies[i];
                data.walletData.balances[i] = new BalanceSerializableData
                {
                    currencyId = currency.id,
                    balance = currency.initialBalance
                };
            }

            return data;
        }

        /// <summary>
        /// Creates the missing catalog if necessary.
        /// </summary>
        internal void VerifyCatalogs()
        {
            var updated = false;

            if (m_InventoryCatalog is null)
            {
                m_InventoryCatalog = CreateInstance<InventoryCatalogAsset>();
                m_InventoryCatalog.name = "_Catalog_Inventories";
                m_InventoryCatalog.m_Database = this;
                updated = true;
            }

            if (m_StoreCatalog is null)
            {
                m_StoreCatalog = CreateInstance<StoreCatalogAsset>();
                m_StoreCatalog.name = "_Catalog_Stores";
                m_StoreCatalog.m_Database = this;
                m_StoreCatalog.VerifyDefaultCollections();
                updated = true;
            }

            if (m_StatCatalog is null)
            {
                m_StatCatalog = CreateInstance<StatCatalogAsset>();
                m_StatCatalog.name = "_Catalog_Stats";
                m_StatCatalog.m_Database = this;
                updated = true;
            }

            if (m_CurrencyCatalog is null)
            {
                m_CurrencyCatalog = CreateInstance<CurrencyCatalogAsset>();
                m_CurrencyCatalog.name = "_Catalog_Currency";
                m_CurrencyCatalog.m_Database = this;
                updated = true;
            }

            if (m_TransactionCatalog is null)
            {
                m_TransactionCatalog = CreateInstance<TransactionCatalogAsset>();
                m_TransactionCatalog.name = "_Catalog_Transaction";
                m_TransactionCatalog.m_Database = this;
                updated = true;
            }

            if (updated)
            {
#if UNITY_EDITOR
                var path = UnityEditor.AssetDatabase.GetAssetPath(this);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    Editor_Save(path);
                }
#endif
            }

            InitializeCatalogs();
        }

        internal void InitializeCatalogs()
        {
            m_CurrencyCatalog?.Initialize();
            m_InventoryCatalog?.Initialize();
            m_StoreCatalog?.Initialize();
            m_TransactionCatalog?.Initialize();
        }

        /// <summary>
        /// Checks if a category exists among the catalogs.
        /// </summary>
        /// <param name="id">The identifier of the category.</param>
        /// <returns></returns>
        internal bool ContainsCategory(string id)
        {
            return
                (m_CurrencyCatalog?.ContainsCategory(id) ?? false)
                || (m_InventoryCatalog?.ContainsCategory(id) ?? false)
                || (m_StoreCatalog?.ContainsCategory(id) ?? false)
                || (m_TransactionCatalog?.ContainsCategory(id) ?? false)
                ;
        }

        /// <summary>
        /// Verifies the catalogs.
        /// </summary>
        protected void Awake()
        {
            VerifyCatalogs();
        }

        /// <inheritdoc />
        void ICatalogConfigurator.Configure(CatalogBuilder builder)
        {
            m_StatCatalog.Configure(builder);
            m_InventoryCatalog.Configure(builder);
            m_CurrencyCatalog.Configure(builder);
            m_TransactionCatalog.Configure(builder);
            m_StoreCatalog.Configure(builder);
        }
    }
}
