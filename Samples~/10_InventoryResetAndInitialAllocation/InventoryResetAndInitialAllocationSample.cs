using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.GameFoundation.DefaultLayers.Persistence;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene and serves as an example for inventory reset & inventory initial allocation feature.
    /// </summary>
    public class InventoryResetAndInitialAllocationSample : MonoBehaviour
    {
        private bool m_WrongDatabase;

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
        public Button addAppleButton;
        public Button addOrangeButton;
        public Button removeAppleButton;
        public Button removeOrangeButton;
        public Button removeAllButton;
        public Button deleteAndReinitializeButton;

        /// <summary>
        /// We need to keep a reference to the persistence data layer if we want to save data.
        /// </summary>
        private PersistenceDataLayer m_DataLayer;

        /// <summary>
        /// Saved local persistence data which we can reuse to permit deleting local persistence data.
        /// </summary>
        LocalPersistence m_LocalPersistence;

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

            InitializeGameFoundation();
        }

        /// <summary>
        /// Initialize Game Foundation.  
        /// Called at startup as well as when reinitializing.
        /// </summary>
        private void InitializeGameFoundation()
        {
            // Disable all buttons for initialization.
            addAppleButton.interactable = false;
            addOrangeButton.interactable = false;
            removeAppleButton.interactable = false;
            removeOrangeButton.interactable = false;
            removeAllButton.interactable = false;
            deleteAndReinitializeButton.interactable = false;

            // - Initialize must always be called before working with any game foundation code.
            // - GameFoundation requires an IDataAccessLayer object that will provide and persist
            //   the data required for the various services (Inventory, Wallet, ...).
            m_LocalPersistence = new LocalPersistence("GF_Sample10_DataPersistence", new JsonDataSerializer());
            m_DataLayer = new PersistenceDataLayer(m_LocalPersistence);

            // Initialize Game Foundation, calls OnGameFoundationInitialized callback when complete.
            GameFoundation.Initialize(m_DataLayer, OnGameFoundationInitialized, Debug.LogError);
        }

        /// <summary>
        /// Once Game Foundation completes initialization, we can setup callbacks and update GUI.
        /// </summary>
        private void OnGameFoundationInitialized()
        {
            // Enable all permanent buttons (others are enabled only as appropriate in RefreshUI).
            addAppleButton.interactable = true;
            addOrangeButton.interactable = true;
            deleteAndReinitializeButton.interactable = true;

            // Show list of inventory items and update the button interactability.
            RefreshUI();
        }

        /// <summary>
        /// Enable InventoryManager callbacks for our UI refresh method.
        /// These callbacks will automatically be invoked anytime an inventory is added, or removed.
        /// This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
        /// </summary>
        void OnEnable()
        {
            InventoryManager.itemAdded += OnInventoryItemChanged;
            InventoryManager.itemRemoved += OnInventoryItemChanged;
        }

        /// <summary>
        /// Disable InventoryManager callbacks.
        /// </summary>
        void OnDisable()
        {
            InventoryManager.itemAdded -= OnInventoryItemChanged;
            InventoryManager.itemRemoved -= OnInventoryItemChanged;
        }

        /// <summary>
        /// Adds a single apple to the main inventory.
        /// </summary>
        public void AddItem(string itemDefinitionId)
        {
            try
            {
                // This will create a new item inside the InventoryManager, if the definitionId exists in the inventory catalog.
                // Because this method will throw an exception if the definitionId is not found in the inventory catalog, we'll surround
                // in a try catch and log any exceptions thrown.
                InventoryManager.CreateItem(itemDefinitionId);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Removes a single item from the main inventory.
        /// </summary>
        public void RemoveItem(string itemDefinition)
        {
            // To remove a single item from the InventoryManager, you need a specific instance of that item. Since we only know the 
            // InventoryItemDefinition id of the item we want to remove, we'll first look for all items with that definition.
            // We'll use the version of FindItemsByDefinition that lets us pass in a collection to be filled to reduce allocations.
            var items = InventoryManager.FindItemsByDefinition(itemDefinition);
            
            // Make sure there actually is an item available to return
            if (items.Length > 0)
            {
                // We'll remove the first instance in the array of apple items
                InventoryManager.RemoveItem(items[0]);
            }
        }

        /// <summary>
        /// Removes all items from the inventory WITHOUT reinitializing.
        /// </summary>
        public void RemoveAllInventoryItems()
        {
            InventoryManager.RemoveAllItems();
        }

        /// <summary>
        /// Uninitializes Game Foundation, deletes persistence data, then reinitializes Game Foundation.
        /// Note: Because Game Foundation is initialized again, all Initial Allocation items will be added again.
        /// </summary>
        public void DeleteAndReinitializeGameFoundation()
        {
            GameFoundation.Uninitialize();

            m_LocalPersistence.Delete(InitializeGameFoundation);
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

            RefreshButtons();
        }

        /// <summary>
        /// Enables/Disables the remove item buttons.
        /// The addButtons will always be interactable,
        /// but we only want to allow removing items if we have some to remove.
        /// </summary>
        private void RefreshButtons()
        {
            // FindItemsByDefinition will return a count of the number of items found no matter if you pass in a list or null
            // Since we don't actually care about the list, only the account, we'll save the allocation and just pass null
            var appleCount = InventoryManager.FindItemsByDefinition("apple", null);
            var orangeCount = InventoryManager.FindItemsByDefinition("orange", null);

            removeAppleButton.interactable = appleCount > 0;
            removeOrangeButton.interactable = orangeCount > 0;
            removeAllButton.interactable = appleCount + orangeCount > 0;
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
            RefreshUI();
        }
    }
}
