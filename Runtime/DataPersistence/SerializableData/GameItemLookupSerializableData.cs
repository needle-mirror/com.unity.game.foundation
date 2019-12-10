using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Serializable data structure that contains the state of GameItemLookup.
    /// </summary>
    [Serializable]
    public class GameItemLookupSerializableData : ISerializableData
    {
        [SerializeField] int m_LastGameItemIdUsed;
        
        /// <summary>
        /// The last guid used by the GameItemLookup class
        /// </summary>
        public int lastGameItemIdUsed
        {
            get { return m_LastGameItemIdUsed; }
        }

        /// <summary>
        /// Basic constructor that takes in the version of the persistence layer and the index of the last GUID used by the GameItemLookup.
        /// </summary>
        /// <param name="lastGameItemIdUsed"></param>
        public GameItemLookupSerializableData(int lastGameItemIdUsed)
        {
            m_LastGameItemIdUsed = lastGameItemIdUsed;
        }
        
        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public GameItemLookupSerializableData()
        {
        }
    }
}