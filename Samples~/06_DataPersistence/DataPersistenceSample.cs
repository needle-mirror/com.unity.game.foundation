using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.GameFoundation.DefaultLayers;
using UnityEngine.GameFoundation.DefaultLayers.Persistence;
using UnityEngine.Promise;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.Sample
{
    /// <summary>
    /// This class manages the scene for showcasing the data persistence sample.
    /// </summary>
    public class DataPersistenceSample : MonoBehaviour
    {
        private bool m_WrongDatabase;

        /// <summary>
        /// Flag for whether the Inventory has changes in it that have not yet been updated in the UI.
        /// </summary>
        private bool m_InventoryChanged;
        
        /// <summary>
        /// We need to keep a reference to the persistence data layer if we want to save data.
        /// </summary>
        private PersistenceDataLayer m_DataLayer;
        
        /// <summary>
        /// Reference to a list of InventoryItems in the InventoryManager.
        /// </summary>
        private readonly List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        /// <summary>
        /// Used to reduce times mainText.text is accessed.
        /// </summary>
        private readonly StringBuilder m_DisplayText = new StringBuilder();

        /// <summary>
        /// Reference to the panel to display when the wrong database is in use.
        /// </summary>
        public GameObject wrongDatabasePanel;

        /// <summary>
        /// We will need a reference to the main text box in the scene so we can easily modify it.
        /// </summary>
        public Text mainText;

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
            // - For this sample we will persist GameFoundation's data using a PersistenceDataLayer.
            //   We create it with a LocalPersistence setup to save/load these data in a JSON file
            //   named "DataPersistenceSampleV2" stored on the device.  Note: 'V2' appended
            //   to the filename to ensure old persistence from previous version of Game Foundation
            //   isn't used, causing Sample to throw at initialization.  This is only needed during
            //   the 'preview' phase of Game Foundation while the structure of persistent data is
            //   changing.
            m_DataLayer = new PersistenceDataLayer(
                new LocalPersistence("DataPersistenceSampleV2", new JsonDataSerializer()));

            GameFoundation.Initialize(m_DataLayer, OnGameFoundationInitialized, Debug.LogError);
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
        /// Adds a new sword item to the InventoryManager.
        /// The sword Inventory Item first needs to be set up in the Inventory window.
        /// </summary>
        public void AddNewSword()
        {
            InventoryManager.CreateItem("sword");
        }

        /// <summary>
        /// Adds a health potion item to the inventory manager.
        /// The health potion Inventory Item first needs to be set up in the Inventory window.
        /// </summary>
        public void AddHealthPotion()
        {
            InventoryManager.CreateItem("healthPotion");
        }

        /// <summary>
        /// Removes all Inventory Items from InventoryManager.
        /// </summary>
        public void RemoveAllItems()
        {
            var count = InventoryManager.RemoveAllItems();
            Debug.Log(count + " inventory items removed");
        }

        /// <summary>
        /// This will save game foundation's data as a JSON file on your machine.
        /// This data will persist between play sessions.
        /// This sample only showcases inventories, but this method saves their items, and properties too. 
        /// </summary>
        public void Save()
        {
            // Deferred is a struct that helps you track the progress of an asynchronous operation of Game Foundation.
            Deferred saveOperation = m_DataLayer.Save();

            // Check if the operation is already done.
            if (saveOperation.isDone)
            {
                LogSaveOperationCompletion(saveOperation);
            }
            else
            {
                StartCoroutine(WaitForSaveCompletion(saveOperation));
            }
        }

        /// <summary>
        /// This will un-initialize game foundation and re-initialize it with data from the save file.
        /// This will set the current state of inventories and properties to be what's within the save file.
        /// </summary>
        public void Load()
        {
            // Don't forget to stop listening to events before un-initializing.
            InventoryManager.itemAdded -= OnInventoryItemChanged;
            InventoryManager.itemRemoved -= OnInventoryItemChanged;

            GameFoundation.Uninitialize();

            GameFoundation.Initialize(m_DataLayer, OnGameFoundationInitialized, Debug.LogError);
        }

        /// <summary>
        /// This will fill out the main text box with information about the main inventory.
        /// </summary>
        private void RefreshUI()
        {
            m_DisplayText.Clear();
            // Display the main inventory's display name
            m_DisplayText.Append("<b><i>Inventory:</i></b>");
            m_DisplayText.AppendLine();

            // We'll use the version of GetItems that lets us pass in a collection to be filled to reduce allocations
            InventoryManager.GetItems(m_InventoryItems);

            // Loop through every type of item within the inventory and display its name and quantity.
            foreach (InventoryItem inventoryItem in m_InventoryItems)
            {
                // All InventoryItems have an associated InventoryItemDefinition which contains a display name.
                string itemName = inventoryItem.definition.key;

                m_DisplayText.Append(itemName);
                m_DisplayText.AppendLine();
            }

            mainText.text = m_DisplayText.ToString();
        }

        private static IEnumerator WaitForSaveCompletion(Deferred saveOperation)
        {
            // Wait for the operation to complete.
            yield return saveOperation.Wait();

            LogSaveOperationCompletion(saveOperation);
        }

        private static void LogSaveOperationCompletion(Deferred saveOperation)
        {
            // Check if the operation was successful.
            if (saveOperation.isFulfilled)
            {
                Debug.Log("Saved!");
            }
            else
            {
                Debug.LogError($"Save failed! Error: {saveOperation.error}");
            }
        }

        private void OnGameFoundationInitialized()
        {
            // Here we bind our UI refresh method to callbacks on the inventory manager.
            // These callbacks will automatically be invoked anytime an inventory is added, or removed.
            // This prevents us from having to manually invoke RefreshUI every time we perform one of these actions.
            InventoryManager.itemAdded += OnInventoryItemChanged;
            InventoryManager.itemRemoved += OnInventoryItemChanged;

            RefreshUI();
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
