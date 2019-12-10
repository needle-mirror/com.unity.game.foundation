using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for inventory basics.
    /// </summary>
    public class InventoryBasics : MonoBehaviour
    {
        private bool m_WrongDatabase;
        
        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

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
            GameFoundation.Initialize();
            
            // Here we bind our UI refresh method to callbacks on the main inventory.
            // These callbacks will automatically be invoked anytime an item is added, removed, or has its quantity changed within the main inventory.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            Inventory.main.onItemAdded += RefreshUI;
            Inventory.main.onItemRemoved += RefreshUI;
            Inventory.main.onItemQuantityChanged += RefreshUI;
            
            RefreshUI();
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        /// <param name="item">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void RefreshUI(InventoryItem item = null)
        {
            // Display the main inventory's display name
            mainText.text = "Inventory - " + Inventory.main.displayName + "\n";

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in Inventory.main.GetItems())
            {
                // All game items have an associated display name, this includes game items.
                string itemName = inventoryItem.displayName;

                // Every inventory item has an associated quantity. This represents how many units of this item there are within the inventory.
                int quantity = inventoryItem.quantity;

                mainText.text += itemName + ": " + quantity + "\n";
            }
        }
        

        /// <summary>
        /// Adds a single apple to the main inventory.
        /// If there is not an apple in the main inventory yet, it will create a new instance.
        /// If there is already an apple instance, it will increase its quantity by 1.
        /// </summary>
        public void AddApple()
        {
            Inventory.main.AddItem("apple", 1);
        }

        /// <summary>
        /// Removes a single apple from the main inventory.
        /// If there is not an apple in the main inventory, nothing will happen.
        /// If the apple instance in the inventory has a quantity of 1, this will remove it entirely from the inventory.
        /// If the apple instance in the inventory has a quantity > 1, it will simply decrement the quantity by 1.
        /// If an item's quantity is at or below 0 after a RemoveItem call, this method will automatically remove the instance from the inventory entirely.
        /// </summary>
        public void RemoveApple()
        {
            Inventory.main.RemoveItem("apple", 1);
        }

        /// <summary>
        /// Sets the amount of apples in the main inventory to be exactly 10.
        /// This can only be done if there is already an apple instance in the inventory.
        /// For this method, if there is no apple we will add 10 apples.
        /// </summary>
        public void TenApples()
        {
            // ContainsItem is a simple way to easily check if an item type is within an inventory.
            if (!Inventory.main.ContainsItem("apple"))
            {
                // We can initially add any amount of items we want.
                Inventory.main.AddItem("apple", 10);
            }
            else
            {
                Inventory.main.SetQuantity("apple", 10);
            }
        }
    }
}