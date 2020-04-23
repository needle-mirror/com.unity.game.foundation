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

        /// <summary>
        /// Stores the next supposingly available instance id.
        /// This value is checked when assigning it to a new game item.
        /// </summary>
        static int m_NextInstancetId = int.MinValue;

        /// <summary>
        /// Gets the next available instance id.
        /// </summary>
        /// <returns></returns>
        static int GetNextInstanceId()
        {
            while(m_Instances.ContainsKey(m_NextInstancetId))
            {
                m_NextInstancetId++;
            }

            var id = m_NextInstancetId++;
            return id;
        }

        /// <summary>
        /// Assigns a instance id to the newly created
        /// <paramref name="gameItem"/>.
        /// </summary>
        /// <param name="gameItem">The GameItem to register.</param>
        /// <returns>The instance id of <paramref name="gameItem"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if
        /// <paramref name="gameItem"/> is null.</exception>
        internal static int Register(GameItem gameItem)
        {
            Tools.ThrowIfArgNull(gameItem, nameof(gameItem));

            if(m_Instances.ContainsValue(gameItem))
            {
                return gameItem.instanceId;
            }

            var instanceId = GetNextInstanceId();

            gameItem.m_InstanceId = instanceId;

            m_Instances.Add(instanceId, gameItem);

            return instanceId;
        }

        /// <summary>
        /// Unregisters the <paramref name="gameItem"/>.
        /// </summary>
        /// <param name="gameItem"></param>
        internal static void Unregister(GameItem gameItem)
        {
            if(gameItem == null) return;
            m_Instances.Remove(gameItem.instanceId);
        }

        /// <summary>
        /// Looks up GameItem for specified <paramref name="instanceId"/>.
        /// </summary>
        /// <param name="instanceId">The GameItem's instance id to look up.</param>
        /// <returns>GameItem previously registered with specified
        /// <paramref name="instanceId"/>.</returns>
        public static GameItem GetInstance(int instanceId)
        {
            m_Instances.TryGetValue(instanceId, out var gameItem);
            return gameItem;
        }
    }
}
