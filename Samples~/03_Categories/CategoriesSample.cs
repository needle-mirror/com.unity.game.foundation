using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for inventory basics.
    /// </summary>
    public class CategoriesSample : MonoBehaviour
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
        /// Reference to the buttons
        /// </summary>
        public Button[] buttons;

        private Inventory m_ShoppingCart;

        private string m_CurrentCategory;

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

            // Grab a reference to the shopping cart
            m_ShoppingCart = InventoryManager.GetInventory("shoppingCart");

            FruitCategory();
            RefreshUI();
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        /// <param name="item">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void RefreshUI(InventoryItem item = null)
        {
            // Display the main inventory's display name
            mainText.text = "Inventory - " + m_ShoppingCart.displayName + "\n\n";

            // Similar to GetItems, GetItemsByCategory will return a list of items, but only those that have the requested category assigned to them.
            InventoryItem[] items = m_ShoppingCart.GetItemsByCategory(m_CurrentCategory);

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in items)
            {
                // All game items have an associated display name, this includes game items.
                string itemName = inventoryItem.displayName;

                // Every inventory item has an associated quantity. This represents how many units of this item there are within the inventory.
                int quantity = inventoryItem.quantity;

                mainText.text += itemName + ": " + quantity + "\n";
            }
        }

        /// <summary>
        /// Set the current category to be fruit.
        /// </summary>
        public void FruitCategory()
        {
            m_CurrentCategory = "fruit";

            UnselectAllButtons();
            buttons[0].interactable = false;

            RefreshUI();
        }

        /// <summary>
        /// Set the current category to be blank.
        /// </summary>
        public void FoodCategory()
        {
            m_CurrentCategory = "food";

            UnselectAllButtons();
            buttons[1].interactable = false;

            RefreshUI();
        }

        /// <summary>
        /// Set the current category to be vegetable.
        /// </summary>
        public void VegetableCategory()
        {
            m_CurrentCategory = "vegetable";

            UnselectAllButtons();
            buttons[2].interactable = false;

            RefreshUI();
        }

        private void UnselectAllButtons()
        {
            foreach (var button in buttons)
            {
                button.interactable = true;
            }
        }
    }
}
