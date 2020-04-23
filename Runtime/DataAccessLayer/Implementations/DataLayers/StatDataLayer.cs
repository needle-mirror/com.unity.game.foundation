using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Straightforward implementation of <see cref="IStatDataLayer"/>.
    /// </summary>
    class StatDataLayer : IStatDataLayer
    {
        /// <summary>
        /// Owner this <see cref="StatDataLayer"/> instance.
        /// </summary>
        BaseMemoryDataLayer m_Owner;

        /// <summary>
        /// Container of all the stats of Game Foundation.
        /// </summary>
        internal Dictionary<string, Dictionary<string, StatValue>> m_Stats =
            new Dictionary<string, Dictionary<string, StatValue>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatDataLayer"/> class
        /// with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="owner">The data layer owning this object.</param>
        /// <param name="data">StatManager's serializable data.</param>
        public StatDataLayer(BaseMemoryDataLayer owner, StatManagerSerializableData data)
        {
            m_Owner = owner;
            UpdateStats(data);
        }

        /// <inheritdoc />
        StatManagerSerializableData IStatDataLayer.GetData()
        {
            var items = new StatItemSerializableData[m_Stats.Count];

            var itemIndex = 0;
            foreach (var kvp in m_Stats)
            {
                var itemData = new StatItemSerializableData
                {
                    itemId = kvp.Key
                };

                var itemStats = kvp.Value;

                itemData.stats = new ItemStat[itemStats.Count];
                var statIndex = 0;
                foreach (var statKvp in itemStats)
                {
                    itemData.stats[statIndex++] = new ItemStat
                    {
                        statId = statKvp.Key,
                        value = statKvp.Value
                    };
                }

                items[itemIndex++] = itemData;
            }

            return new StatManagerSerializableData
            {
                items = items
            };
        }

        /// <inheritdoc />
        StatValue IStatDataLayer.GetStatValue(string gameItemId, string statDefinitionId)
        {
            Tools.ThrowIfArgNullOrEmpty(gameItemId, nameof(gameItemId));
            Tools.ThrowIfArgNullOrEmpty(statDefinitionId, nameof(statDefinitionId));

            m_Stats.TryGetValue(gameItemId, out var itemStats);
            if (itemStats is null)
            {
                throw new StatDefinitionNotFoundException(gameItemId, statDefinitionId);
            }

            var found = itemStats.TryGetValue(statDefinitionId, out var statValue);
            if (!found)
            {
                throw new StatDefinitionNotFoundException(gameItemId, statDefinitionId);
            }

            return statValue;
        }

        /// <inheritdoc />
        void IStatDataLayer.SetStatValue(
            string gameItemId,
            string statDefinitionId,
            StatValue value,
            Completer completer)
        {
            Tools.ThrowIfArgNullOrEmpty(gameItemId, nameof(gameItemId));
            Tools.ThrowIfArgNullOrEmpty(statDefinitionId, nameof(statDefinitionId));

            var inventoryCatalog = m_Owner.database.inventoryCatalog;
            var statCatalog = m_Owner.database.statCatalog;

            var itemFound = m_Owner.m_InventoryDataLayer.m_Items.TryGetValue
                (gameItemId, out var item);

            if (!itemFound)
            {
                var exception = new InventoryItemNotFoundException(gameItemId);
                completer.Reject(exception);
                return;
            }

            var statDefinition = statCatalog.FindStatDefinition(statDefinitionId);
            if (statDefinition is null)
            {
                var exception =
                    new ArgumentException(
                        $"{nameof(StatDefinition)} {statDefinitionId} not found",
                        nameof(statDefinitionId));

                completer.Reject(exception);
                return;
            }

            if (statDefinition.statValueType != value.type)
            {
                var exception = new InvalidCastException
                    ($"Stat {statDefinitionId} is not {value.type}");

                completer.Reject(exception);
                return;
            }

            var itemDefinition = inventoryCatalog.FindItem(item.definitionId);

            var statDetail = itemDefinition.GetDetail<StatDetailAsset>();
            if (statDetail is null)
            {
                var exception = new DetailNotFoundException
                    (gameItemId, typeof(StatDetailAsset));

                completer.Reject(exception);
                return;
            }

            var statFound = statDetail.m_Stats.TryGetValue
                (statDefinition, out var statValue);

            if (!statFound)
            {
                var exception =
                    new StatDefinitionNotFoundException(gameItemId, statDefinitionId);

                completer.Reject(exception);
                return;
            }


            m_Stats.TryGetValue(gameItemId, out var itemStats);

            if (itemStats is null)
            {
                itemStats = new Dictionary<string, StatValue>();
                m_Stats.Add(gameItemId, itemStats);
            }

            itemStats[statDefinitionId] = value;
            completer.Resolve();
        }

        /// <inheritdoc />
        void IStatDataLayer.DeleteStatValue
            (string gameItemId, string statDefinitionId, Completer completer)
        {
            m_Stats.TryGetValue(gameItemId, out var itemStats);
            if (itemStats is null)
            {
                completer.Resolve();
                return;
            }

            itemStats.Remove(statDefinitionId);
            completer.Resolve();
        }

        /// <summary>
        /// Updates all the stats.
        /// Clears the container before populating again.
        /// </summary>
        /// <param name="data">The data of all the stats to add.</param>
        void UpdateStats(StatManagerSerializableData data)
        {
            m_Stats.Clear();

            if (data.items == null)
            {
                return;
            }

            foreach (var item in data.items)
            {
                var itemStats = new Dictionary<string, StatValue>();
                foreach (var stat in item.stats)
                {
                    itemStats.Add(stat.statId, stat.value);
                }
                m_Stats.Add(item.itemId, itemStats);
            }
        }
        /// <summary>
        /// Initializes the stat of the new item.
        /// </summary>
        /// <param name="itemId">Id of the item</param>
        public void InitStats(string itemId)
        {
            var inventoryCatalog = m_Owner.database.inventoryCatalog;

            var found = m_Owner.m_InventoryDataLayer.m_Items.TryGetValue(itemId, out var item);
            if (!found)
            {
                throw new InventoryItemNotFoundException(itemId);
            }

            // Initialize the stats
            var itemDefinition = inventoryCatalog.FindItem(item.definitionId);

            var statDetail = itemDefinition.GetDetail<StatDetailAsset>();

            // Not having stat is possible
            if (statDetail is null) return;

            foreach (var kvp in statDetail.m_Stats)
            {
                var statDefinition = kvp.Key;
                var statValue = kvp.Value;

                (this as IStatDataLayer).SetStatValue
                    (itemId, statDefinition.id, statValue, Completer.None);
            }
        }
    }
}
