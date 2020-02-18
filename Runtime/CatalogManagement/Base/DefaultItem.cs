namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// This class contains a pair of data, a GameItemDefinition Hash and an int for quantity.
    /// These are used to setup the DefaultItems in a CollectionDefinition.
    /// </summary>
    [System.Serializable]
    public class DefaultItem
    {
        [SerializeField]
        private int m_DefinitionHash;

        /// <summary>
        /// The Hash of the GameItemDefinition to use.
        /// </summary>
        public int definitionHash
        {
            get
            {
                if (m_DefinitionHash == 0)
                {
                    m_DefinitionHash = Tools.StringToHash(m_DefinitionId);
                }
                return m_DefinitionHash;
            }
        }

        [SerializeField]
        private string m_DefinitionId;

        /// <summary>
        /// The ID the GameItemDefinition to use.
        /// </summary>
        public string definitionId
        {
            get { return m_DefinitionId; }
        }

        [SerializeField]
        private int m_Quantity;

        /// <summary>
        /// The starting quantity of the Item instance once setup.
        /// </summary>
        public int quantity
        {
            get { return m_Quantity; }
            set { m_Quantity = value; }
        }

        /// <summary>
        /// Basic constructor that takes in fields for all variables this class uses.
        /// </summary>
        /// <param name="definitionId">The id of the GameItemDefinition to use.</param>
        /// <param name="quantity">The starting quantity of this Item.</param>
        public DefaultItem(string definitionId, int quantity)
        {
            Tools.ThrowIfPlayMode("Cannot create a DefaultItem while in play mode.");

            m_DefinitionId = definitionId;
            m_DefinitionHash = Tools.StringToHash(definitionId);
            m_Quantity = quantity;
        }
    }
}
