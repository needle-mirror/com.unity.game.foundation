using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of the StatManager.
    /// </summary>
    [Serializable]
    public struct StatManagerSerializableData
    {
        /// <summary>
        /// Get an empty instance of this class.
        /// </summary>
        public static StatManagerSerializableData Empty => new StatManagerSerializableData
        {
            items = new StatItemSerializableData[0]
        };

        /// <summary>
        /// Return the data of all runtime stat dictionaries
        /// </summary>
        public StatItemSerializableData[] items;
    }
}
