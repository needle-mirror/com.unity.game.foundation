namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This class is the base of the <see cref="VirtualTransaction"/> and <see cref="IAPTransaction"/> classes.
    /// Warning: Custom implementations of this base class are not supported by TransactionManager.
    /// </summary>
    public abstract class BaseTransaction : CatalogItem
    {
        /// <summary>
        /// Describes the rewards of the transaction. 
        /// </summary>
        public TransactionExchangeDefinition rewards { get; internal set; }
    }
}
