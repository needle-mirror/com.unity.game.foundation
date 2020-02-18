using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of a StatItem.
    /// </summary>
    [Serializable]
    public struct StatItemSerializableData
    {
        /// <summary>
        /// The GameItem hash id of the stat.
        /// </summary>
        public int gameItemId;

        /// <summary>
        /// The definition id of the stat.
        /// </summary>
        public string definitionId;

        /// <summary>
        /// The int value of the stat when StatDictionary type is an int.
        /// </summary>
        public int intValue;

        /// <summary>
        /// The default int value of the stat when StatDictionary type is an int.
        /// </summary>
        public int defaultIntValue;

        /// <summary>
        /// The float value of the stat when StatDictionary type is a float.
        /// </summary>
        public float floatValue;

        /// <summary>
        /// The float value of the stat when StatDictionary type is a float.
        /// </summary>
        public float defaultFloatValue;
    }
}
