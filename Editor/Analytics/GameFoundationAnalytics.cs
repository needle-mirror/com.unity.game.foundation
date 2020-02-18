//#define DEBUG_EDITOR_ANALYTICS

using System;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    [InitializeOnLoad]
    static class GameFoundationAnalytics
    {
        const string k_Prefix = "gameFoundation";
        const string k_OpenTabEvent = k_Prefix + "OpenTab";
        const string k_SnapshotEvent = k_Prefix + "CatalogSnapshot";

        const string k_LastSyncTimeSessionStateKey = "game-foundation.catalogSnapshot.lastSync.Time";

        const double k_SnapshotFrequency = 5 * 60;

        static readonly CatalogSnapshotContainer s_CatalogSnapshotContainer = new CatalogSnapshotContainer();

        static DateTime s_LastSyncTime;

        [Serializable]
        struct AnalyticsData
        {
            public string TabName;
        }

        public enum TabName
        {
            InventoryItems,
            Categories,
            Inventories
        }

        const int k_MaxEventsPerHour = 100;
        const int k_MaxNumberOfElements = 100;
        const string k_VendorKey = "unity.gamefoundation.editor";
        const int k_Version = 1;

        static GameFoundationAnalytics()
        {
            EditorAnalytics.RegisterEventWithLimit(k_OpenTabEvent, k_MaxEventsPerHour, k_MaxNumberOfElements, k_VendorKey, k_Version);
            EditorAnalytics.RegisterEventWithLimit(k_SnapshotEvent, k_MaxEventsPerHour, k_MaxNumberOfElements, k_VendorKey, k_Version);

            EditorApplication.update += SyncCatalogSnapshot;
        }

        public static bool DoSnapshot(out CatalogSnapshot snapshot)
        {
            try
            {
                var database = GameFoundationDatabaseSettings.database;
                if(database == null)
                {
                    snapshot = default;
                    return false;
                }

                snapshot = new CatalogSnapshot();

                var giCatalog = database.gameItemCatalog;
                var iCatalog = database.inventoryCatalog;
                var sCatalog = database.statCatalog;

                // Game item definitions
                {
                    var gameItems = giCatalog != null ? giCatalog.GetGameItemDefinitions() : null;
                    snapshot.gameItemCount = gameItems != null ? gameItems.Length : 0;
                }

                // Categories
                {
                    var categories = iCatalog != null ? iCatalog.GetCategories() : null;
                    snapshot.categoryCount = categories != null ? categories.Length : 0;
                }

                // Inventories definitions
                {
                    var inventories = iCatalog != null ? iCatalog.GetCollectionDefinitions() : null;
                    snapshot.inventoryCount = inventories != null ? inventories.Length : 0;
                }

                // Inventory item definitions
                {
                    var items = iCatalog != null ? iCatalog.GetItemDefinitions() : null;
                    snapshot.inventoryItemCount = items != null ? items.Length : 0;
                }

                // Stat definitions
                {
                    var stats = sCatalog != null ? sCatalog.GetStatDefinitions() : null;
                    snapshot.statCount = stats != null ? stats.Length : 0;
                }

                return true;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                snapshot = default;
                return false;
            }
        }


        public static AnalyticsResult SendOpenTabEvent(TabName name)
        {
            return EditorAnalytics.SendEventWithLimit(k_OpenTabEvent, new AnalyticsData(){TabName = name.ToString() });
        }

        static DateTime GetLastSyncInfo()
        {
            if(s_LastSyncTime != default)
            {
                return s_LastSyncTime;
            }

            var lastSyncString = SessionState.GetString(k_LastSyncTimeSessionStateKey, "0");
#if DEBUG_EDITOR_ANALYTICS
            Debug.Log($"{k_LastSyncTimeSessionStateKey} is {lastSyncString}");
#endif
            var parsed = long.TryParse(lastSyncString, out var lastSyncBinary);
            if(!parsed)
            {
                lastSyncBinary = 0;
            }
            return DateTime.FromBinary(lastSyncBinary);
        }

        static void SyncCatalogSnapshot()
        {
            var now = DateTime.UtcNow;

            var lastSyncDate = GetLastSyncInfo();

            var duration = now - lastSyncDate;

            if(duration.TotalSeconds > k_SnapshotFrequency)
            {
#if DEBUG_EDITOR_ANALYTICS
                Debug.Log("Try to sync");
#endif
                var done = DoSnapshot(out var snapshot);

                if(!done)
                {
#if DEBUG_EDITOR_ANALYTICS
                    Debug.LogError("Can't do snapshot");
#endif
                    return;
                }

                if(s_CatalogSnapshotContainer.CatalogSnapshot != snapshot)
                {
                    s_CatalogSnapshotContainer.CatalogSnapshot = snapshot;
                    // The snapshot "looks" different than last time we synced.
                    var result = EditorAnalytics.SendEventWithLimit(
                        k_SnapshotEvent,
                        s_CatalogSnapshotContainer);

#if DEBUG_EDITOR_ANALYTICS
                    if(result == AnalyticsResult.Ok)
                    {
                        Debug.Log("Successfully sent snapshot");
                    }
                    else
                    {
                        Debug.LogError($"Failed sent snapshot: {result}");
                    }
#endif
                }

                s_LastSyncTime = now;
                var syncBinary = now.ToBinary();
                SessionState.SetString(k_LastSyncTimeSessionStateKey, syncBinary.ToString());
            }
        }
    }
}
