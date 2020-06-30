#if UNITY_ANALYTICS

//using System.Collections.Generic;
//using UnityEngine.Analytics;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Game Foundation wrapper class for Unity Analytics
    /// </summary>
    class AnalyticsWrapper
    {
        static bool m_Initialized;

        /// <summary>
        /// This binds the firing methods to the notification system.
        /// </summary>
        internal static bool Initialize()
        {
            if (m_Initialized)
            {
                Debug.LogWarning("AnalyticsWrapper is already initialized and cannot be initialized again.");
                return false;
            }

            if (!VerifyAnalyticsEnabled())
            {
                return false;
            }

            NotificationSystem.RegisterNotification(NotificationType.Created, SendItemCreated);
            NotificationSystem.RegisterNotification(NotificationType.Destroyed, SendItemDestroyed);
            NotificationSystem.RegisterNotification(NotificationType.Modified, SendItemModified);

            m_Initialized = true;

            return true;
        }

        internal static void Uninitialize()
        {
            if (!m_Initialized)
            {
                return;
            }

            NotificationSystem.UnRegisterNotification(NotificationType.Created, SendItemCreated);
            NotificationSystem.UnRegisterNotification(NotificationType.Destroyed, SendItemDestroyed);
            NotificationSystem.UnRegisterNotification(NotificationType.Modified, SendItemModified);

            m_Initialized = false;
        }

        static AnalyticsDetail GetAnalyticsDetail(InventoryItem item)
        {
            if (item == null || item.definition == null)
            {
                return null;
            }

            return item.definition.GetDetail<AnalyticsDetail>();
        }

        static bool VerifyAnalyticsEnabled()
        {
#if UNITY_ANALYTICS
            if (!Application.isPlaying && GameFoundationSettings.enableEditorModeAnalytics)
            {
                return true;
            }

            if (GameFoundationSettings.enablePlayModeAnalytics)
            {
                return true;
            }
#endif
            return false;
        }

        static void SendCustomGameItemEvent(string eventName, InventoryItem item)
        {
#if UNITY_ANALYTICS

            // if (!Analytics.Analytics.enabled || !m_Initialized || GetAnalyticsDetail(item) == null)
            // {
            //     return;
            // }

            // TODO: rewrite for new inventory architecture
            // int quantity = 0;
            // string inventoryOwner = "None";
            // bool hasQuantity = false;
            // if (item.GetType() == typeof(InventoryItem))
            // {
            //     var inventoryItem = (InventoryItem)(item);
            //     quantity = inventoryItem.quantity;
            //     hasQuantity = true;
            //
            //     if (inventoryItem.inventory != null)
            //     {
            //         inventoryOwner = inventoryItem.inventory.displayName;
            //     }
            // }
            //
            // AnalyticsEvent.Custom(eventName, new Dictionary<string, object>
            // {
            //     { "id", item.id },
            //     { "quantity", hasQuantity ? quantity.ToString() : "-" },
            //     //{ "currencyType", currencyType },
            //     { "owner", inventoryOwner }
            // });
#endif
        }

        /// <summary>
        /// Triggered on Created notifications.
        /// </summary>
        /// <param name="item">The game item that was created.</param>
        static void SendItemCreated(InventoryItem item)
        {
            SendCustomGameItemEvent("gameitem_created", item);
        }

        /// <summary>
        /// Triggered on Destroyed notifications
        /// </summary>
        /// <param name="item">The game item that was destroyed.</param>
        static void SendItemDestroyed(InventoryItem item)
        {
            SendCustomGameItemEvent("gameitem_destroyed", item);
        }

        /// <summary>
        /// Triggered on Modified notifications
        /// </summary>
        /// <param name="item">The game item that was modified.</param>
        static void SendItemModified(InventoryItem item)
        {
            SendCustomGameItemEvent("gameitem_modified", item);
        }
    }
}
