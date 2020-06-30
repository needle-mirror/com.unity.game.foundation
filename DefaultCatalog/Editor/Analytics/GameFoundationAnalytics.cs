// #define DEBUG_EDITOR_ANALYTICS

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
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
            StoreItems,
            Tags,
            Inventories,
            Stores,
            Currencies,
            Transactions,
            GameParameters
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
                if (database == null)
                {
                    snapshot = default;
                    return false;
                }

                snapshot = new CatalogSnapshot();

                var inventoryCatalog = database.inventoryCatalog;
                var storeCatalog = database.storeCatalog;
                var transactionCatalog = database.transactionCatalog;

                // Inventory item definitions
                var items = inventoryCatalog != null ? inventoryCatalog.GetItems() : null;
                snapshot.inventoryItemCount = items?.Length ?? 0;

                // Stores definitions
                var stores = storeCatalog != null ? storeCatalog.GetItems() : null;
                snapshot.storeCount = stores?.Length ?? 0;

                // Properties
                {
                    var staticPropertyKeys = new HashSet<string>();

                    void FillStaticPropertyKeysHash(IEnumerable<CatalogItemAsset> catalogItemList)
                    {
                        foreach (var catalogItemAsset in catalogItemList)
                        {
                            foreach (var staticPropertyKey in catalogItemAsset.staticProperties.Keys)
                            {
                                staticPropertyKeys.Add(staticPropertyKey);
                            }
                        }
                    }

                    var catalogItems = new List<CatalogItemAsset>();
                    if (database.currencyCatalog != null)
                    {
                        database.currencyCatalog.GetItems(catalogItems);

                        FillStaticPropertyKeysHash(catalogItems);
                    }

                    if (database.transactionCatalog != null)
                    {
                        database.transactionCatalog.GetItems(catalogItems);

                        FillStaticPropertyKeysHash(catalogItems);
                    }

                    if (stores != null)
                        FillStaticPropertyKeysHash(stores);

                    if (items != null)
                    {
                        FillStaticPropertyKeysHash(items);

                        var propertyKeys = new HashSet<string>();
                        foreach (var item in items)
                        {
                            foreach (var itemProperty in item.properties)
                            {
                                propertyKeys.Add(itemProperty.Key);
                            }
                        }

                        snapshot.propertyCount = staticPropertyKeys.Count + propertyKeys.Count;
                    }
                    else
                        snapshot.propertyCount = staticPropertyKeys.Count;
                }

                // Transaction definitions
                {
                    var iapTransactions = transactionCatalog != null ? transactionCatalog.GetItems<IAPTransactionAsset>() : null;
                    var virtualTransactions = transactionCatalog != null ? transactionCatalog.GetItems<VirtualTransactionAsset>() : null;
                    snapshot.virtualTransactionCount = virtualTransactions?.Length ?? 0;
                    snapshot.iapTransactionCount = iapTransactions?.Length ?? 0;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                snapshot = default;
                return false;
            }
        }

        public static AnalyticsResult SendOpenTabEvent(TabName name)
        {
            return EditorAnalytics.SendEventWithLimit(k_OpenTabEvent, new AnalyticsData { TabName = name.ToString() });
        }

        static DateTime GetLastSyncInfo()
        {
            if (s_LastSyncTime != default)
            {
                return s_LastSyncTime;
            }

            var lastSyncString = SessionState.GetString(k_LastSyncTimeSessionStateKey, "0");
#if DEBUG_EDITOR_ANALYTICS
            Debug.Log($"{k_LastSyncTimeSessionStateKey} is {lastSyncString}");
#endif
            var parsed = long.TryParse(lastSyncString, out var lastSyncBinary);
            if (!parsed)
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

            if (duration.TotalSeconds > k_SnapshotFrequency)
            {
#if DEBUG_EDITOR_ANALYTICS
                Debug.Log("Try to sync");
#endif
                var done = DoSnapshot(out var snapshot);

                if (!done)
                {
#if DEBUG_EDITOR_ANALYTICS
                    Debug.LogError("Can't do snapshot");
#endif
                    return;
                }

                if (s_CatalogSnapshotContainer.CatalogSnapshot != snapshot)
                {
                    s_CatalogSnapshotContainer.CatalogSnapshot = snapshot;

                    // The snapshot "looks" different than last time we synced.
                    var result = EditorAnalytics.SendEventWithLimit(
                        k_SnapshotEvent,
                        s_CatalogSnapshotContainer);

#if DEBUG_EDITOR_ANALYTICS
                    if (result == AnalyticsResult.Ok)
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
