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

        private static int m_LastHashUsed = 0;

        /// <summary>
        /// Registers a specific hash for specified GameItem so it can be looked up later.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's hash id to unregister with GameItemLookup.
        /// <param name="gameItem">The GameItem to register with GameItemLookup.
        /// <returns>True if GameItem was properly registered (hash must not already be registered).</returns>
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
        /// Unregisters a specific hash from GameItemLookup.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's hash id to unregister.
        /// <returns>True if GameItem was properly unregistered (hash must be registered).</returns>
        public static bool UnregisterInstance(int gameItemIdHash)
        {
            if (!m_Instances.ContainsKey(gameItemIdHash))
            {
                return false;
            }
            
            return m_Instances.Remove(gameItemIdHash);
        }

        /// <summary>
        /// Looks up GameItem for specified hash.
        /// </summary>
        /// <param name="gameItemIdHash">The GameItem's id hash to look up.
        /// <returns>GameItem previously registered with specified hash.</returns>
        public static GameItem GetInstance(int gameItemIdHash)
        {
            if (!m_Instances.ContainsKey(gameItemIdHash))
            {
                return null;
            }
            
            return m_Instances[gameItemIdHash];
        }

        /// <summary>
        /// Returns next hash to assign to a GameItem and updates internal counter so all hashes assigned are unique.
        /// </summary>
        /// <returns>Hash to assign to newly created GameItem.</returns>
        public static int GetNextIdForInstance()
        {
            ++m_LastHashUsed;
            return m_LastHashUsed;
        }
    }
}
