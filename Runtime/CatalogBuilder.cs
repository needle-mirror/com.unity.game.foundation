using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// The builder of all the runtime static data of Game Foundation.
    /// It is given to the data layer so it can configure the data based on its
    /// internal data structure, that Game Foundation doesn't need to know.
    /// </summary>
    public class CatalogBuilder
    {
        /// <summary>
        /// The configurations of the <see cref="Category"/> instances to build.
        /// </summary>
        readonly Dictionary<string, CategoryConfig> m_Categories =
            new Dictionary<string, CategoryConfig>();

        /// <summary>
        /// The configurations of the <see cref="CatalogItem"/> instances to
        /// build.
        /// </summary>
        readonly Dictionary<string, CatalogItemConfig> m_Items =
            new Dictionary<string, CatalogItemConfig>();

        /// <summary>
        /// The configurations of the <see cref="StatDefinition"/> instances to
        /// build.
        /// </summary>
        readonly Dictionary<string, StatDefinitionConfig> m_Stats =
            new Dictionary<string, StatDefinitionConfig>();


        /// <summary>
        /// Gets a <see cref="CatalogItemConfig"/> from its <paramref name="id"/>
        /// or die trying (throws an <see cref="Exception"/>).
        /// </summary>
        /// <param name="id">The identifier of the
        /// <see cref="CatalogItemConfig"/> to find.</param>
        /// <returns>The <see cref="CatalogItemConfig"/> corresponding to the
        /// <paramref name="id"/> parameter.</returns>
        internal CatalogItemConfig GetItemOrDie(string id)
        {
            var found = m_Items.TryGetValue(id, out var item);
            if (!found)
            {
                throw new Exception
                    ($"{nameof(CatalogItemConfig)} {id} not found");
            }

            return item;
        }

        /// <summary>
        /// Adds all the <see cref="CatalogItemConfig"/> instances of the
        /// given <typeparamref name="TCatalogItem"/> type into the given
        /// <paramref name="target"/> collection.
        /// </summary>
        /// <typeparam name="TCatalogItem">The type of items to find.</typeparam>
        /// <param name="target">The target collection where the items will be
        /// added</param>
        void GetItems<TCatalogItem>(List<CatalogItemConfig> target)
            where TCatalogItem : CatalogItemConfig
        {
            foreach (var item in m_Items.Values)
            {
                if (item is TCatalogItem catalogItem)
                {
                    target.Add(catalogItem);
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="CategoryConfig"/> instance by ids
        /// <paramref name="id"/> or die trying (throws an
        /// <see cref="Exception"/>).
        /// </summary>
        /// <param name="id">The identifier of the <see cref="CategoryConfig"/>
        /// to find</param>
        /// <returns>The <see cref="CategoryConfig"/> corresponding to the
        /// <paramref name="id"/></returns>
        internal CategoryConfig GetCategoryOrDie(string id)
        {
            var found = m_Categories.TryGetValue(id, out var category);
            if (!found)
            {
                throw new Exception
                    ($"{nameof(CategoryConfig)} {id} not found");
            }

            return category;
        }

        /// <summary>
        /// Gets a <see cref="StatDefinitionConfig"/> isntance by its
        /// <paramref name="id"/> or die trying (throws an
        /// <see cref="Exception"/>).
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public StatDefinitionConfig GetStatOrDie(string id)
        {
            var found = m_Stats.TryGetValue(id, out var stat);
            if (!found)
            {
                throw new Exception($"Stat {id} not found");
            }
            return stat;
        }

        /// <summary>
        /// Creates a new <see cref="CategoryConfig"/> instance.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="CategoryConfig"/>
        /// instance to create.</param>
        /// <returns>The new <see cref="CategoryConfig"/> instance.</returns>
        public CategoryConfig CreateCategory(string id)
        {
            if (m_Categories.ContainsKey(id))
            {
                throw new Exception
                    ($"A {nameof(CategoryConfig)} with id {id} already exists");
            }

            var category = new CategoryConfig();
            category.id = id;
            m_Categories.Add(id, category);

            return category;
        }

        /// <summary>
        /// Creates a new <typeparamref name="TCatalogItem"/> instance.
        /// </summary>
        /// <typeparam name="TCatalogItem">The type of the item to
        /// create.</typeparam>
        /// <param name="id">The identifier of the
        /// <typeparamref name="TCatalogItem"/> instance to create.</param>
        /// <returns>The new <typeparamref name="TCatalogItem"/>
        /// instance.</returns>
        public TCatalogItem Create<TCatalogItem>(string id)
            where TCatalogItem : CatalogItemConfig, new()
        {
            var found = m_Items.TryGetValue(id, out var existingItem);
            if (found)
            {
                throw new Exception
                    ($"Another {nameof(CatalogItem)} of type {existingItem.GetType()} and id {id} already exists");
            }

            var item = new TCatalogItem();
            item.id = id;
            m_Items.Add(id, item);
            return item;
        }

        /// <summary>
        /// Creates a new <see cref="StatDefinitionConfig"/> instance.
        /// </summary>
        /// <param name="id">The identifier of the
        /// <see cref="StatDefinitionConfig"/> to create.</param>
        /// <returns>The new <see cref="StatDefinitionConfig"/>
        /// instance.</returns>
        public StatDefinitionConfig CreateStat(string id)
        {
            if (m_Stats.ContainsKey(id))
            {
                throw new Exception
                    ($"{nameof(StatDefinitionConfig)} {id} not found");
            }

            var config = new StatDefinitionConfig();
            config.id = id;
            m_Stats.Add(config.id, config);

            return config;
        }

        /// <summary>
        /// Builds and returns the CatalogManager.
        /// </summary>
        /// <returns>The new <see cref="CatalogManager"/> instance.</returns>
        public CatalogManager Build()
        {
            Compile();
            Link();
            var catalogs = BuildCatalogManager();
            return catalogs;
        }

        /// <summary>
        /// Checks the configurations and build the elements of each
        /// configuration objects.
        /// </summary>
        void Compile()
        {
            foreach (var category in m_Categories.Values)
            {
                category.Compile();
            }

            foreach (var item in m_Items.Values)
            {
                item.Compile();
            }

            foreach (var stat in m_Stats.Values)
            {
                stat.Compile();
            }
        }

        /// <summary>
        /// Resolves all the possible references each <see cref="CatalogItem"/>
        /// may contain.
        /// </summary>
        void Link()
        {
            foreach (var item in m_Items.Values)
            {
                item.Link(this);
            }
        }

        /// <inheritdoc cref="Build"/>
        CatalogManager BuildCatalogManager()
        {
            var catalogItems = new List<CatalogItemConfig>();

            var categories = new Category[m_Categories.Count];

            // Categories
            {
                var index = 0;
                foreach (var categoryConfig in m_Categories.Values)
                {
                    categories[index++] = categoryConfig.runtimeCategory;
                }
            }

            var catalogs = new CatalogManager();

            // Stats
            {
                var catalog = new StatCatalog();
                catalogs.statCatalog = catalog;

                var statDefinitions = new Dictionary<string, StatDefinition>(m_Stats.Count);
                catalog.m_StatDefinitions = statDefinitions;

                foreach (var statDefinitionConfig in m_Stats.Values)
                {
                    var statDefinition = statDefinitionConfig.runtimeItem;
                    statDefinitions.Add(statDefinition.id, statDefinition);
                }
            }

            // Currencies
            {
                var catalog = new CurrencyCatalog();
                catalogs.currencyCatalog = catalog;

                catalog.m_Categories = categories;

                try
                {
                    GetItems<CurrencyConfig>(catalogItems);

                    var currencies = new Dictionary<string, Currency>(catalogItems.Count);
                    catalog.m_Items = currencies;

                    foreach (CurrencyConfig currencyConfig in catalogItems)
                    {
                        var currency = currencyConfig.runtimeItem as Currency;
                        currencies.Add(currency.id, currency);
                    }
                }
                finally
                {
                    catalogItems.Clear();
                }
            }

            // Items
            {
                var catalog = new InventoryCatalog();
                catalogs.inventoryCatalog = catalog;

                catalog.m_Categories = categories;

                try
                {
                    GetItems<InventoryItemDefinitionConfig>(catalogItems);

                    var items = new Dictionary<string, InventoryItemDefinition>(catalogItems.Count);
                    catalog.m_Items = items;

                    foreach (InventoryItemDefinitionConfig itemConfig in catalogItems)
                    {
                        var item = itemConfig.runtimeItem as InventoryItemDefinition;
                        items.Add(item.id, item);
                    }
                }
                finally
                {
                    catalogItems.Clear();
                }
            }

            // Transactions
            {
                var catalog = new TransactionCatalog();
                catalogs.transactionCatalog = catalog;

                catalog.m_Categories = categories;

                try
                {
                    GetItems<BaseTransactionConfig>(catalogItems);

                    var transactions = new Dictionary<string, BaseTransaction>(catalogItems.Count);
                    catalog.m_Items = transactions;

                    foreach (BaseTransactionConfig transactionConfig in catalogItems)
                    {
                        var transaction = transactionConfig.runtimeItem as BaseTransaction;
                        transactions.Add(transaction.id, transaction);
                    }
                }
                finally
                {
                    catalogItems.Clear();
                }
            }

            // Stores
            {
                var catalog = new StoreCatalog();
                catalogs.storeCatalog = catalog;

                catalog.m_Categories = categories;

                try
                {
                    GetItems<StoreConfig>(catalogItems);

                    var stores = new Dictionary<string, Store>(catalogItems.Count);
                    catalog.m_Items = stores;

                    foreach (StoreConfig storeConfig in catalogItems)
                    {
                        var store = storeConfig.runtimeItem as Store;
                        stores.Add(store.id, store);
                    }
                }
                finally
                {
                    catalogItems.Clear();
                }
            }

            return catalogs;
        }
    }
}
