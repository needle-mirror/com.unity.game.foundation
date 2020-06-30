using System;
using UnityEngine.GameFoundation.Data;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// This consolidates all catalogs into one asset.
    /// </summary>
    public partial class GameFoundationDatabase : ScriptableObject, ICatalogConfigurator
    {
        /// <inheritdoc cref="tagCatalog"/>
        [SerializeField]
        internal TagCatalogAsset m_TagCatalog;

        /// <summary>
        /// A reference to an tag catalog
        /// </summary>
        public TagCatalogAsset tagCatalog => m_TagCatalog;

        /// <inheritdoc cref="inventoryCatalog"/>
        [SerializeField]
        InventoryCatalogAsset m_InventoryCatalog;

        /// <summary>
        /// A reference to an inventory catalog
        /// </summary>
        public InventoryCatalogAsset inventoryCatalog => m_InventoryCatalog;

        /// <inheritdoc cref="storeCatalog"/>
        [SerializeField]
        StoreCatalogAsset m_StoreCatalog;

        /// <summary>
        /// A reference to an Store catalog
        /// </summary>
        public StoreCatalogAsset storeCatalog => m_StoreCatalog;

        /// <inheritdoc cref="currencyCatalog"/>
        [SerializeField]
        CurrencyCatalogAsset m_CurrencyCatalog;

        /// <summary>
        /// A reference to an Currency catalog
        /// </summary>
        public CurrencyCatalogAsset currencyCatalog => m_CurrencyCatalog;

        /// <inheritdoc cref="transactionCatalog"/>
        [SerializeField]
        TransactionCatalogAsset m_TransactionCatalog;

        /// <summary>
        /// A reference to an Transaction catalog
        /// </summary>
        public TransactionCatalogAsset transactionCatalog => m_TransactionCatalog;

        /// <inheritdoc cref="gameParameterCatalog"/>
        [SerializeField]
        GameParameterCatalogAsset m_GameParameterCatalog;

        /// <summary>
        /// A reference to an Game Parameter catalog
        /// </summary>
        public GameParameterCatalogAsset gameParameterCatalog => m_GameParameterCatalog;

        /// <summary>
        /// Creates a default data structure for a new player.
        /// </summary>
        public GameFoundationData CreateDefaultData()
        {
            var currencies = m_CurrencyCatalog.m_Items;

            // count quantity of unique items which requested InitialAllocation
            var initialAllocations = 0;
            var inventories = m_InventoryCatalog.m_Items;
            foreach (var inventory in inventories)
            {
                initialAllocations += inventory.initialAllocation;
            }

            // setup GameFoundationData with correct sizes of 
            var data = new GameFoundationData
            {
                inventoryManagerData = new InventoryManagerData
                {
                    items = new InventoryItemData[initialAllocations]
                },
                walletData = new WalletData
                {
                    balances = new BalanceData[currencies.Count]
                }
            };

            // add all inventory item initial allocations
            var inventoryManagerDataOn = 0;
            foreach (var inventory in inventories)
            {
                for (int i = 0; i < inventory.initialAllocation; ++ i)
                {
                    data.inventoryManagerData.items[inventoryManagerDataOn] = new InventoryItemData
                    {
                        definitionKey = inventory.key,
                        id = Guid.NewGuid().ToString()
                    };

                    ++inventoryManagerDataOn;
                }
            }

            // add all currency initial balances
            for (var i = 0; i < currencies.Count; i++)
            {
                var currency = currencies[i];
                data.walletData.balances[i] = new BalanceData
                {
                    currencyKey = currency.key,
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

            if (m_TagCatalog is null)
            {
                m_TagCatalog = CreateInstance<TagCatalogAsset>();
                m_TagCatalog.name = "_Catalog_Tags";
                m_TagCatalog.m_Database = this;
                updated = true;
            }

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

            if (m_GameParameterCatalog is null)
            {
                m_GameParameterCatalog = CreateInstance<GameParameterCatalogAsset>();
                m_GameParameterCatalog.name = "_Catalog_GameParameter";
                m_GameParameterCatalog.m_Database = this;
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
            m_GameParameterCatalog?.Initialize();
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
            m_TagCatalog.Configure(builder);
            m_InventoryCatalog.Configure(builder);
            m_CurrencyCatalog.Configure(builder);
            m_TransactionCatalog.Configure(builder);
            m_StoreCatalog.Configure(builder);
            m_GameParameterCatalog.Configure(builder);
        }
    }
}
