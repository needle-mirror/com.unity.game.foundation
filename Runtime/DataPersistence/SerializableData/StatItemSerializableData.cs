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
        /// The id of the item the stat is linked to.
        /// </summary>
        public string itemId;

        /// <summary>
        /// List of the item stats with their value.
        /// </summary>
        public ItemStat[] stats;

        /// <summary>
        /// Initializes an instance from a <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source data.</param>
        public StatItemSerializableData(StatItemSerializableData source)
        {
            itemId = source.itemId;
            stats = Tools.ToArray(source.stats);
        }
    }
}
