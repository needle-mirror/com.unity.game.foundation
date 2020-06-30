using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     The builder of all the runtime static data of Game Foundation.
    ///     It is given to the data layer so it can configure the data based on its
    ///     internal data structure, that Game Foundation doesn't need to know.
    /// </summary>
    public class CatalogBuilder
    {
        /// <summary>
        ///     The configurations of the <see cref="Tag" /> instances to build.
        /// </summary>
        readonly Dictionary<string, TagConfig> m_Tags =
            new Dictionary<string, TagConfig>();

        /// <summary>
        ///     The configurations of the <see cref="CatalogItem" /> instances to
        ///     build.
        /// </summary>
        readonly Dictionary<string, CatalogItemConfig> m_Items =
            new Dictionary<string, CatalogItemConfig>();

        /// <summary>
        ///     Gets a <see cref="CatalogItemConfig" /> from its <paramref name="key" />
        ///     or die trying (throws an <see cref="Exception" />).
        /// </summary>
        /// <param name="key">
        ///     The identifier of the <see cref="CatalogItemConfig" /> to find.
        /// </param>
        /// <returns>
        ///     The <see cref="CatalogItemConfig" /> corresponding to the <paramref name="key" /> parameter.
        /// </returns>
        internal CatalogItemConfig GetItemOrDie(string key)
        {
            var found = m_Items.TryGetValue(key, out var item);
            if (!found)
            {
                throw new Exception($"{nameof(CatalogItemConfig)} {key} not found");
            }

            return item;
        }

        /// <summary>
        ///     Adds all the <see cref="CatalogItemConfig" /> instances of the
        ///     given <typeparamref name="TCatalogItem" /> type into the given
        ///     <paramref name="target" /> collection.
        /// </summary>
        /// <typeparam name="TCatalogItem">
        ///     The type of items to find.
        /// </typeparam>
        /// <param name="target">
        ///     The target collection where the items will be added.
        /// </param>
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
        ///     Creates a new <typeparamref name="TCatalogItem" /> instance.
        /// </summary>
        /// <typeparam name="TCatalogItem">
        ///     The type of the item to create.
        /// </typeparam>
        /// <param name="key">
        ///     The identifier of the <typeparamref name="TCatalogItem" /> instance to create.
        /// </param>
        /// <returns>
        ///     The new <typeparamref name="TCatalogItem" /> instance.
        /// </returns>
        public TCatalogItem Create<TCatalogItem>(string key)
            where TCatalogItem : CatalogItemConfig, new()
        {
            var found = m_Items.TryGetValue(key, out var existingItem);
            if (found)
            {
                throw new Exception
                    ($"Another {nameof(CatalogItem)} of type {existingItem.GetType()} and key {key} already exists");
            }

            var item = new TCatalogItem();
            item.key = key;
            m_Items.Add(key, item);
            return item;
        }

        /// <summary>
        ///     Creates a new Tag instance.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the new tag.
        /// </param>
        /// <returns>
        ///     The new Tag instance.
        /// </returns>
        public TagConfig CreateTag(string key)
        {
            var found = m_Tags.TryGetValue(key, out var existingItem);
            if (found)
            {
                throw new Exception(
                    $"Another {nameof(CatalogItem)} of type {existingItem.GetType()} and id {key} already exists");
            }

            var item = new TagConfig();
            item.key = key;
            m_Tags.Add(key, item);
            return item;
        }

        /// <summary>
        ///     Builds and returns the CatalogManager.
        /// </summary>
        /// <returns>
        ///     The new <see cref="CatalogManager" /> instance.
        /// </returns>
        public CatalogManager Build()
        {
            Compile();
            Link();
            var catalogs = BuildCatalogManager();
            return catalogs;
        }

        /// <summary>
        ///     Checks the configurations and build the elements of each configuration objects.
        /// </summary>
        void Compile()
        {
            foreach (var tag in m_Tags.Values)
            {
                tag.Compile();
            }

            foreach (var item in m_Items.Values)
            {
                item.Compile();
            }
        }

        /// <summary>
        ///     Resolves all the possible references each <see cref="CatalogItem" /> may contain.
        /// </summary>
        void Link()
        {
            foreach (var item in m_Items.Values)
            {
                item.Link(this);
            }
        }

        /// <inheritdoc cref="Build" />
        CatalogManager BuildCatalogManager()
        {
            var catalogItems = new List<CatalogItemConfig>();

            var catalogs = new CatalogManager();

            // Tags
            {
                var catalog = new TagCatalog();
                catalogs.tagCatalog = catalog;

                var tags = new Tag[m_Tags.Count];

                var index = 0;
                foreach (var tagConfig in m_Tags.Values)
                {
                    tags[index++] = tagConfig.runtimeTag;
                }

                catalog.m_Tags = tags;
            }

            // Currencies
            {
                var catalog = new CurrencyCatalog();
                catalogs.currencyCatalog = catalog;

                try
                {
                    GetItems<CurrencyConfig>(catalogItems);

                    var currencies = new Dictionary<string, Currency>(catalogItems.Count);
                    catalog.m_Items = currencies;

                    foreach (CurrencyConfig currencyConfig in catalogItems)
                    {
                        var currency = currencyConfig.runtimeItem;
                        currencies.Add(currency.key, currency);
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

                try
                {
                    GetItems<InventoryItemDefinitionConfig>(catalogItems);

                    var items = new Dictionary<string, InventoryItemDefinition>(catalogItems.Count);
                    catalog.m_Items = items;

                    foreach (InventoryItemDefinitionConfig itemConfig in catalogItems)
                    {
                        var item = itemConfig.runtimeItem;
                        items.Add(item.key, item);
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

                try
                {
                    GetItems<BaseTransactionConfig>(catalogItems);

                    var transactions = new Dictionary<string, BaseTransaction>(catalogItems.Count);
                    catalog.m_Items = transactions;

                    foreach (BaseTransactionConfig transactionConfig in catalogItems)
                    {
                        var transaction = transactionConfig.runtimeItem;
                        transactions.Add(transaction.key, transaction);
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

                try
                {
                    GetItems<StoreConfig>(catalogItems);

                    var stores = new Dictionary<string, Store>(catalogItems.Count);
                    catalog.m_Items = stores;

                    foreach (StoreConfig storeConfig in catalogItems)
                    {
                        var store = storeConfig.runtimeItem;
                        stores.Add(store.key, store);
                    }
                }
                finally
                {
                    catalogItems.Clear();
                }
            }

            // Game Parameter
            {
                var catalog = new GameParameterCatalog();
                catalogs.gameParameterCatalog = catalog;

                try
                {
                    GetItems<GameParameterConfig>(catalogItems);

                    var parameters = new Dictionary<string, GameParameter>(catalogItems.Count);
                    catalog.m_Items = parameters;

                    foreach (GameParameterConfig parameterConfig in catalogItems)
                    {
                        var parameter = parameterConfig.runtimeItem;
                        parameters.Add(parameter.key, parameter);
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
