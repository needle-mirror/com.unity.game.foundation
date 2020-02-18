namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Represents a single item type and quantity to give for a payout.
    /// </summary>
    public struct OutputItem
    {
        /// <summary>
        /// The id of the inventory item used in this output item.
        /// </summary>
        public string id { get; }

        /// <summary>
        /// Get the payout quantity.
        /// </summary>
        public int quantity { get; }

        /// <summary>
        /// Returns the id of the destination inventory to dispense items into
        /// </summary>
        public string destinationInventoryId { get; }
        
        /// <summary>
        /// Standard constructor for an output item, takes an id, quantity, and optional destination inventory.
        /// </summary>
        /// <param name="id">The id of the item this gives.</param>
        /// <param name="quantity">The quantity of the item to give.</param>
        /// <param name="destinationInventoryId">The override inventory to try and put the item into.</param>
        public OutputItem(string id, int quantity, string destinationInventoryId = null)
        {
            this.id = id;
            this.quantity = quantity;
            this.destinationInventoryId = destinationInventoryId;
        }

        /// <summary>
        /// Constructor for creating a clone with the same information as another OutputItem.
        /// </summary>
        /// <param name="other">The other output item to copy information from.</param>
        public OutputItem(OutputItem other)
        {
            id = other.id;
            quantity = other.quantity;
            destinationInventoryId = other.destinationInventoryId;
        }

        /// <summary>
        /// Outputs all main data of the output item into a single string.
        /// TODO: Definitely may want to improve this formatting once we're ready to release this.
        /// </summary>
        /// <returns>As above.</returns>
        public override string ToString()
        {
            string data = id + "," + quantity;
            if (!string.IsNullOrEmpty(destinationInventoryId))
            {
                data += "," + destinationInventoryId;
            }

            return data;
        }
    } 
}
