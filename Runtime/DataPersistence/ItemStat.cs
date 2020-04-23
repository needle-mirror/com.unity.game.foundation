using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public struct ItemStat
    {
        /// <summary>
        /// Id of the stat.
        /// </summary>
        public string statId;

        /// <summary>
        /// Value of the stat.
        /// </summary>
        public StatValue value;
    }
}