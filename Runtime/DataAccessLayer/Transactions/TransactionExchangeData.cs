using UnityEngine.GameFoundation.DataPersistence;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Description of the result of a transaction.
    /// </summary>
    public struct TransactionExchangeData
    {
        /// <summary>
        /// The updated currencies
        /// </summary>
        public CurrencyExchangeData[] currencies;

        /// <summary>
        /// The created/removed items
        /// </summary>
        public InventoryItemSerializableData[] items;
    }
}
