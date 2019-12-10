using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DataPersistence;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Track of all runtime stats data and exposes methods for managing the data at runtime.
    /// </summary>
    public static class StatManager
    {
        /// <summary>
        // The StatDictionary class holds a dictionary of stat values by generic type T, along with a few
        // helper methods that are called by StatManager to manipulate/examine the dictionary.
        /// </summary>
        internal static class StatDictionary<T> where T : new()
        {
            // Note: long is gameItem.gameItemId << 32 | statDefHashId
            //       gggggggg gggggggg gggggggg gggggggg  ssssssss ssssssss ssssssss ssssssss
            // where 'g' represents gameItem.gameItemId
            //  and  's' represents the stat definition Hash
            static Dictionary<long, StatItem<T>> m_StatItems = null;

            // list of all stats for each game item by gameItem.gameItemId
            // note: needed to remove all stats when game item is removed from an inventory and can be used to find all stats for a particular game item
            static Dictionary<int, List<int>> m_GameItemStatList = null;

            internal static Dictionary<long, StatItem<T>> statItems
            {
                get => m_StatItems;
            }

            internal static Dictionary<int, List<int>> gameItemStatList
            {
                get => m_GameItemStatList;
            }

            /// <summary>
            // Initialize the dictionary for this generic type.  Should only be called once.
            /// </summary>
            public static void Initialize()
            {
                if (IsInitialized)
                {
                    Debug.LogWarning("StatManager.Initialize was called more than once.");
                    return;
                }
                m_StatItems = new Dictionary<long, StatItem<T>>();
                m_GameItemStatList = new Dictionary<int, List<int>>();
            }

            internal static void Unintialize()
            {
                if (!IsInitialized)
                    return;

                m_StatItems = null;
                m_GameItemStatList = null;
            }

            /// <summary>
            // Determine if this stat dictionary has been initialized (i.e. Initialize() has been called).
            /// </summary>
            /// <returns>True if initialized</returns>
            public static bool IsInitialized
            {
                get
                {
                    return !ReferenceEquals(m_StatItems, null);
                }
            }

            /// <summary>
            // Reset this StatDictionary by re-initializing its dictionary.
            /// </summary>
            public static void Reset()
            {
                ThrowIfNotInitialized();

                m_StatItems = new Dictionary<long, StatItem<T>>();
                m_GameItemStatList = new Dictionary<int, List<int>>();
            }

            /// <summary>
            // Simply throws exception if Initialize() has not been called.
            /// </summary>
            private static void ThrowIfNotInitialized()
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Error: GameFoundation.Initialize() MUST be called before the StatManager is used.");
                }
            }

            /// <summary>
            // Get the key for specified game item and stat definition Hash
            /// </summary>
            /// <param name="gameItem">Game item to get key for.</param>
            /// <param name="statDefinitionHash">StatDefinition Hash to get key for.</param>
            /// <returns>Key to be used in dictionary to find specified GameItem's Stat.</returns>
            internal static long GetKey(GameItem gameItem, int statDefinitionHash)
            {
                ThrowIfNotInitialized();

                if (ReferenceEquals(gameItem, null))
                {
                    return 0;
                }
                return (((long)gameItem.gameItemId) << 32) | (((long)statDefinitionHash) & 0xffffffff);
            }

            /// <summary>
            // Get the Stat item for specified game item and Stat definition Hash
            /// </summary>
            /// <param name="gameItem">GameItem to get StatItem for.</param>
            /// <param name="statDefinitionHash">StatDefinition Hash to get key for.</param>
            /// <returns>StatItem which holds current value of the specified GameItem's Stat.</returns>
            public static StatItem<T> GetStatItem(GameItem gameItem, int statDefinitionHash)
            {
                ThrowIfNotInitialized();

                if (ReferenceEquals(gameItem, null))
                {
                    return null;
                }

                var key = GetKey(gameItem, statDefinitionHash);

                StatItem<T> statItem;
                if (m_StatItems.TryGetValue(key, out statItem))
                {
                    return statItem;
                }

                return null;
            }

            /// <summary>
            // Returns true if the specified Stat exists by game item and Stat definition Hash.
            /// </summary>
            /// <param name="gameItem">GameItem to check if value exists for.</param>
            /// <param name="statDefinitionHash">StatDefinition Hash to check if value exists for.</param>
            /// <returns>True if the specified GameItem ahs the requested StatDefinition set.</returns>
            public static bool HasValue(GameItem gameItem, int statDefinitionHash)
            {
                ThrowIfNotInitialized();

                return !ReferenceEquals(GetStatItem(gameItem, statDefinitionHash), null);
            }

            /// <summary>
            // Get the current Stat value for specified game item and Stat definition Hash
            /// </summary>
            /// <param name="gameItem">GameItem to get current value of Stat for.</param>
            /// <param name="statDefinitionHash">StatDefintion Hash to get current value of Stat for.</param>
            /// <returns>The current value of the specified Stat.</returns>
            public static T GetValue(GameItem gameItem, int statDefinitionHash)
            {
                ThrowIfNotInitialized();

                var statItem = GetStatItem(gameItem, statDefinitionHash);
                if (ReferenceEquals(statItem, null))
                {
                    throw new KeyNotFoundException("No stat found for game item.");
                }
                return statItem.value;
            }

            /// <summary>
            // Get the specified Stat's value, returns true if found and successfully returned, else false
            /// </summary>
            /// <param name="gameItem">GameItem to try to get Stat value for.</param>
            /// <param name="statDefinitionHash">StatDefinition Hash to try to get Stat value for.</param>
            /// <param name="value">return value for specified Stat, if it exists</param>
            /// <returns>True if Stat value was found and returned.</returns>
            public static bool TryGetValue(GameItem gameItem, int statDefinitionHash, out T value)
            {
                ThrowIfNotInitialized();

                var statItem = GetStatItem(gameItem, statDefinitionHash);
                if (!ReferenceEquals(statItem, null))
                {
                    value = statItem.value;
                    return true;
                }
                value = new T();
                return false;
            }

            /// <summary>
            /// Sets specified Stat on specified gameItem to a specific value
            /// </summary>
            /// <param name="gameItem">GameItem to set Stat value for.</param>
            /// <param name="statDefinitionHash">StatDefinition Hash to set Stat value for.</param>
            /// <param name="value">New value for Stat.</param>
            public static void SetValue(GameItem gameItem, int statDefinitionHash, T value)
            {
                ThrowIfNotInitialized();

                if (ReferenceEquals(gameItem, null))
                {
                    throw new ArgumentNullException("The GameItem passed in was null.");
                }

                var key = GetKey(gameItem, statDefinitionHash);

                StatItem<T> statItem;
                if (!m_StatItems.TryGetValue(key, out statItem))
                {
                    var statDefinition = GameFoundationSettings.database.statCatalog.GetStatDefinition(statDefinitionHash);
                    if (ReferenceEquals(statDefinition, null))
                    {
                        throw new NullReferenceException("Attempted to set stat for hash that does not exist in stat catalog.");
                    }
                    if (!statDefinition.DoesValueTypeMatch(typeof(T)))
                    {
                        throw new System.InvalidOperationException($"Stat value type does not match. Expected {statDefinition.statValueType} but received {typeof(T)}");
                    }

                    var gameItemId = gameItem.gameItemId;
                    statItem = new StatItem<T>(gameItemId, statDefinition.id, value);
                    m_StatItems[key] = statItem;

                    // add stat to list of stats FOR THE GAME ITEM
                    List<int> statList;
                    if (!m_GameItemStatList.TryGetValue(gameItemId, out statList))
                    {
                        // if no list exists FOR THE GAME ITEM (i.e. this is the first stat ever for this game item)
                        m_GameItemStatList[gameItemId] = new List<int>() { statDefinitionHash }; 
                    }

                    // if some stats already exist for this game item then add the new stat to the list
                    else
                    {
                        if (!statList.Contains(statDefinitionHash))
                        {
                            statList.Add(statDefinitionHash);
                        }
                    }
                }

                // save value for the stat
                statItem.value = value;
            }

            /// <summary>
            /// Remove specified Stat value for specified GameItem and StatDefinition Hash.
            /// </summary>
            /// <param name="gameItem">GameItem to remove Stat for.</param>
            /// <param name="statDefinitionHash">StatDefinition Hash to remove Stat for.</param>
            /// <returns>True if specified Stat was found and removed, else False.</returns>
            public static bool RemoveValue(GameItem gameItem, int statDefinitionHash)
            {
                ThrowIfNotInitialized();

                if (ReferenceEquals(gameItem, null))
                {
                    return false;
                }

                var key = GetKey(gameItem, statDefinitionHash);
                if (!m_StatItems.Remove(key))
                {
                    return false;
                }

                // stat was removed--remove it also from the list of stats FOR THE GAME ITEM
                var statList = m_GameItemStatList[gameItem.gameItemId];

                // if removing the last stat from the list
                if (statList.Count == 1)
                {
                    Debug.Assert(statList[0] == statDefinitionHash, "The final stat must be the stat being removed.");

                    // remove the entire list since it's now empty
                    m_GameItemStatList.Remove(gameItem.gameItemId);
                }

                // if NOT the last stat then find it and remove only the 1 stat
                else
                {
                    Debug.Assert(statList.Count > 1, "There should NEVER be 0 items in stats list or the stat would have already been removed.");

                    // remove the specified stat from the list of stats for the specified game item
                    statList.Remove(statDefinitionHash);
                }

                return true;
            }

            /// <summary>
            /// Reset stat to the correct default value.
            /// </summary>
            /// <param name="gameItem">GameItem to set Stat default value for.</param>
            /// <param name="statDefinitionHash">StatDefinition Hash to reset Stat value for.</param>
            /// <returns>True if Stat was reset, else false.</returns>
            public static bool ResetToDefaultValue(GameItem gameItem, int statDefinitionHash)
            {
                ThrowIfNotInitialized();

                var stat = GetStatItem(gameItem, statDefinitionHash);
                if (!ReferenceEquals(stat, null))
                {
                    stat.value = stat.defaultValue;
                    return true;
                }

                return false;
            }

            internal static StatSerializableData[] GetSerializableStatsData(StatDictionarySerializableData.StatType type)
            {
                List<StatSerializableData> serializableStats = new List<StatSerializableData>();
                
                foreach (var statItem in m_StatItems)
                {
                    StatItemSerializableData item = null;
                    switch (type)
                    {
                        case StatDictionarySerializableData.StatType.Int:
                            item = new StatItemSerializableData(
                                type,
                                statItem.Value.gameItemId,
                                statItem.Value.definitionId,
                                (int)(object) statItem.Value.value,
                                (int)(object) statItem.Value.defaultValue);
                            break;
                        
                        case StatDictionarySerializableData.StatType.Float:
                            item = new StatItemSerializableData(
                                type,
                                statItem.Value.gameItemId,
                                statItem.Value.definitionId,
                                (float)(object) statItem.Value.value,
                                (float)(object) statItem.Value.defaultValue);
                            break;
                        
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }

                    if (item != null)
                    {
                        StatSerializableData stat = new StatSerializableData(statItem.Key, item);
                        serializableStats.Add(stat);
                    }
                }

                return serializableStats.ToArray();
            }
        }
        
        internal static bool Initialize(ISerializableData data = null)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("StatManager is already initialized and cannot be initialized again.");
                return false;
            }
            
            StatDictionary<int>.Initialize();
            StatDictionary<float>.Initialize();

            m_IsInitialized = true;

            if (data != null)
            {
                m_IsInitialized = FillFromStatsData(data);
            }

            return m_IsInitialized;
        }

        internal static void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            StatDictionary<int>.Unintialize();
            StatDictionary<float>.Unintialize();

            m_IsInitialized = false;
        }

        /// <summary>
        /// Returns the current initialization state of the StatManager.
        /// </summary>
        /// <returns>The current initialization State of the StatManager.</returns>
        public static bool IsInitialized
        {
            get { return m_IsInitialized; }
        }
        private static bool m_IsInitialized = false;

        /// <summary>
        /// This is the StatCatalog the StatManager uses.
        /// </summary>
        /// <returns>The StatCatalog the StatManager uses.</returns>
        public static StatCatalog catalog
        {
            get { return GameFoundationSettings.database.statCatalog; }
        }

        /// <summary>
        /// Can be called after Initialize() as many times as needed.
        /// Will reset everything to be as it was after Initialize() was called.
        /// </summary>
        public static void Reset()
        {
            StatDictionary<int>.Reset();
            StatDictionary<float>.Reset();
        }

        internal static bool FillFromStatsData(ISerializableData data)
        {
            var statManagerData = ((GameFoundationSerializableData) data)?.statManagerData;
            if (statManagerData == null)
            {
                Debug.LogWarning("Persistence Data data doesn't contain Stats.");
                return false;
            }

            Reset();
            
            foreach (var statDictionary in statManagerData.statDictionaries)
            {
                switch (statDictionary.statType)
                {
                    case StatDictionarySerializableData.StatType.Int:

                        var intStats = StatDictionary<int>.statItems;
                        var gameItemIntStatList = StatDictionary<int>.gameItemStatList;

                        for (int i = 0; i < statDictionary.stats.Length; i++)
                        {
                            var stat = statDictionary.stats[i];
                            StatItem<int> item = new StatItem<int>(
                                stat.statItem.gameItemId,
                                stat.statItem.definitionId,
                                stat.statItem.intValue,
                                stat.statItem.defaultIntValue);
                            
                            intStats[stat.statDictionaryId] = item;

                            AddStatToGameItemStatList(gameItemIntStatList, stat.statItem.gameItemId, stat.statDictionaryId);
                        }

                        break;
                    
                    case StatDictionarySerializableData.StatType.Float:
                        
                        var floatStats = StatDictionary<float>.statItems;
                        var gameItemFloatStatList = StatDictionary<float>.gameItemStatList;

                        for (int i = 0; i < statDictionary.stats.Length; i++)
                        {
                            var stat = statDictionary.stats[i];
                            StatItem<float> item = new StatItem<float>(
                                stat.statItem.gameItemId,
                                stat.statItem.definitionId,
                                stat.statItem.floatValue,
                                stat.statItem.defaultFloatValue);

                            floatStats[stat.statDictionaryId] = item;

                            AddStatToGameItemStatList(gameItemFloatStatList, stat.statItem.gameItemId, stat.statDictionaryId);
                        }
                        
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }

        private static void AddStatToGameItemStatList(Dictionary<int, List<int>> gameItemStatList, int gameItemId, long statDictionaryId)
        {
            int statHash = (int)(statDictionaryId & 0xffffffff);

            List<int> statList;
            if (!gameItemStatList.TryGetValue(gameItemId, out statList))
            {
                gameItemStatList[gameItemId] = new List<int>() { statHash };
            }
            else
            {
                statList.Add(statHash);
            }
        }

        // called by game item destructor to clean up stats for the game item
        internal static void OnGameItemDiscarded(GameItem gameItem)
        {
            // since inventory manager can be used without a stat manager, we should only ...
            // ... remove stats if the stat manager is actually active
            if (IsInitialized)
            {
                RemoveGameItemStats<int>(gameItem);
                RemoveGameItemStats<float>(gameItem);
            }
        }

        // private helper method to remove all stats for game item of a specific type (int or float)
        static void RemoveGameItemStats<T>(GameItem gameItem) where T: new()
        {
            var statItems = StatDictionary<T>.statItems;
            var gameItemStatList = StatDictionary<T>.gameItemStatList;

            List<int> statList;
            if (gameItemStatList.TryGetValue(gameItem.gameItemId, out statList))
            {
                foreach (int statHash in statList)
                {
                    var key = StatDictionary<T>.GetKey(gameItem, statHash);
                    statItems.Remove(key);
                }

                gameItemStatList.Remove(gameItem.gameItemId);
            }
        }

        internal static StatManagerSerializableData GetSerializableData()
        {
            var statDictionaries = new StatDictionarySerializableData[2]
            {
                new StatDictionarySerializableData(
                    StatDictionarySerializableData.StatType.Int,
                    StatDictionary<int>.GetSerializableStatsData(StatDictionarySerializableData.StatType.Int)),
                new StatDictionarySerializableData(
                    StatDictionarySerializableData.StatType.Float,
                    StatDictionary<float>.GetSerializableStatsData(StatDictionarySerializableData.StatType.Float))
            };
            
            var statManagerData = new StatManagerSerializableData(statDictionaries);
            return statManagerData;
        }

        // ***********************
        // IMPLEMENTATION FOR INTs
        // ***********************

        /// <summary>
        /// Checks if the specified Stat has been set for specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        public static bool HasIntValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<int>.HasValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Checks if the specified Stat has been set for specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        public static bool HasIntValue(GameItem gameItem, int statDefinitionHash)
        {
            return StatDictionary<int>.HasValue(gameItem, statDefinitionHash);
        }

        /// <summary>
        /// Get's the value of the specified Stat on specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <returns>Current Stat value.</returns>
        public static int GetIntValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<int>.GetValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get's the value of the specified Stat on specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <returns>Current Stat value.</returns>
        public static int GetIntValue(GameItem gameItem, int statDefinitionHash)
        {
            return StatDictionary<int>.GetValue(gameItem, statDefinitionHash);
        }

        /// <summary>
        /// Tries to get specified Stat for specified GameItem and set out variable, if found.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <param name="value">Current value of Stat, if found for this GameItem.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else False.</returns>
        public static bool TryGetIntValue(GameItem gameItem, string statDefinitionId, out int value)
        {
            return StatDictionary<int>.TryGetValue(gameItem, Tools.StringToHash(statDefinitionId), out value);
        }

        /// <summary>
        /// Tries to get specified Stat for specified GameItem and set out variable, if found.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <param name="value">Current value of Stat, if found for this GameItem.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else False.</returns>
        public static bool TryGetIntValue(GameItem gameItem, int statDefinitionHash, out int value)
        {
            return StatDictionary<int>.TryGetValue(gameItem, statDefinitionHash, out value);
        }

        /// <summary>
        /// Sets specified Stat on specified gameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        public static void SetIntValue(GameItem gameItem, string statDefinitionId, int value)
        {
            StatDictionary<int>.SetValue(gameItem, Tools.StringToHash(statDefinitionId), value);
        }

        /// <summary>
        /// Sets specified Stat on specified gameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        public static void SetIntValue(GameItem gameItem, int statDefinitionHash, int value)
        {
            StatDictionary<int>.SetValue(gameItem, statDefinitionHash, value);
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <returns>True if Stat found and removed, else false</returns>
        public static bool RemoveIntValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<int>.RemoveValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <returns>True if Stat found and removed, else false</returns>
        public static bool RemoveIntValue(GameItem gameItem, int statDefinitionHash)
        {
            return StatDictionary<int>.RemoveValue(gameItem, statDefinitionHash);
        }

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <returns>True if specified Stat was found and reset, else False.</returns>
        public static bool ResetToDefaultIntValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<int>.ResetToDefaultValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <returns>True if specified Stat was found and reset, else False.</returns>
        public static bool ResetToDefaultIntValue(GameItem gameItem, int statDefinitionHash)
        {
            return StatDictionary<int>.ResetToDefaultValue(gameItem, statDefinitionHash);
        }


        // *************************
        // IMPLEMENTATION FOR FLOATs
        // *************************

        /// <summary>
        /// Checks if the specified Stat has been set for specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        public static bool HasFloatValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<float>.HasValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Checks if the specified Stat has been set for specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        public static bool HasFloatValue(GameItem gameItem, int statDefinitionHash)
        {
            return StatDictionary<float>.HasValue(gameItem, statDefinitionHash);
        }

        /// <summary>
        /// Get's the value of the specified Stat on specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <returns>Current Stat value.</returns>
        public static float GetFloatValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<float>.GetValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get's the value of the specified Stat on specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <returns>Current Stat value.</returns>
        public static float GetFloatValue(GameItem gameItem, int statDefinitionHash)
        {
            return StatDictionary<float>.GetValue(gameItem, statDefinitionHash);
        }

        /// <summary>
        /// Tries to get specified Stat for specified GameItem and set out variable, if found.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <param name="value">Current value of Stat, if found for this GameItem.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else False.</returns>
        public static bool TryGetFloatValue(GameItem gameItem, string statDefinitionId, out float value)
        {
            return StatDictionary<float>.TryGetValue(gameItem, Tools.StringToHash(statDefinitionId), out value);
        }

        /// <summary>
        /// Tries to get specified Stat for specified GameItem and set out variable, if found.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <param name="value">Current value of Stat, if found for this GameItem.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else False.</returns>
        public static bool TryGetFloatValue(GameItem gameItem, int statDefinitionHash, out float value)
        {
            return StatDictionary<float>.TryGetValue(gameItem, statDefinitionHash, out value);
        }

        /// <summary>
        /// Sets specified Stat on specified gameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        public static void SetFloatValue(GameItem gameItem, string statDefinitionId, float value)
        {
            StatDictionary<float>.SetValue(gameItem, Tools.StringToHash(statDefinitionId), value);
        }

        /// <summary>
        /// Sets specified Stat on specified gameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        public static void SetFloatValue(GameItem gameItem, int statDefinitionHash, float value)
        {
            StatDictionary<float>.SetValue(gameItem, statDefinitionHash, value);
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <returns>True if Stat found and removed, else false</returns>
        public static bool RemoveFloatValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<float>.RemoveValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <returns>True if Stat found and removed, else false</returns>
        public static bool RemoveFloatValue(GameItem gameItem, int statDefinitionHash)
        {
            return StatDictionary<float>.RemoveValue(gameItem, statDefinitionHash);
        }

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionId">StatDefinition's Id to search for.</param>
        /// <returns>True if specified Stat was found and reset, else False.</returns>
        public static bool ResetToDefaultFloatValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<float>.ResetToDefaultValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionHash">StatDefinition's Hash to search for.</param>
        /// <returns>True if specified Stat was found and reset, else False.</returns>
        public static bool ResetToDefaultFloatValue(GameItem gameItem, int statDefinitionHash)
        {
            return StatDictionary<float>.ResetToDefaultValue(gameItem, statDefinitionHash);
        }
    }
}
