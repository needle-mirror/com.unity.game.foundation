using System;
using System.Collections.Generic;

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
        static class StatDictionary<T> where T : new()
        {
            // Note: long is gameItem.gameItemId << 32 | statDefHashId
            //       gggggggg gggggggg gggggggg gggggggg  ssssssss ssssssss ssssssss ssssssss
            // where 'g' represents gameItem.gameItemId
            //  and  's' represents the stat definition id hash
            static Dictionary<long, StatItem<T>> m_StatItems = null;

            /// <summary>
            // Initialize the dictionary for this generic type.  Should only be called once.
            /// </summary>
            public static void Initialize()
            {
                if (IsInitialized)
                {
                    throw new InvalidOperationException("Error: StatManager.Initialize was called more than once--please use Reset() to reinitialize.");
                }
                m_StatItems = new Dictionary<long, StatItem<T>>();
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
            }

            /// <summary>
            // Simply throws exception if Initialize() has not been called.
            /// </summary>
            private static void ThrowIfNotInitialized()
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Error: StatManager.Initialize() MUST be called before the StatManager is used.");
                }
            }

            /// <summary>
            // Get the key for specified game item and stat definition hash
            /// </summary>
            /// <param name="gameItem">Game item to get key for.</param>
            /// <param name="statDefinitionIdHash">StatDefinition hash to get key for.</param>
            /// <returns>Key to be used in dictionary to find specified GameItem's Stat.</returns>
            static long GetKey(GameItem gameItem, int statDefinitionIdHash)
            {
                ThrowIfNotInitialized();

                if (ReferenceEquals(gameItem, null))
                {
                    return 0;
                }
                return (((long)gameItem.gameItemId) << 32) | (((long)statDefinitionIdHash) & 0xffff);
            }

            /// <summary>
            // Get the stat item for specified game item and stat definition hash
            /// </summary>
            /// <param name="gameItem">GameItem to get StatItem for.</param>
            /// <param name="statDefinitionIdHash">StatDefinition hash to get key for.</param>
            /// <returns>StatItem which holds current value of the specified GameItem's Stat.</returns>
            public static StatItem<T> GetStatItem(GameItem gameItem, int statDefinitionIdHash)
            {
                ThrowIfNotInitialized();

                if (ReferenceEquals(gameItem, null))
                {
                    return null;
                }

                var key = GetKey(gameItem, statDefinitionIdHash);

                StatItem<T> statItem;
                if (m_StatItems.TryGetValue(key, out statItem))
                {
                    return statItem;
                }

                return null;
            }

            /// <summary>
            // Returns true if the specified stat exists by game item and stat definition id hash.
            /// </summary>
            /// <param name="gameItem">GameItem to check if value exists for.</param>
            /// <param name="statDefinitionIdHash">StatDefinition id hash to check if value exists for.</param>
            /// <returns>True if the specified GameItem ahs the requested StatDefinition set.</returns>
            public static bool HasValue(GameItem gameItem, int statDefinitionIdHash)
            {
                ThrowIfNotInitialized();

                return !ReferenceEquals(GetStatItem(gameItem, statDefinitionIdHash), null);
            }

            /// <summary>
            // Get the current stat value for specified game item and stat definition id hash
            /// </summary>
            /// <param name="gameItem">GameItem to get current value of stat for.</param>
            /// <param name="statDefinitionIdHash">StatDefintion id hash to get current value of stat for.</param>
            /// <returns>The current value of the specified Stat.</returns>
            public static T GetValue(GameItem gameItem, int statDefinitionIdHash)
            {
                ThrowIfNotInitialized();

                var statItem = GetStatItem(gameItem, statDefinitionIdHash);
                if (ReferenceEquals(statItem, null))
                {
                    throw new KeyNotFoundException("No stat found for game item.");
                }
                return statItem.value;
            }

            /// <summary>
            // Get the specified stat's value, returns true if found and successfully returned, else false
            /// </summary>
            /// <param name="gameItem">GameItem to try to get stat value for.</param>
            /// <param name="statDefinitionIdHash">StatDefinition id hash to try to get stat value for.</param>
            /// <param name="value">return value for specified stat, if it exists</param>
            /// <returns>True if stat value was found and returned.</returns>
            public static bool TryGetValue(GameItem gameItem, int statDefinitionIdHash, out T value)
            {
                ThrowIfNotInitialized();

                var statItem = GetStatItem(gameItem, statDefinitionIdHash);
                if (!ReferenceEquals(statItem, null))
                {
                    value = statItem.value;
                    return true;
                }
                value = new T();
                return false;
            }

            /// <summary>
            /// Sets specified stat on specified gameItem to a specific value
            /// </summary>
            /// <param name="gameItem">GameItem to set Stat value for.</param>
            /// <param name="statDefinitionIdHash">StatDefinition id hash to set Stat value for.</param>
            /// <param name="value">New value for Stat.</param>
            public static void SetValue(GameItem gameItem, int statDefinitionIdHash, T value)
            {
                ThrowIfNotInitialized();

                if (ReferenceEquals(gameItem, null))
                {
                    throw new ArgumentNullException("The GameItem passed in was null.");
                }

                var gameItemId = gameItem.gameItemId;

                var key = GetKey(gameItem, statDefinitionIdHash);

                StatItem<T> statItem;
                if (!m_StatItems.TryGetValue(key, out statItem))
                {
                    var statDefinition = GameFoundationSettings.statCatalog.GetStatDefinition(statDefinitionIdHash);
                    if (ReferenceEquals(statDefinition, null))
                    {
                        throw new NullReferenceException("Attempted to set stat for hash that does not exist in stat catalog.");
                    }
                    if (!statDefinition.DoesValueTypeMatch(typeof(T)))
                    {
                        throw new System.InvalidOperationException($"Stat value type does not match. Expected {statDefinition.statValueType} but received {typeof(T)}");
                    }
                    statItem = new StatItem<T>(gameItemId, statDefinition.id, value);
                    m_StatItems[key] = statItem;
                }
                statItem.value = value;
            }

            /// <summary>
            /// Remove specified Stat value for specified GameItem and StatDefinition id hash.
            /// </summary>
            /// <param name="gameItem">GameItem to remove stat for.</param>
            /// <param name="statDefinitionIdHash">StatDefinition id hash to remove Stat for.</param>
            /// <returns>True if specified Stat was found and removed, else False.</returns>
            public static bool RemoveValue(GameItem gameItem, int statDefinitionIdHash)
            {
                ThrowIfNotInitialized();

                if (ReferenceEquals(gameItem, null))
                {
                    return false;
                }

                var key = GetKey(gameItem, statDefinitionIdHash);
                return m_StatItems.Remove(key);
            }

            /// <summary>
            /// Reset stat to the correct default value.
            /// </summary>
            /// <param name="gameItem">GameItem to set Stat default value for.</param>
            /// <param name="statDefinitionIdHash">StatDefinition id hash to reset Stat value for.</param>
            /// <returns>True if Stat was reset, else false.</returns>
            public static bool ResetToDefaultValue(GameItem gameItem, int statDefinitionIdHash)
            {
                ThrowIfNotInitialized();

                var stat = GetStatItem(gameItem, statDefinitionIdHash);
                if (!ReferenceEquals(stat, null))
                {
                    stat.value = stat.defaultValue;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Initializes the StatManager.  Must be called before any stat access is allowed and must never be called again.
        /// Once Initialized, you must use Reset() to return StatManager to its default state, if desired.
        /// </summary>
        public static void Initialize()
        {
            StatDictionary<int>.Initialize();
            StatDictionary<float>.Initialize();
        }

        /// <summary>
        /// Returns the current initialization state of the StatManager.
        /// </summary>
        /// <returns>The current initialization state of the StatManager.</returns>
        public static bool IsInitialized
        {
            get { return StatDictionary<int>.IsInitialized; }
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


        // ***********************
        // IMPLEMENTATION FOR INTs
        // ***********************

        /// <summary>
        /// Checks if the specified Stat has been set for specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        public static bool HasIntValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<int>.HasValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Checks if the specified Stat has been set for specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which stat is to be inspected.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        public static bool HasIntValue(GameItem gameItem, int statDefinitionIdHash)
        {
            return StatDictionary<int>.HasValue(gameItem, statDefinitionIdHash);
        }

        /// <summary>
        /// Get's the value of the specified Stat on specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <returns>Current Stat value.</returns>
        public static int GetIntValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<int>.GetValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get's the value of the specified Stat on specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <returns>Current Stat value.</returns>
        public static int GetIntValue(GameItem gameItem, int statDefinitionIdHash)
        {
            return StatDictionary<int>.GetValue(gameItem, statDefinitionIdHash);
        }

        /// <summary>
        /// Tries to get specified Stat for specified GameItem and set out variable, if found.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
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
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <param name="value">Current value of Stat, if found for this GameItem.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else False.</returns>
        public static bool TryGetIntValue(GameItem gameItem, int statDefinitionIdHash, out int value)
        {
            return StatDictionary<int>.TryGetValue(gameItem, statDefinitionIdHash, out value);
        }

        /// <summary>
        /// Sets specified Stat on specified gameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        public static void SetIntValue(GameItem gameItem, string statDefinitionId, int value)
        {
            StatDictionary<int>.SetValue(gameItem, Tools.StringToHash(statDefinitionId), value);
        }

        /// <summary>
        /// Sets specified Stat on specified gameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        public static void SetIntValue(GameItem gameItem, int statDefinitionIdHash, int value)
        {
            StatDictionary<int>.SetValue(gameItem, statDefinitionIdHash, value);
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <returns>True if stat found and removed, else false</returns>
        public static bool RemoveIntValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<int>.RemoveValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <returns>True if stat found and removed, else false</returns>
        public static bool RemoveIntValue(GameItem gameItem, int statDefinitionIdHash)
        {
            return StatDictionary<int>.RemoveValue(gameItem, statDefinitionIdHash);
        }

        /// <summary>
        /// Reset stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <returns>True if specified stat was found and reset, else False.</returns>
        public static bool ResetToDefaultIntValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<int>.ResetToDefaultValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Reset stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <returns>True if specified stat was found and reset, else False.</returns>
        public static bool ResetToDefaultIntValue(GameItem gameItem, int statDefinitionIdHash)
        {
            return StatDictionary<int>.ResetToDefaultValue(gameItem, statDefinitionIdHash);
        }


        // *************************
        // IMPLEMENTATION FOR FLOATs
        // *************************

        /// <summary>
        /// Checks if the specified Stat has been set for specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        public static bool HasFloatValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<float>.HasValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Checks if the specified Stat has been set for specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which stat is to be inspected.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        public static bool HasFloatValue(GameItem gameItem, int statDefinitionIdHash)
        {
            return StatDictionary<float>.HasValue(gameItem, statDefinitionIdHash);
        }

        /// <summary>
        /// Get's the value of the specified Stat on specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <returns>Current Stat value.</returns>
        public static float GetFloatValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<float>.GetValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get's the value of the specified Stat on specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <returns>Current Stat value.</returns>
        public static float GetFloatValue(GameItem gameItem, int statDefinitionIdHash)
        {
            return StatDictionary<float>.GetValue(gameItem, statDefinitionIdHash);
        }

        /// <summary>
        /// Tries to get specified Stat for specified GameItem and set out variable, if found.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be inspected.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
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
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <param name="value">Current value of Stat, if found for this GameItem.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else False.</returns>
        public static bool TryGetFloatValue(GameItem gameItem, int statDefinitionIdHash, out float value)
        {
            return StatDictionary<float>.TryGetValue(gameItem, statDefinitionIdHash, out value);
        }

        /// <summary>
        /// Sets specified Stat on specified gameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        public static void SetFloatValue(GameItem gameItem, string statDefinitionId, float value)
        {
            StatDictionary<float>.SetValue(gameItem, Tools.StringToHash(statDefinitionId), value);
        }

        /// <summary>
        /// Sets specified Stat on specified gameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        public static void SetFloatValue(GameItem gameItem, int statDefinitionIdHash, float value)
        {
            StatDictionary<float>.SetValue(gameItem, statDefinitionIdHash, value);
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <returns>True if stat found and removed, else false</returns>
        public static bool RemoveFloatValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<float>.RemoveValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <returns>True if stat found and removed, else false</returns>
        public static bool RemoveFloatValue(GameItem gameItem, int statDefinitionIdHash)
        {
            return StatDictionary<float>.RemoveValue(gameItem, statDefinitionIdHash);
        }

        /// <summary>
        /// Reset stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionId">StatDefinition's id to search for.</param>
        /// <returns>True if specified stat was found and reset, else False.</returns>
        public static bool ResetToDefaultFloatValue(GameItem gameItem, string statDefinitionId)
        {
            return StatDictionary<float>.ResetToDefaultValue(gameItem, Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Reset stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionIdHash">StatDefinition's id hash to search for.</param>
        /// <returns>True if specified stat was found and reset, else False.</returns>
        public static bool ResetToDefaultFloatValue(GameItem gameItem, int statDefinitionIdHash)
        {
            return StatDictionary<float>.ResetToDefaultValue(gameItem, statDefinitionIdHash);
        }
    }
}
