namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describe the item change of a <see cref="TransactionRewards"/>.
    /// </summary>
    public struct ItemExchangeDefinition
    {
        /// <summary>
        /// The item of the exchange.
        /// </summary>
        public InventoryItemDefinition item { get; internal set; }

        /// <summary>
        /// The amount of the <see cref="item"/>.
        /// As a reward, the amount is added to the inventory.
        /// As a cost, the amount is removed from the inventory.
        /// </summary>
        public long amount { get; internal set; }
    }
}
