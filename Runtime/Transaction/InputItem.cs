namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Represents a single item type and quantity to be used in a price.
    /// </summary>
    public struct InputItem
    {
        /// <summary>
        /// The id of the inventory item used in this input item.
        /// </summary>
        public string id { get; }

        /// <summary>
        /// Gets the price (IAP) of the item or the quantity (InventoryItem) of the cost of this item.
        /// </summary>
        public double price { get; }

        /// <summary>
        /// Returns the id of the source inventory to look for items inside.
        /// </summary>
        public string sourceInventoryOverride { get; }

        /// <summary>
        /// Construct a transaction price requirement. Pays from Wallet by default.
        /// </summary>
        /// <param name="price">price of item</param>
        /// <param name="transactionType">type of payment</param>
        /// <param name="id">definition id of item</param>
        /// <param name="sourceInventoryOverride">override inventory of detail specified inventory</param>
        public InputItem(string id, double price, string sourceInventoryOverride = null)
        {
            this.id = id;
            this.price = price;
            this.sourceInventoryOverride = sourceInventoryOverride;
        }

        /// <summary>
        /// Constructor for creating a clone with the same information as another InputItem.
        /// </summary>
        /// <param name="other">The other input item to copy information from.</param>
        public InputItem(InputItem other)
        {
            id = other.id;
            price = other.price;
            sourceInventoryOverride = other.sourceInventoryOverride;
        }

        /// <summary>
        /// Outputs all main data of the input item into a single string.
        /// TODO: Definitely may want to improve this formatting once we're ready to release this.
        /// </summary>
        /// <returns>As above.</returns>
        public override string ToString()
        {
            string data = id + "," + price;
            if (!string.IsNullOrEmpty(sourceInventoryOverride))
            {
                data += "," + sourceInventoryOverride;
            }

            return data;
        }
    }
}
