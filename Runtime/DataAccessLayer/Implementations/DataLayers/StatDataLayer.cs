using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    using StatType = StatDictionarySerializableData.StatType;

    /// <summary>
    /// Straightforward implementation of <see cref="IStatDataLayer"/>.
    /// </summary>
    class StatDataLayer : IStatDataLayer
    {
        /// <summary>
        /// The number of handled stat types.
        /// </summary>
        static readonly int k_StatsTypeCount;

        /// <summary>
        /// Contains all stats for its handled <see cref="StatDictionarySerializableData.StatType"/>.
        /// </summary>
        /// <remarks>
        /// The stats at the i-th index are stats of the type (StatDictionarySerializableData.StatType)i.
        /// </remarks>
        List<StatSerializableData>[] m_Stats;

        static StatDataLayer()
        {
            k_StatsTypeCount = Enum.GetValues(typeof(StatDefinition.StatValueType)).Length;
        }

        /// <summary>
        /// Create a new <see cref="StatDataLayer"/> with the given data.
        /// </summary>
        /// <param name="data">StatManager's serializable data.</param>
        public StatDataLayer(StatManagerSerializableData data)
        {
            m_Stats = new List<StatSerializableData>[k_StatsTypeCount];
            for (var i = 0; i < k_StatsTypeCount; ++i)
            {
                m_Stats[i] = new List<StatSerializableData>();
            }

            UpdateStats(data);
        }

        /// <inheritdoc />
        StatManagerSerializableData IStatDataLayer.GetData()
        {
            var data = new StatManagerSerializableData
            {
                statDictionaries = new StatDictionarySerializableData[m_Stats.Length]
            };

            for (var i = 0; i < m_Stats.Length; ++i)
            {
                data.statDictionaries[i] = new StatDictionarySerializableData
                {
                    statType = (StatType)i,
                    stats = m_Stats[i].ToArray()
                };
            }

            return data;
        }

        /// <inheritdoc />
        void IStatDataLayer.SetStatValue<T>(int gameItemId, string statDefinitionId, T value, T defaultValue, Completer completer)
        {
            var valueType = typeof(T);
            StatType statType;
            if (typeof(int) == valueType)
            {
                statType = StatType.Int;
            }
            else if (typeof(float) == valueType)
            {
                statType = StatType.Float;
            }
            else
            {
                completer.Reject(new ArgumentException($"The type \"{valueType.Name}\" isn't a valid stat type."));

                return;
            }

            var typedStats = m_Stats[(int)statType];

            var isUpdate = false;
            var indexToSet = -1;
            StatSerializableData statToSet = default;

            //Search for the stat to update.
            for (var i = 0; i < typedStats.Count; i++)
            {
                var stat = typedStats[i];
                if (stat.statItem.gameItemId != gameItemId
                    || stat.statItem.definitionId != statDefinitionId)
                {
                    continue;
                }

                indexToSet = i;
                statToSet = stat;

                isUpdate = true;
            }

            //Create the stat since it doesn't exist yet.
            if (!isUpdate)
            {
                statToSet = new StatSerializableData
                {
                    //Note that any generic version works for the get key.
                    statDictionaryId = StatManager.GetGameItemStatKey(gameItemId, statDefinitionId),
                    statItem = new StatItemSerializableData
                    {
                        definitionId = statDefinitionId,
                        gameItemId = gameItemId
                    }
                };

                indexToSet = typedStats.Count;

                typedStats.Add(statToSet);
            }

            //Update the stat. There is no default case since this they would have already been rejected.
            switch (statType)
            {
                case StatType.Int:
                {
                    statToSet.statItem.intValue = Convert.ToInt32(value);
                    statToSet.statItem.defaultIntValue = Convert.ToInt32(defaultValue);

                    break;
                }

                case StatType.Float:
                {
                    statToSet.statItem.floatValue = Convert.ToSingle(value);
                    statToSet.statItem.defaultFloatValue = Convert.ToSingle(defaultValue);

                    break;
                }
            }

            //Don't forget to reassign the value since we are working on structs.
            typedStats[indexToSet] = statToSet;

            completer.Resolve();
        }

        /// <inheritdoc />
        void IStatDataLayer.DeleteStatValue<T>(int gameItemId, string statDefinitionId, Completer completer)
        {
            var valueType = typeof(T);
            StatType statType;
            if (typeof(int) == valueType)
            {
                statType = StatType.Int;
            }
            else if (typeof(float) == valueType)
            {
                statType = StatType.Float;
            }
            else
            {
                completer.Reject(new ArgumentException($"The type \"{valueType.Name}\" isn't a valid stat type."));

                return;
            }

            var typedStats = m_Stats[(int)statType];
            for (var i = 0; i < typedStats.Count; ++i)
            {
                var stat = typedStats[i];
                if (stat.statItem.gameItemId != gameItemId
                    || stat.statItem.definitionId != statDefinitionId)
                {
                    continue;
                }

                typedStats.RemoveAt(i);

                break;
            }

            completer.Resolve();
        }

        void UpdateStats(StatManagerSerializableData data)
        {
            foreach (var typedStats in m_Stats)
            {
                typedStats.Clear();
            }

            if (data.statDictionaries == null)
            {
                return;
            }

            foreach (var statDictionary in data.statDictionaries)
            {
                var statData = m_Stats[(int)statDictionary.statType];
                statData.AddRange(statDictionary.stats);
            }
        }
    }
}
