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
    public class InventoryWithQuantitySample : MonoBehaviour
    {
        private bool m_WrongDatabase;
        
        /// <summary>
        /// Flag for whether the Inventory has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_InventoryChanged;

        /// <summary>
        /// We will keep an aggregated list of Inventory Items that are in InventoryManager
        /// based on their InventoryItemDefinition ids so that we can display each item with a quantity
        /// rather than each item individually.
        /// </summary>
        private readonly Dictionary<string, List<InventoryItem>> m_UniqueInventoryItems = new Dictionary<string, List<InventoryItem>>();

        /// <summary>
        /// Reference to a list of InventoryItems in the InventoryManager.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

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
        public Button removeAllApplesButton;

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

            // For this sample, we're focusing on apples, so let's remove any initial oranges from the Inventory.
            InventoryManager.RemoveItemsByDefinition("orange");

            // Here we bind a listener that will set an inventoryChanged flag to callbacks on the Inventory Manager.
            // These callbacks will automatically be invoked anytime an item is added or removed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            InventoryManager.itemAdded += OnInventoryItemChanged;
            InventoryManager.itemRemoved += OnInventoryItemChanged;

            // We'll call this to get our initial list of items to know the correct quantities for each aggregated item
            RefreshUniqueItems();
        }
        
        /// <summary>
        /// Standard Update method for Unity scripts.
        /// </summary>
        private void Update()
        {
            // This flag will be set to true when something has changed in the InventoryManager (either items were added or removed)
            if (m_InventoryChanged)
            {
                RefreshUniqueItems();
                m_InventoryChanged = false;
            }
        }

        /// <summary>
        /// Adds a single apple to the InventoryManager.
        /// </summary>
        public void AddApple()
        {
            try
            {
                // This will create a new item inside the InventoryManager, if the definitionId exists in the inventory catalog.
                // Because this method will throw an exception if the definitionId is not found in the inventory catalog, we'll surround
                // in a try catch and log any exceptions thrown.
                InventoryManager.CreateItem("apple");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Removes a single apple from the main inventory.
        /// </summary>
        public void RemoveApple()
        {
            // First we'll check that our list of unique items contains any items with apple as their inventoryItemDefinition id
            // If it doesn't, we'll take no action since there is nothing to remove.
            if (m_UniqueInventoryItems.TryGetValue("apple", out var itemGroup))
            {
                // Because we only care about the items in aggregate, and not their specific instances,
                // we can remove any item with the correct inventoryItemDefinition id. In this case we'll remove
                // the last one in our list.
                InventoryManager.RemoveItem(itemGroup[itemGroup.Count - 1]);
                // Once we remove the item from the InventoryManager, the reference to it will be broken.
                itemGroup[itemGroup.Count - 1] = null;
            }
        }

        /// <summary>
        /// Removes all instances of apple InventoryItemDefinition from the Inventory.
        /// </summary>
        public void RemoveAllApples()
        {
            // This method can be called whether or not there are any items with this definition id in the InventoryManager.
            // If there are no items with that definition id, the method will take no action and return a count of 0;
            int itemsRemovedCount = InventoryManager.RemoveItemsByDefinition("apple");
            
            Debug.Log(itemsRemovedCount + " apple item(s) removed from inventory.");
        }
        
        /// <summary>
        /// Updates the aggregated list of inventory items to make sure that all counts are accurate.
        /// This example shows the more complicated way of getting an aggregated list of all items in the InventoryManager.
        /// If you want to only track one item (like we're doing here with apple) you can also use the
        /// InventoryManager.FindItemsByDefinition() group of methods. 
        /// </summary>
        private void RefreshUniqueItems()
        {
            // Gets the list of all items in the Inventory Manager. This list will contain one item for each inventory item
            // created, which means it may include multiple of the same inventoryItemDefinition.
            InventoryManager.GetItems(m_InventoryItems);
            m_UniqueInventoryItems.Clear();

            // We will loop through the list of all items, adding them to our list of unique items based on the id of their inventoryItemDefinition
            foreach (var item in m_InventoryItems)
            {
                if (m_UniqueInventoryItems.ContainsKey(item.definition.key))
                {
                    m_UniqueInventoryItems[item.definition.key].Add(item);
                }
                else
                {
                    m_UniqueInventoryItems.Add(item.definition.key, new List<InventoryItem>{ item });
                }
            }

            // Update the UI with the new list information
            RefreshUI();
        }

        /// <summary>
        /// This will fill out the main text box with information about the inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            m_DisplayText.Append("<b><i>Inventory:</i></b>");
            m_DisplayText.AppendLine();

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (KeyValuePair<string, List<InventoryItem>> itemGroup in m_UniqueInventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                // We'll use that to display the name of this aggregated group of Items.
                string itemName = itemGroup.Value[0].definition.displayName;

                // The quantity in this case will the count of items in the itemGroup list.
                var quantity = itemGroup.Value.Count;

                m_DisplayText.Append(itemName + ": " + quantity);
                m_DisplayText.AppendLine();
            }

            mainText.text = m_DisplayText.ToString();

            RefreshRemoveButtons();
        }

        /// <summary>
        /// Enables/Disables the remove item buttons.
        /// The addButton will always be interactable,
        /// but we only want to allow removing items if we have some to remove.
        /// </summary>
        private void RefreshRemoveButtons()
        {
            if (m_UniqueInventoryItems.ContainsKey("apple"))
            {
                removeAppleButton.interactable = removeAllApplesButton.interactable = m_UniqueInventoryItems["apple"].Count > 0;
            }
            else
            {
                removeAppleButton.interactable = removeAllApplesButton.interactable = false;
            }
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
