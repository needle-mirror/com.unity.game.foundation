using System;
using UnityEngine;
using UnityEngine.Analytics;

namespace UnityEditor.GameFoundation
{
    static class GameFoundationAnalytics
    {

        const string k_Prefix = "gameFoundation";
        const string k_OpenTabEvent = k_Prefix + "OpenTab";

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
        }


        public static AnalyticsResult SendOpenTabEvent(TabName name)
        {
            return EditorAnalytics.SendEventWithLimit(k_OpenTabEvent, new AnalyticsData(){TabName = name.ToString() });
        }
    }
}
