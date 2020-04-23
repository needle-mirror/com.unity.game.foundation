using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Configs.Details;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// A tuple (stat, stat value)
    /// </summary>
    [Serializable]
    struct StatEntry
    {
        public StatDefinitionAsset stat;
        public StatValue value;
    }

    /// <summary>
    /// Detail defining the mutable properties and their default values for a
    /// <see cref="CatalogItemAsset"/>.
    /// </summary>
    public sealed partial class StatDetailAsset : BaseDetailAsset, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The serialized list of tuple (stat, stat value) 
        /// </summary>
        [SerializeField, HideInInspector]
        StatEntry[] m_StatEntries;

        /// <summary>
        /// The list of stats in their default value.
        /// </summary>
        internal Dictionary<StatDefinitionAsset, StatValue> m_Stats;

        // internal constructor to prohibit developers instantiating StatDetailDefinitions.
        internal StatDetailAsset() { }

        /// <summary>
        /// Gets the stat catalog this detail is stored into.
        /// </summary>
        /// <returns></returns>
        internal StatCatalogAsset GetStatCatalog()
        {
            return itemDefinition.catalog.database.statCatalog;
        }

        /// <summary>
        /// Gets the value of the <see cref="StatDefinitionAsset"/> from its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the stat.</param>
        /// <returns></returns>
        StatValue GetStatValue(string id)
        {
            var catalog = GetStatCatalog();
            var statDefinition = catalog.FindStatDefinition(id);
            if (statDefinition is null)
            {
                throw new ArgumentException($"{id} not found", nameof(id));
            }

            if (m_Stats.TryGetValue(statDefinition, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException("Attempted to get stat default int value for stat that does not exist.");
        }

        /// <summary>
        /// Gets all the <see cref="StatDefinitionAsset"/> instances from this
        /// <see cref="StatDetailAsset"/> instance.
        /// </summary>
        /// <param name="target">The target collection where the
        /// <see cref="StatDefinitionAsset"/> will be added.</param>
        /// <returns>The number of <see cref="StatDefinitionAsset"/>
        /// added.</returns>
        public int GetStatDefinitions(ICollection<StatDefinitionAsset> target = null)
            => GFTools.Copy(m_Stats.Keys, target);

        /// <inheritdoc/>
        public override string DisplayName() => "Stat Detail";

        /// <inheritdoc/>
        public override string TooltipMessage() => "The stat detail allows the attachment of stats with specific default values to the given definition.";

        /// <summary>
        /// Initializes the internal collections if necessary.
        /// </summary>
        protected override void AwakeDetail()
        {
            if (m_StatEntries is null)
            {
                m_StatEntries = new StatEntry[0];
            }

            if (m_Stats is null)
            {
                m_Stats = new Dictionary<StatDefinitionAsset, StatValue>();
            }
        }

        /// <summary>
        /// Gets default value for a <see cref="StatDefinitionAsset"/> by its
        /// <paramref name="statDefinitionId"/>.
        /// </summary>
        /// <param name="statDefinitionId">The identifier of a
        /// <see cref="StatDefinitionAsset"/></param>
        /// <returns>Default value for specified stat.</returns>
        public StatValue GetStat(string statDefinitionId)
        {
            GFTools.ThrowIfArgNullOrEmpty
                (statDefinitionId, nameof(statDefinitionId));

            return GetStatValue(statDefinitionId);
        }

        /// <summary>
        /// Called before serializing to prepare dictionary to be serialized
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_StatEntries = new StatEntry[m_Stats.Count];
            var index = 0;
            foreach (var kvp in m_Stats)
            {
                m_StatEntries[index++] = new StatEntry
                {
                    stat = kvp.Key,
                    value = kvp.Value
                };
            }
        }

        /// <summary>
        /// Called after deserializing to update dictionary from serialized data
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_StatEntries is null)
            {
                m_StatEntries = new StatEntry[0];
            }

            if (m_Stats is null)
            {
                m_Stats = new Dictionary<StatDefinitionAsset, StatValue>();
            }
            else
            {
                m_Stats.Clear();
            }

            foreach (var entry in m_StatEntries)
            {
                m_Stats.Add(entry.stat, entry.value);
            }
        }

        /// <inheritdoc />
        internal override BaseDetailConfig CreateConfig()
        {
            var statDetail = new StatDetailConfig();

            foreach (var kvp in m_Stats)
            {
                var statId = kvp.Key.id;
                StatValue value = kvp.Value;
                statDetail.entries.Add(statId, value);
            }

            return statDetail;
        }
    }
}
