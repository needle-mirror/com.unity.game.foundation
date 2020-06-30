using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.GameFoundation.DefaultLayers;
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
        /// Flag for whether the Inventory has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_InventoryChanged;

        /// <summary>
        /// Reference to a list of InventoryItems in the InventoryManager.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        /// Reference to a list of InventoryItems of a certain tag.
        /// </summary>
        private readonly List<InventoryItem> m_ItemsByTag = new List<InventoryItem>();
        
        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

        /// <summary>
        /// References to the remove buttons to enable/disable when the action is not possible.
        /// </summary>
        public Button removeAppleButton;
        public Button removeOrangeButton;
        public Button removeAllApplesButton;
        public Button removeAllOrangesButton;

        /// <summary>
        /// Standard starting point for Unity scripts.
        /// </summary>
        private void Start()
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
            //   the data required for the various services (Inventory, Wallet, ...).
            // - For this sample we don't need to persist any data so we use the MemoryDataLayer
            //   that will store GameFoundation's data only for the play session.
            GameFoundation.Initialize(new MemoryDataLayer());

            // Here we bind a listener that will set an inventoryChanged flag to callbacks on the Inventory Manager.
            // These callbacks will automatically be invoked anytime an item is added or removed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            InventoryManager.itemAdded += OnInventoryItemChanged;
            InventoryManager.itemRemoved += OnInventoryItemChanged;

            // The Inventory Manager starts with initial allocation of 2 apples and 1 orange, but we 
            // can add an additional orange here to get us started.
            InventoryManager.CreateItem("orange");

            RefreshUI();
        }

        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when something has changed in the InventoryManager (either items were added or removed)
            if (m_InventoryChanged)
            {
                RefreshUI();
                m_InventoryChanged = false;
            }
        }

        /// <summary>
        /// Adds a single apple to the main inventory.
        /// </summary>
        public void AddFruit(string fruitItemDefinitionId)
        {
            try
            {
                // This will create a new item inside the InventoryManager, if the definitionId exists in the inventory catalog.
                // Because this method will throw an exception if the definitionId is not found in the inventory catalog, we'll surround
                // in a try catch and log any exceptions thrown.
                InventoryManager.CreateItem(fruitItemDefinitionId);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Removes a single apple from the main inventory.
        /// </summary>
        public void RemoveFruit(string fruitItemDefinitionId)
        {
            // To remove a single item from the InventoryManager, you need a specific instance of that item. Since we only know the 
            // InventoryItemDefinition id of the item we want to remove, we'll first look for all items with that definition.
            // We'll use the version of FindItemsByDefinition that lets us pass in a collection to be filled to reduce allocations.
            InventoryManager.FindItemsByDefinition(fruitItemDefinitionId, m_ItemsByTag);
            
            // Make sure there actually is an item available to return
            if (m_ItemsByTag.Count > 0)
            {
                // We'll remove the first instance in the array of apple items
                InventoryManager.RemoveItem(m_ItemsByTag[0]);
                // Once we remove the item from the InventoryManager, the reference to it will be broken.
                m_ItemsByTag[0] = null;
            }
        }

        /// <summary>
        /// Removes all instances of apple InventoryItemDefinition from the Inventory.
        /// </summary>
        public void RemoveAllFruitType(string fruitItemDefinitionId)
        {
            // This method can be called whether or not there are any items with this definition id in the InventoryManager.
            // If there are no items with that definition id, the method will take no action and return a count of 0;
            int itemsRemovedCount = InventoryManager.RemoveItemsByDefinition(fruitItemDefinitionId);
            
            Debug.Log($"{itemsRemovedCount} {fruitItemDefinitionId} item(s) removed from inventory.");
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            m_DisplayText.Append("<b><i>Inventory:</i></b>");
            m_DisplayText.AppendLine();
            
            // We'll use the version of GetItems that lets us pass in a collection to be filled to reduce allocations
            InventoryManager.GetItems(m_InventoryItems);

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in m_InventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                string itemName = inventoryItem.definition.displayName;

                m_DisplayText.Append(itemName);
                m_DisplayText.AppendLine();
            }

            mainText.text = m_DisplayText.ToString();

            RefreshRemoveButtons();
        }

        /// <summary>
        /// Enables/Disables the remove item buttons.
        /// The addButtons will always be interactable,
        /// but we only want to allow removing items if we have some to remove.
        /// </summary>
        private void RefreshRemoveButtons()
        {
            // FindItemsByDefinition will return a count of the number of items found no matter if you pass in a list or null
            // Since we don't actually care about the list, only the account, we'll save the allocation and just pass null
            var appleCount = InventoryManager.FindItemsByDefinition("apple", null);
            var orangeCount = InventoryManager.FindItemsByDefinition("orange", null);

            removeAppleButton.interactable = removeAllApplesButton.interactable = appleCount > 0;
            removeOrangeButton.interactable = removeAllOrangesButton.interactable = orangeCount > 0;
        }

        /// <summary>
        /// Listener for changes in InventoryManager. Will get called whenever an item is added or removed.
        /// Because many items can get added or removed at a time, we will have the listener only set a flag
        /// that changes exist, and on our next update, we will check the flag to see whether changes to the UI
        /// need to be made.
        /// </summary>
        /// <param name="itemChanged">This parameter will not be used, but must exist so the signature is compatible with the inventory callbacks so we can bind it.</param>
        private void OnInventoryItemChanged(InventoryItem itemChanged)
        {
            m_InventoryChanged = true;
        }
    }
}
