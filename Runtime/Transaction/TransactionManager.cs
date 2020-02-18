namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contains the main helper methods used for initiating and processing transactions.
    /// </summary>
    public class TransactionManager
    {
        private static long k_TransactionIdCounter = 0;
        
        /// <summary>
        /// Simple callback delegate
        /// </summary>
        public delegate void TransactionEvent(TransactionReceipt receipt);

        /// <summary>
        /// Handles a single transaction with a single price and single payout.
        /// </summary>
        /// <param name="price">Contains information for the cost of this transaction.</param>
        /// <param name="payout">Contains information for the outcome of this transaction.</param>
        /// <param name="sourceInventory">The inventory to read from and eventually deduct items from.</param>
        /// <param name="destinationInventory">The inventory to place outputs to.</param>
        /// <param name="onTransactionSuccess">Callback for a successful transaction.</param>
        /// <param name="onTransactionFail">Callback for a failed transaction.</param>
        /// <returns></returns>
        public static long HandleTransaction(Price price, Payout payout, Inventory sourceInventory = null, Inventory destinationInventory = null,
            TransactionEvent onTransactionSuccess = null, TransactionEvent onTransactionFail = null)
        {
            // Default to main inventory
            if (sourceInventory == null)
            {
                sourceInventory = InventoryManager.main;
            }
            if (destinationInventory == null)
            {
                // Default to main inventory
                destinationInventory = InventoryManager.main;
            }
            
            // Verify the contents of the price are in the input inventory
            if (!CheckAndDeductInputItems(price, sourceInventory, false))
            {
                k_TransactionIdCounter++;
                TransactionReceipt failedReceipt = new TransactionReceipt(k_TransactionIdCounter, false,
                    "Input inventory did not have enough required input items.",
                    sourceInventory.id, destinationInventory.id, price, payout);
                
                onTransactionFail?.Invoke(failedReceipt);
                
                return k_TransactionIdCounter;
            }
            
            // Should have enough of the price, deduct it from the input inventory
            CheckAndDeductInputItems(price, sourceInventory, true);
            
            // Payout to the destination inventory
            foreach (OutputItem outputItem in payout.GetPayoutItems())
            {
                // Individual items may want to output to specific inventories, check for that here.
                Inventory itemInventory = destinationInventory;
                string outputId = outputItem.destinationInventoryId;
                if (!string.IsNullOrEmpty(outputId) && InventoryManager.GetInventory(outputId) != null)
                {
                    itemInventory = InventoryManager.GetInventory(outputId);
                }
                
                // Increase the items quantity if it already exists, or add a new instance with the given quantity.
                string outputItemId = outputItem.id;
                if (itemInventory.ContainsItem(outputItemId))
                {
                    itemInventory.GetItem(outputItemId).quantity += outputItem.quantity;
                }
                else
                {
                    itemInventory.AddItem(outputItemId).quantity = outputItem.quantity;
                }
            }
            
            // Success!
            k_TransactionIdCounter++;
            TransactionReceipt successReceipt = new TransactionReceipt(k_TransactionIdCounter, true, "NA",
                sourceInventory.id, destinationInventory.id, price, payout);
            
            onTransactionSuccess?.Invoke(successReceipt);
            payout.InvokePayoutEvent();
            
            return k_TransactionIdCounter;
        }

        /// <summary>
        /// Helper method that will be used for checking if an input inventory has the items on the given price.
        /// Can also be used to perform the actual deduction.
        /// </summary>
        /// <param name="price">The price struct to read information from.</param>
        /// <param name="sourceInventory">The inventory to look in.</param>
        /// <param name="deductQuantity">Whether or not we want to "pay" with the items right now.</param>
        /// <returns>If the given inventory contains the price of the price.</returns>
        private static bool CheckAndDeductInputItems(Price price, Inventory sourceInventory, bool deductQuantity)
        {
            // Verify the contents of the price are in the input inventory
            foreach (InputItem inputItem in price.GetInputItems())
            {
                // Individual items may want to check specific inventories, check for that here.
                Inventory itemInventory = sourceInventory;
                string inputId = inputItem.sourceInventoryOverride;
                if (!string.IsNullOrEmpty(inputId) && InventoryManager.GetInventory(inputId) != null)
                {
                    itemInventory = InventoryManager.GetInventory(inputId);
                }
                
                // Verify it contains an instance at all
                string inputItemId = inputItem.id;
                if (!itemInventory.ContainsItem(inputItemId))
                {
                    return false;
                }
            
                // Verify that the instance has enough of the quantity
                if (itemInventory.GetItem(inputItemId).quantity < inputItem.price)
                {
                    return false;
                }
                
                // If we want to modify the contents, do so now
                if (deductQuantity)
                {
                    // Deduct the item's quantity and remove the instance if it goes to or below 0.
                    var item = itemInventory.GetItem(inputItem.id);
                    item.quantity -= (int) inputItem.price;
                    if (item.quantity <= 0)
                    {
                        itemInventory.RemoveItem(item.id);
                    }
                }
            }

            return true;
        }
    }
}
