using System.Collections.Generic;
using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the wallet and store sample.
    /// </summary>
    public class WalletSample : MonoBehaviour
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
        /// References to the specific buy/sell buttons to enable/disable when either action is not possible.
        /// </summary>
        public Button buyButton;
        public Button sellButton;

        /// <summary>
        /// We will want to hold onto reference to inventories for easy use later.
        /// </summary>
        private Inventory m_Wallet;
        private Inventory m_Store;
        private Inventory m_Main;

        /// <summary>
        /// Used to determine how many coins an apple will cost.
        /// </summary>
        private int m_ApplePrice;

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

            // - Initialize must always be called before working with any game foundation code.
            // - GameFoundation requires an IDataAccessLayer object that will provide and persist
            //   the data required for the various services (Inventory, Stats, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            GameFoundation.Initialize(new MemoryDataLayer());

            // Grab a reference to the main, wallet, and store inventories.
            m_Main = Inventory.main;
            m_Wallet = InventoryManager.wallet;
            m_Store = InventoryManager.GetInventory("store");

            // Here we bind our UI refresh method to callbacks on the inventory manager.
            // These callbacks will automatically be invoked anytime an inventory is added, or removed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            m_Wallet.onItemAdded += RefreshUI;
            m_Wallet.onItemRemoved += RefreshUI;
            m_Wallet.onItemQuantityChanged += RefreshUI;

            m_ApplePrice = Random.Range(5, 26);
            RefreshUI();
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        /// <param name="item">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void RefreshUI(InventoryItem item = null)
        {
            // Show the current price of an apple
            mainText.text = "Apple Price: " + m_ApplePrice + "\n\n";

            // We only care about displaying the wallet, the main inventory, and the store.
            List<Inventory> inventories = new List<Inventory>();
            inventories.Add(m_Wallet);
            inventories.Add(m_Main);
            inventories.Add(m_Store);

            // Loop through the inventories we want to display
            foreach (Inventory inventory in inventories)
            {
                // Display the main inventory's display name
                mainText.text += "Inventory - " + inventory.displayName + "\n";

                // Loop through every type of item within the inventory and display its name and quantity.
                foreach (InventoryItem inventoryItem in inventory.GetItems())
                {
                    // All game items have an associated display name, this includes game items.
                    string itemName = inventoryItem.displayName;

                    // Every inventory item has an associated quantity. This represents how many units of this item there are within the inventory.
                    int quantity = inventoryItem.quantity;

                    mainText.text += itemName + ": " + quantity + "\n";
                }

                // Display a separator between inventories
                mainText.text += "\n";
            }
        }

        /// <summary>
        /// This method does a price check to make sure there are enough coins in the wallet to buy an apple.
        /// If so, it will add an apple to the main inventory, remove one to the store, and deduct the value from the wallet.
        /// It will also refresh to the apple price to keep things interesting.
        /// Because we never use RemoveItem in this sample, we don't have to worry about safety checking everything.
        /// </summary>
        public void BuyApple()
        {
            var coin = m_Wallet.GetItem("coin");
            if (coin.quantity >= m_ApplePrice)
            {
                m_Main.GetItem("apple").quantity++;
                m_Store.GetItem("apple").quantity--;
                coin.quantity -= m_ApplePrice;
                RefreshBuySelllButtons();
            }
        }

        /// <summary>
        /// This method makes sure there is at least 1 apple to sell.
        /// If so, it will remove an apple from the main inventory, add one to the store, and add the value to the wallet.
        /// It will also refresh to the apple price to keep things interesting.
        /// Because we never use RemoveItem in this sample, we don't have to worry about safety checking everything.
        /// </summary>
        public void SellApple()
        {
            var apple = m_Main.GetItem("apple");
            if (apple.quantity >= 1)
            {
                apple.quantity--;
                m_Store.GetItem("apple").quantity++;
                m_Wallet.GetItem("coin").quantity += m_ApplePrice;
                RefreshBuySelllButtons();
            }
        }

        /// <summary>
        /// Enables/Disables the buy/sell buttons.
        /// Can only buy more apples if you have enough coins.
        /// Can only sell apples if you have at least 1 apple to sell.
        /// </summary>
        private void RefreshBuySelllButtons()
        {
            buyButton.interactable = m_Wallet.ContainsItem("coin") && m_Wallet.GetQuantity("coin") >= m_ApplePrice;
            sellButton.interactable = m_Main.ContainsItem("apple") && m_Main.GetQuantity("apple") >= 1;
        }
    }
}
