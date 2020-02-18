using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of GameItemLookup.
    /// </summary>
    [Serializable]
    public struct GameItemLookupSerializableData
    {
        /// <summary>
        /// The last id used by the GameItemLookup class
        /// </summary>
        public int lastGameItemIdUsed;
    }
}
