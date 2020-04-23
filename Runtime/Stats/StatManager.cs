using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Retrieve and manage all runtime stat data.
    /// </summary>
    public static class StatManager
    {
        /// <summary>
        /// Dictionary of ints to dictionaries of strings to StatValues for all stats.
        /// </summary>
        internal static Dictionary<int, Dictionary<string, StatValue>> s_Stats = new Dictionary<int, Dictionary<string, StatValue>>();

        /// <summary>
        /// Static stat data layer providing data to the StatManager.
        /// </summary>
        static IStatDataLayer s_DataLayer;

        /// <summary>
        /// Initialize StatManager.
        /// </summary>
        /// <param name="dataLayer">Stat data layer.</param>
        /// <exception cref="InventoryItemNotFoundException">If the InventoryItem is not found.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="ArgumentException">If any StatDefinitionIds are null or empty.</exception>
        internal static void Initialize(IStatDataLayer dataLayer)
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"{nameof(StatManager)} is already initialized and cannot be initialized again.");
                return;
            }

            IsInitialized = true;

            s_DataLayer = dataLayer;

            if (s_DataLayer != null)
            {
                IsInitialized = FillFromStatsData();
            }
        }

        /// <summary>
        /// Unitialize StatManager.
        /// </summary>
        internal static void Uninitialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            s_Stats.Clear();

            IsInitialized = false;
        }

        /// <summary>
        /// Returns the current initialization state of the StatManager.
        /// </summary>
        /// <returns>The current initialization State of the StatManager.</returns>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// This is the StatCatalog the StatManager uses.
        /// </summary>
        /// <returns>The StatCatalog the StatManager uses.</returns>
        public static StatCatalog catalog => GameFoundation.catalogs.statCatalog;

        /// <summary>
        /// Fills StatManager data from data layer.
        /// </summary>
        /// <returns>true.</returns>
        /// <exception cref="InventoryItemNotFoundException">If the InventoryItem is not found.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="ArgumentException">If any StatDefinitionIds are null or empty.</exception>
        internal static bool FillFromStatsData()
        {
            var statManagerData = s_DataLayer.GetData();
            if (statManagerData.items is null)
            {
                Debug.LogWarning("Persistence Data data doesn't contain Stats.");

                statManagerData = StatManagerSerializableData.Empty;
            }

            foreach (var itemData in statManagerData.items)
            {
                var item = InventoryManager.FindItemInternal(itemData.itemId);
                if (item is null)
                {
                    throw new InventoryItemNotFoundException(itemData.itemId);
                }

                if (itemData.stats != null && itemData.stats.Length > 0)
                {
                    var detail = item.definition.GetDetail<StatDetail>();
                    if (detail is null)
                    {
                        throw new DetailNotFoundException(item.id, typeof(StatDetail));
                    }

                    var statItem = new Dictionary<string, StatValue>();
                    s_Stats.Add(item.instanceId, statItem);
                    foreach (var stat in itemData.stats)
                    {
                        var statDefinition = Tools.GetStatDefinitionOrDie(stat.statId, nameof(stat.statId));
                        detail.HasDefinition(statDefinition);

                        statItem.Add(statDefinition.id, stat.value);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Called by game item destructor to clean up stats for the game item
        /// </summary>
        /// <param name="gameItem">GameItem discarded.</param>
        internal static void OnGameItemDiscarded(GameItem gameItem)
        {
            // since inventory manager can be used without a stat manager, we should only ...
            // ... remove stats if the stat manager is actually active
            if (IsInitialized)
            {
                s_Stats.Remove(gameItem.instanceId);
            }
        }

        /// <summary>
        /// Synchronize the update of the stat value with the data layer.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to sync stat value.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <param name="value">StatValue.</param>
        static void SyncSetValue(string gameItemId, string statDefinitionId, StatValue value)
        {
            s_DataLayer.SetStatValue(gameItemId, statDefinitionId, value, Completer.None);
        }

        /// <summary>
        /// Synchronize the deletion of the stat value with the data layer.
        /// </summary>
        /// <param name="gameItemId">GameItem to sync deleted stat upon.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        static void SyncDeleteValue(string gameItemId, string statDefinitionId)
        {
            s_DataLayer.DeleteStatValue(gameItemId, statDefinitionId, Completer.None);
        }

        /// <summary>
        /// Throw if StatDetail is not found or has errors.
        /// </summary>
        /// <param name="gameItem">GameItem to get StatDetail upon.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        static void AssertDetail(GameItem gameItem, StatDefinition statDefinition)
        {
            var detail = gameItem.definition.GetDetail<StatDetail>();
            if (detail is null)
            {
                throw new DetailNotFoundException(gameItem.id, typeof(StatDetail));
            }

            var found = detail.HasDefinition(statDefinition);
            if (!found)
            {
                throw new StatDefinitionNotFoundException(gameItem.id, statDefinition.id);
            }
        }

        #region HasValue methods

        /// <summary>
        /// Determine if a GameItem has a StatDefinition.
        /// </summary>
        /// <param name="gameItem">GameItem to search.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>True if GameItem has specified StatDefinition.</returns>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        static bool HasValueInternal(GameItem gameItem, StatDefinition statDefinition)
        {
            AssertDetail(gameItem, statDefinition);

            var found = s_Stats.TryGetValue(gameItem.instanceId, out var stats);
            if (!found) return false;

            return stats.ContainsKey(statDefinition.id);
        }

        /// <summary>
        /// Determines if specified GameItem has a value for the specified StatDefinition.
        /// </summary>
        /// <param name="gameItem">The GameItem to check for a stat.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>True if the specified GameItem has a value for the specified StatDefinition.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static bool HasValue(GameItem gameItem, StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            return HasValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Determines if specified GameItem has a value for the specified StatDefinition.
        /// </summary>
        /// <param name="gameItem">The GameItem to check for a stat.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <returns>True if the specified GameItem has a value for the specified StatDefinition.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static bool HasValue(GameItem gameItem, string statDefinitionId)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            return HasValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Determines if specified GameItem has a value for the specified StatDefinition.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to check for stat.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>True if the specified GameItem has a value for the specified StatDefinition.</returns>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static bool HasValue(string gameItemId, StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));

            return HasValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Determines if specified GameItem has a value for the specified StatDefinition.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to check for stat.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <returns>True if specified GameItem has specified Stat.</returns>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static bool HasValue(string gameItemId, string statDefinitionId)
        {
            var item = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));
            var definition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            return HasValueInternal(item, definition);
        }

        #endregion

        #region TryGetValue methods

        /// <summary>
        /// Search for specified StatDefinition from specified GameItem and output StatValue, if found.
        /// </summary>
        /// <param name="gameItem">The GameItem to check for a stat.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <param name="value">StatValue if found.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else false.</returns>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        static bool TryGetValueInternal(GameItem gameItem, StatDefinition statDefinition, out StatValue value)
        {
            AssertDetail(gameItem, statDefinition);

            s_Stats.TryGetValue(gameItem.instanceId, out var stats);
            if (stats is null)
            {
                value = default;
                return false;
            }

            return stats.TryGetValue(statDefinition.id, out value);
        }

        /// <summary>
        /// Try to get specified StatDefinition from specified GameItem and output StatValue, if found.
        /// </summary>
        /// <param name="gameItem">The GameItem to check for specified StatDefinition.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <param name="value">StatValue if found.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else false.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static bool TryGetValue(GameItem gameItem, StatDefinition statDefinition, out StatValue value)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            return TryGetValueInternal(gameItem, statDefinition, out value);
        }

        /// <summary>
        /// Search for specified StatDefinition from specified GameItem and output StatValue, if found.
        /// </summary>
        /// <param name="gameItem">The GameItem to check for specified StatDefinition.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <param name="value">StatValue if found.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else false.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static bool TryGetValue(GameItem gameItem, string statDefinitionId, out StatValue value)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            return TryGetValueInternal(gameItem, statDefinition, out value);
        }

        /// <summary>
        /// Search for specified StatDefinition from specified GameItem and output StatValue, if found.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to check for stat.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <param name="value">StatValue if found.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else false.</returns>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static bool TryGetValue(string gameItemId, StatDefinition statDefinition, out StatValue value)
        {
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            return TryGetValueInternal(gameItem, statDefinition, out value);
        }

        /// <summary>
        /// Search for specified StatDefinition from specified GameItem and output StatValue, if found.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to check for stat.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <param name="value">StatValue if found.</param>
        /// <returns>True if specified Stat exists and is set on specified GameItem, else false.</returns>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static bool TryGetValue(string gameItemId, string statDefinitionId, out StatValue value)
        {
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            return TryGetValueInternal(gameItem, statDefinition, out value);
        }

        #endregion

        #region GetValue methods

        /// <summary>
        /// Gets specified StatDefinition from specified GameItem.
        /// </summary>
        /// <param name="gameItem">The GameItem to search for desired StatDefinition.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>StatValue for specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        static StatValue GetValueInternal(GameItem gameItem, StatDefinition statDefinition)
        {
            AssertDetail(gameItem, statDefinition);

            var found = s_Stats.TryGetValue(gameItem.instanceId, out var stats);
            if (found)
            {
                found = stats.TryGetValue(statDefinition.id, out var value);
                if (found) return value;
            }

            throw new StatDefinitionNotFoundException(gameItem.id, statDefinition.id);
        }

        /// <summary>
        /// Gets specified StatDefinition from specified GameItem.
        /// </summary>
        /// <param name="gameItem">The GameItem to search for desired StatDefinition.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>StatValue for specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        public static StatValue GetValue(GameItem gameItem, StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            return GetValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Gets specified StatDefinition from specified GameItem.
        /// </summary>
        /// <param name="gameItem">The GameItem to search for desired StatDefinition.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <returns>StatValue for specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static StatValue GetValue(GameItem gameItem, string statDefinitionId)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            return GetValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Gets specified StatDefinition from specified GameItem.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to get the stat.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>StatValue for specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        public static StatValue GetValue(string gameItemId, StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNullOrEmpty(gameItemId, nameof(gameItemId));
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));

            return GetValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Gets specified StatDefinition from specified GameItem.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to get the stat.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <returns>StatValue for specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static StatValue GetValue(string gameItemId, string statDefinitionId)
        {
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            return GetValueInternal(gameItem, statDefinition);
        }

        #endregion

        #region SetValue methods

        /// <summary>
        /// Sets specified StatDefinition on specified GameItem to a specific value.
        /// </summary>
        /// <param name="gameItem"></param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        internal static void SetValueInternal(GameItem gameItem, StatDefinition statDefinition, StatValue value)
        {
            if (statDefinition.statValueType != value.type)
            {
                throw new InvalidCastException($"{statDefinition}: Expected type: {statDefinition.statValueType}, given: {value.type}");
            }

            AssertDetail(gameItem, statDefinition);

            var found = s_Stats.TryGetValue(gameItem.instanceId, out var stats);
            if (!found)
            {
                stats = new Dictionary<string, StatValue>();
                s_Stats.Add(gameItem.instanceId, stats);
            }

            stats[statDefinition.id] = value;

            s_DataLayer.SetStatValue(gameItem.id, statDefinition.id, value, Completer.None);

            gameItem.onStatChanged(statDefinition, value);
        }

        /// <summary>
        /// Sets specified StatDefinition on specified GameItem to a specific value.
        /// </summary>
        /// <param name="gameItem"></param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static void SetValue(GameItem gameItem, StatDefinition statDefinition, StatValue value)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            SetValueInternal(gameItem, statDefinition, value);
        }

        /// <summary>
        /// Sets specified StatDefinition on specified GameItem to a specific value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be set.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static void SetValue(GameItem gameItem, string statDefinitionId, StatValue value)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            SetValueInternal(gameItem, statDefinition, value);
        }

        /// <summary>
        /// Sets specified StatDefinition on specified GameItem to a specific value.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to set the stat.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static void SetValue(string gameItemId, StatDefinition statDefinition, StatValue value)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));

            SetValueInternal(gameItem, statDefinition, value);
        }

        /// <summary>
        /// Sets specified StatDefinition on specified GameItem to a specific value.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to set the stat.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <param name="value">New value of Stat to set for this GameItem.</param>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        public static void SetValue(string gameItemId, string statDefinitionId, StatValue value)
        {
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            SetValueInternal(gameItem, statDefinition, value);
        }

        #endregion

        /// <summary>
        /// Adjusts specified StatDefinition on specified gameItem by specified amount.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be adjusted.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <param name="change">Adjustment (positive or negative) to Stat.</param>
        /// <returns>The new value of Stat on GameItem.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="Exception">If the statDefinition not found on game item.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static StatValue AdjustValue(GameItem gameItem, StatDefinition statDefinition, StatValue change)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            var found = TryGetValue(gameItem, statDefinition, out var oldValue);
            if (!found)
            {
                throw new Exception($"Cannot get the stat {statDefinition.id} from game item {gameItem.id}");
            }

            var newValue = oldValue + change;
            SetValue(gameItem, statDefinition, newValue);

            return newValue;
        }

        /// <summary>
        /// Adjusts specified StatDefinition on specified gameItem by specified amount.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be adjusted.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <param name="change">Adjustment (positive or negative) to Stat.</param>
        /// <returns>The new value of Stat on GameItem.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        /// <exception cref="Exception">If the statDefinition not found on game item.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        public static StatValue AdjustValue(GameItem gameItem, string statDefinitionId, StatValue change)
        {
            var found = TryGetValue(gameItem, statDefinitionId, out var oldValue);
            if (!found)
            {
                throw new Exception($"Cannot get the stat {statDefinitionId} from game item {gameItem.id}");
            }

            var newValue = oldValue + change;
            SetValue(gameItem, statDefinitionId, newValue);

            return newValue;
        }

        /// <summary>
        /// Remove specified Stat from specified GameItem.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be removed.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <returns>True if Stat found and removed, else false</returns>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        static bool RemoveValue(GameItem gameItem, string statDefinitionId)
        {
            var found = s_Stats.TryGetValue(gameItem.instanceId, out var stats);
            if (!found) return false;

            var removed = stats.Remove(statDefinitionId);
            if (removed)
            {
                s_DataLayer.DeleteStatValue
                    (gameItem.id, statDefinitionId, Completer.None);
            }

            return removed;
        }

        #region ResetToDefaultValue methods

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem to reset to default value.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>Default value of specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        static StatValue ResetToDefaultValueInternal(GameItem gameItem, StatDefinition statDefinition)
        {
            var statDetail = gameItem.definition.GetDetail<StatDetail>();
            if (statDetail is null)
            {
                throw new DetailNotFoundException(gameItem.id, typeof(StatDetail));
            }

            var found = statDetail.m_DefaultValues.TryGetValue(statDefinition.id, out var defaultValue);

            if (!found)
            {
                throw new StatDefinitionNotFoundException(gameItem.id, statDefinition.id);
            }

            SetValueInternal(gameItem, statDefinition, defaultValue);

            return defaultValue;
        }

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem to reset to default value.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>Default value of specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        public static StatValue ResetToDefaultValue(GameItem gameItem, StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            return ResetToDefaultValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItem">GameItem upon which Stat is to be reset.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <returns>Default value of specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="ArgumentNullException">If the gameItem argument is null.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        public static StatValue ResetToDefaultValue(GameItem gameItem, string statDefinitionId)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            return ResetToDefaultValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to reset stat value.</param>
        /// <param name="statDefinition">StatDefinition this method is to act upon.</param>
        /// <returns>Default value of specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="ArgumentNullException">If the statDefinition argument is null.</exception>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        public static StatValue ResetToDefaultValue(string gameItemId, StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));

            return ResetToDefaultValueInternal(gameItem, statDefinition);
        }

        /// <summary>
        /// Reset Stat to the correct default value.
        /// </summary>
        /// <param name="gameItemId">GameItem upon which StatManager is to reset stat value.</param>
        /// <param name="statDefinitionId">Id of StatDefinition this method is to act upon.</param>
        /// <returns>Default value of specified StatDefinition on specified GameItem.</returns>
        /// <exception cref="ArgumentException">If the gameItemId is null or empty.</exception>
        /// <exception cref="InventoryItemNotFoundException">If the gameItemId is not found.</exception>
        /// <exception cref="ArgumentException">If the statDefinitionId is null or empty.</exception>
        /// <exception cref="StatDefinitionNotFoundException">If the StatDefinition is not found.</exception>
        /// <exception cref="InvalidCastException">If the StatValueType doesn't match StatDefinition.</exception>
        /// <exception cref="DetailNotFoundException">If the StatDetail is not found on GameItem.</exception>
        public static StatValue ResetToDefaultValue(string gameItemId, string statDefinitionId)
        {
            var gameItem = Tools.GetItemOrDie(gameItemId, nameof(gameItemId));
            var statDefinition = Tools.GetStatDefinitionOrDie(statDefinitionId, nameof(statDefinitionId));

            return ResetToDefaultValueInternal(gameItem, statDefinition);
        }

        #endregion
    }
}
