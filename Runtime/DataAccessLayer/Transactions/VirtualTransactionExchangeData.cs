namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Description of the result of a virtual transaction.
    /// </summary>
    public struct VirtualTransactionExchangeData
    {
        /// <summary>
        /// Description of the cost
        /// </summary>
        public TransactionExchangeData cost;

        /// <summary>
        /// Description of the rewards
        /// </summary>
        public TransactionExchangeData rewards;
    }
}