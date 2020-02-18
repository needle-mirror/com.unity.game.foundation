using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of a Stat.
    /// </summary>
    [Serializable]
    public struct StatSerializableData
    {
        /// <summary>
        /// The id of the stat in the dictionary.
        /// </summary>
        public ulong statDictionaryId;

        /// <summary>
        /// The serialized data of the stat.
        /// </summary>
        public StatItemSerializableData statItem;
    }
}
