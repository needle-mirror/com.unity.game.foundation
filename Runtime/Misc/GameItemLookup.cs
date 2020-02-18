using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Allow looking up GameItems at runtime.
    /// </summary>
    internal static class GameItemLookup
    {
        private static Dictionary<int, GameItem> m_Instances = new Dictionary<int, GameItem>();

        static IGameItemLookupDataLayer s_DataLayer;

        private static int m_LastGameItemIdUsed = 0;

        private static bool m_IsInitialized = false;

        /// <summary>
        /// Returns the current initialization state of GameItemLookup.
        /// </summary>
        public static bool IsInitialized
        {
            get { return m_IsInitialized; }
        }

        internal static void Initialize(IGameItemLookupDataLayer dataLayer)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("GameItemLookup is already initialized and cannot be initialized again.");
                return;
            }

            s_DataLayer = dataLayer;

            m_IsInitialized = true;

            if (s_DataLayer != null)
            {
                m_IsInitialized = FillFromLookupData();
            }
        }

        internal static void Unintialize()
        {
            if (!IsInitialized)
            {
                return;
            }

            Reset();

            m_IsInitialized = false;
        }

        internal static void Reset()
        {
            m_Instances = new Dictionary<int, GameItem>();

            // note: we do NOT want to reset this id since any existing game items being held by developer should NOT ...
            //  ...  cause stats to clear on newly-created items that happen to have the same GameItemId
//          m_LastGameItemIdUsed = 0;
        }

        internal static bool FillFromLookupData()
        {
            Reset();

            var data = s_DataLayer.GetData();
            m_LastGameItemIdUsed = data.lastGameItemIdUsed;

            return true;
        }

        /// <summary>
        /// Registers a specific Hash for specified GameItem so it can be looked up later.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's  Hash  to unregister with GameItemLookup.</param>
        /// <param name="gameItem">The GameItem to register with GameItemLookup.</param>
        /// <returns>True if GameItem was properly registered ( Hash must not already be registered).</returns>
        /// <exception cref="ArgumentException">Thrown if the given parameters are duplicates.</exception>
        public static bool RegisterInstance(int gameItemIdHash, GameItem gameItem)
        {
            if (gameItem == null)
            {
                return false;
            }

            if (m_Instances.ContainsKey(gameItemIdHash))
            {
                throw new ArgumentException("Cannot register an instance with a duplicate item hash.");
            }

            m_Instances[gameItemIdHash] = gameItem;
            return true;
        }

        /// <summary>
        /// Unregisters a specific Hash from GameItemLookup.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's  Hash  to unregister.</param>
        /// <returns>True if GameItem was properly unregistered ( Hash must be registered).</returns>
        public static bool UnregisterInstance(int gameItemIdHash)
        {
            if (!m_Instances.ContainsKey(gameItemIdHash))
            {
                return false;
            }

            return m_Instances.Remove(gameItemIdHash);
        }

        /// <summary>
        /// Looks up GameItem for specified Hash.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's Hash to look up.</param>
        /// <returns>GameItem previously registered with specified Hash.</returns>
        public static GameItem GetInstance(int gameItemIdHash)
        {
            if (!m_Instances.ContainsKey(gameItemIdHash))
            {
                return null;
            }

            return m_Instances[gameItemIdHash];
        }

        /// <summary>
        /// Returns next Hash to assign to a GameItem and updates internal counter so all Hash es assigned are unique.
        /// </summary>
        /// <returns>Hash to assign to newly created GameItem.</returns>
        public static int GetNextIdForInstance()
        {
            do
            {
                ++m_LastGameItemIdUsed;
            } while (m_LastGameItemIdUsed == 0
                || m_Instances.ContainsKey(m_LastGameItemIdUsed));

            //Synchronize last game item id used.
            GameFoundation.GetPromiseHandles(out var deferred, out var completer);
            s_DataLayer.SetLastGameItemIdUsed(m_LastGameItemIdUsed, completer);
            GameFoundation.updater.ReleaseOrQueue(deferred);

            return m_LastGameItemIdUsed;
        }
    }
}
