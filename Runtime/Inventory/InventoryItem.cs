namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// An InventoryItem that goes into an Inventory. InventoryItems should only exist inside Inventories. 
    /// </summary>
    /// <inheritdoc/>
    public class InventoryItem : BaseItem<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        /// <summary>
        /// Basic constructor that takes in a reference to the InventoryItemDefinition that this is based on.
        /// </summary>
        /// <param name="itemDefinition">The InventoryItemDefinition this item is based on.</param>
        internal InventoryItem(InventoryItemDefinition itemDefinition, Inventory owner, int gameItemId = 0)
            : base(itemDefinition, owner, gameItemId) { }

        /// <summary>
        /// The Inventory that this InventoryItem belongs to.
        /// </summary>
        /// <returns>The Inventory that this InventoryItem belongs to.</returns>
        public Inventory inventory
        {
            get
            {
                LogWarningIfDisposed();
                return owner;
            }
        }

        /// <summary>
        /// Quantity contained in this Inventory for this InventoryItem.
        /// </summary>
        /// <returns>Quantity contained in this Inventory for this InventoryItem.</returns>
        public int quantity
        {
            get
            {
                LogWarningIfDisposed();
                return intValue;
            }
            set
            {
                LogWarningIfDisposed();
                SetQuantity(value);
            }
        }

        /// <inheritdoc />
        public override int intValue
        {
            get => base.intValue;
            internal set
            {
                base.intValue = value;

                //Owner can now be null since the catalog runtime/editor separation
                if (owner != null)
                {
                    InventoryManager.SyncSetItemQuantity(
                        owner.id,
                        definitionId,
                        value,
                        gameItemId);
                }
            }
        }

        /// <summary>
        /// Sets the quantity of this InventoryItem.
        /// </summary>
        /// <param name="value">The new quantity for specified InventoryItemDefinition.</param>
        public void SetQuantity(int value)
        {
            LogWarningIfDisposed();

            if (inventory != null)
            {
                inventory.SetQuantity(hash, value);
            }
            else
            {
                intValue = value;
            }
        }

        /// <summary>
        /// Synchronize this item deletion with the data access layer.
        /// </summary>
        protected override void CustomDiscard()
        {
            //Do not sync if the owner is already discarded because discarding a collection already discard all its items.
            if (!owner.discarded
                && InventoryManager.IsInitialized)
            {
                //Use owner instead of inventory to avoid calling LogWarningIfDisposed.
                InventoryManager.SyncDeleteItem(owner.id, definitionId);
            }
        }
    }
}
