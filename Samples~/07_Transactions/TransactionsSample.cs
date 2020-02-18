using System.Collections.Generic;
using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    public class TransactionsSample : MonoBehaviour
    {
        private bool m_WrongDatabase;

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;
        
        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// References to the specific blend buttons to enable/disable when either action is not possible.
        /// </summary>
        public Button blendFruitButton;
        public Button blendVeggieButton;

        /// <summary>
        /// We will want to hold onto a reference to the inventory we're using for easy use later.
        /// </summary>
        private Inventory m_ShoppingCart;

        /// <summary>
        /// The transaction detail contains the information of the transaction itself.
        /// </summary>
        private PurchasableDetailDefinition m_SmoothieTransaction;

        private TransactionReceipt m_Receipt;
        
        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        void Start()
        {
            // The database has been properly setup.
            m_WrongDatabase = !SamplesHelper.VerifyDatabase();
            if (m_WrongDatabase)
            {
                wrongDatabasePanel.SetActive(true);
                return;
            }

            // Initialize must always be called before working with any game foundation code.
            GameFoundation.Initialize(new MemoryDataLayer());
            
            // Grab a reference to the shopping cart inventory.
            m_ShoppingCart = InventoryManager.GetInventory("shoppingCart");
            
            // Grab a reference to the smoothie purchasable detail.
            m_SmoothieTransaction = CatalogManager.inventoryCatalog.GetItemDefinition("smoothie").GetDetailDefinition<PurchasableDetailDefinition>();
            
            // Here we bind our UI refresh method to callbacks on the inventory manager.
            // These callbacks will automatically be invoked anytime an inventory is added, or removed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            // For this sample, we'll only refresh the UI when a smoothie gets added to the shopping cart.
            m_ShoppingCart.onItemAdded += RefreshUI;
            m_ShoppingCart.onItemQuantityChanged += RefreshUI;
            
            RefreshUI();
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        /// <param name="item">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void RefreshUI(InventoryItem item = null)
        {
            // Display the shopping cart's display name
            mainText.text = "Inventory - " + m_ShoppingCart.displayName + "\n";
            
            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in m_ShoppingCart.GetItems())
            {
                // All game items have an associated display name, this includes game items.
                string itemName = inventoryItem.displayName;
            
                // Every inventory item has an associated quantity. This represents how many units of this item there are within the inventory.
                int quantity = inventoryItem.quantity;
            
                mainText.text += itemName + ": " + quantity + "\n";
            }
            
            RefreshBlendButtons();
        }

        /// <summary>
        /// This will attempt to make a transaction using the fruit input items.
        /// The default price is used for GetPrice if no id is provided. Each detail always has the same payout.
        /// We use the shopping cart as both the input and output inventory.
        /// Finally, we use a lambda to retrieve the receipt of this transaction. The receipt contains various pieces of useful information such as the time of the transaction, if it succeeded, etc.
        /// </summary>
        public void BlendFruitSmoothie()
        {
            TransactionReceipt receipt;
            TransactionManager.HandleTransaction(m_SmoothieTransaction.GetPrice(), m_SmoothieTransaction.payout, m_ShoppingCart, m_ShoppingCart, transactionReceipt => { receipt = transactionReceipt;} );
        }

        /// <summary>
        /// This will attempt to make a transaction using the veggie input items.
        /// Here we provide a specific id for the price, it will use the "Veggie" price we setup which is how it uses the vegetable inputs. The payout remains the same, a single smoothie.
        /// We use the shopping cart as both the input and output inventory.
        /// In this case, we use the premade helper method rather than setup a new lambda. You can also pass it in to the onTransactionFailed callback if you want the receipt for failed situations.
        /// </summary>
        public void BlendVeggieSmoothie()
        {
            TransactionManager.HandleTransaction(m_SmoothieTransaction.GetPrice("Veggie"), m_SmoothieTransaction.payout, m_ShoppingCart, m_ShoppingCart, GetReceipt);
        }

        /// <summary>
        /// Simple method that stores the given receipt into our local receipt variable.
        /// Will primarily be used in the veggie transaction callback.
        /// </summary>
        /// <param name="receipt">The receipt to assign, will be generated by HandleTransaction.</param>
        private void GetReceipt(TransactionReceipt receipt)
        {
            m_Receipt = receipt;
        }

        /// <summary>
        /// Enables/Disables the blend buttons
        /// Each button should verify there are enough ingredients for what it needs.
        /// </summary>
        private void RefreshBlendButtons()
        {
            blendFruitButton.interactable = m_ShoppingCart.ContainsItem("apple") && m_ShoppingCart.GetQuantity("apple") >= 1 &&
                                            m_ShoppingCart.ContainsItem("orange") && m_ShoppingCart.GetQuantity("orange") >= 1 &&
                                            m_ShoppingCart.ContainsItem("banana") && m_ShoppingCart.GetQuantity("banana") >= 1;

            blendVeggieButton.interactable = m_ShoppingCart.ContainsItem("carrot") && m_ShoppingCart.GetQuantity("carrot") >= 1 &&
                                              m_ShoppingCart.ContainsItem("broccoli") && m_ShoppingCart.GetQuantity("broccoli") >= 1;
        }
    }
}
