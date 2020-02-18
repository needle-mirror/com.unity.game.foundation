using UnityEngine.GameFoundation.DataAccessLayers;
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

            // - Initialize must always be called before working with any game foundation code.
            // - GameFoundation requires an IDataAccessLayer object that will provide and persist
            //   the data required for the various services (Inventory, Stats, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            GameFoundation.Initialize(new MemoryDataLayer());

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
            // It's a good idea to perform a safety check by using ContainsItem when getting items before performing operations on them.
            if (Inventory.main.ContainsItem("apple"))
            {
                Inventory.main.GetItem("apple").quantity++;
            }
            else
            {
                // This will happen if there is no apple in the inventory yet. If that is the case, add one.
                Inventory.main.AddItem("apple");
            }
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
            // If you don't want quantity to drop into a negative value, it'll be a good idea to check the quantity first.
            // It's also more efficient to use GetItem and do a null check rather than use ContainsItem and then GetItem.
            // This lowers the amount of times the inventory system needs to check its dictionary.
            var apple = Inventory.main.GetItem("apple");
            if (apple != null && apple.quantity > 0)
            {
                apple.quantity--;
            }
            else
            {
                Inventory.main.RemoveItem("apple");
            }
        }

        /// <summary>
        /// Sets the amount of apples in the main inventory to be exactly 10.
        /// This can only be done if there is already an apple instance in the inventory.
        /// For this method, if there is no apple we will add 10 apples.
        /// </summary>
        public void TenApples()
        {
            // Add item will create a new instance if need be and return that, or return a reference to the one already in the inventory.
            // In both cases, we want to set the quantity to 10.
            Inventory.main.AddItem("apple").SetQuantity(10);
        }
    }
}
