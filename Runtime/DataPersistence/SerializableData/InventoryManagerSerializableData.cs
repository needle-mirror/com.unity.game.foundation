using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Serializable data structure that contains the state of runtime inventories.
    /// </summary>
    [Serializable]
    public class InventoryManagerSerializableData : ISerializableData
    {
        [SerializeField] int m_Version = 0;
        [SerializeField] InventorySerializableData[] m_Inventories = null;

        /// <summary>
        /// Return the data of all runtime inventories
        /// </summary>
        public InventorySerializableData[] inventories
        {
            get { return m_Inventories; }
        }
        
        /// <summary>
        /// Basic constructor that takes in an array of InventoryData of all runtime inventories.
        /// </summary>
        /// <param name="version">The version of the save schematic</param>
        /// <param name="inventories">The InventoryData array the RuntimeInventoryCatalogData is based of.</param>
        public InventoryManagerSerializableData(int version, InventorySerializableData[] inventories)
        {
            m_Version = version;
            m_Inventories = inventories;
        }

        /// <summary>
        /// Default constructor for serialization purpose.
        /// </summary>
        public InventoryManagerSerializableData()
        {
        }
    }
}