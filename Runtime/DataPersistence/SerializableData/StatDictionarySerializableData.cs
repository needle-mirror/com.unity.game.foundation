using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of a StatDictionary.
    /// </summary>
    [Serializable]
    public struct StatDictionarySerializableData
    {
        /// <summary>
        /// The type of value of a StatDictionary
        /// </summary>
        public enum StatType
        {
            /// <summary>
            /// For stats of int type.
            /// </summary>
            Int,

            /// <summary>
            /// For stats of float type.
            /// </summary>
            Float
        }

        /// <summary>
        /// The type of value of the StatDictionary
        /// </summary>
        public StatType statType;

        /// <summary>
        /// Array of all serialized stats of the StatDictionary
        /// </summary>
        public StatSerializableData[] stats;
    }
}
