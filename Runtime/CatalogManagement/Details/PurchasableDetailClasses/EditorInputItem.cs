using System;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    [Serializable]
    public class EditorInputItem
    {
        /// <summary>
        /// Types of supported price transaction types
        /// </summary>
        public enum TransactionType
        {
            InventoryItem
        }

        [SerializeField]
        private TransactionType m_TransactionType;

        /// <summary>
        /// Get the type of transaction for this price requirement
        /// </summary>
        internal TransactionType transactionType { get => m_TransactionType; }

        [SerializeField]
        private string m_DefinitionId;

        /// <summary>
        /// Get the item definition ids relevant to this price requirement
        /// </summary>
        public string definitionId => m_DefinitionId;

        [SerializeField]
        private double m_Price;
        /// <summary>
        /// Gets the price (IAP) of the item or the quantity (InventoryItem) of the cost of this item.
        /// </summary>
        public double price => m_Price;

        [SerializeField]
        private string m_SourceInventoryId;
        /// <summary>
        /// Returns the id of the source inventory to look for items inside.
        /// </summary>
        public string sourceInventoryId => m_SourceInventoryId;

        /// <summary>
        /// Construct a transaction price requirement. Pays from Wallet by default.
        /// </summary>
        /// <param name="price">price of item</param>
        /// <param name="transactionType">type of payment</param>
        /// <param name="definitionId">definition id of item</param>
        /// <param name="sourceInventoryId">override inventory of detail specified inventory</param>
        public EditorInputItem(string definitionId, double price, TransactionType transactionType = TransactionType.InventoryItem, string sourceInventoryId = null)
        {
            m_DefinitionId = definitionId;
            m_Price = price;
            m_TransactionType = transactionType;
            m_SourceInventoryId = sourceInventoryId;
        }

        /// <summary>
        /// Outputs all main data of the input item into a single string.
        /// TODO: Definitely may want to improve this formatting once we're ready to release this.
        /// </summary>
        /// <returns>As above.</returns>
        public override string ToString()
        {
            string data = transactionType + "," + definitionId + "," + price;
            if (!string.IsNullOrEmpty(sourceInventoryId))
            {
                data += "," + sourceInventoryId;
            }

            return data;
        }
    }
}
