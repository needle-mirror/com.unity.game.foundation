namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// An InventoryItem that goes into an Inventory. InventoryItems should only exist inside Inventories. 
    /// </summary>
    /// <inheritdoc/>
    public class InventoryItem : GameItem
    {
        /// <summary>
        /// Basic constructor that takes in a reference to the InventoryItemDefinition that this is based on.
        /// </summary>
        /// <param name="itemDefinition">The InventoryItemDefinition this item is based on.</param>
        /// <param name="id">
        /// The unique identifier of this instance.
        /// If <c>null</c>, the constructor will create one itself.
        /// </param>
        internal InventoryItem(InventoryItemDefinition itemDefinition, string id = null)
            : base(itemDefinition, id) { }
    }
}
