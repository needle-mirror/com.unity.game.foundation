using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Manages all Inventories. Can subscribe to events relevant to Inventories and create and remove them here.
    /// InventoryManager also owns the main and wallet Inventories, as well as all other Inventories of InventoryItems.
    /// The InventoryManager can create Inventories from InventoryDefinitions or default Inventories as needed.
    /// </summary>
    public static class InventoryManager
    {
        private static readonly int k_MainInventoryHash = Tools.StringToHash(InventoryCatalog.k_MainInventoryDefinitionId);

        /// <summary>
        /// This is the 'main' Inventory id as a hash for quick lookup and compares.
        /// </summary>
        /// <returns>The 'main' Inventory id as a hash for quick lookup and compares.</returns>
        internal static int mainInventoryHash { get; } = k_MainInventoryHash;

        /// <summary>
        /// This is a reference to the "main" Inventory for the InventoryManager.
        /// </summary>
        /// <returns>The "main" Inventory for the InventoryManager.</returns>
        public static Inventory main
        {
            get
            {
                ThrowIfNotInitialized();
                return m_Inventories[k_MainInventoryHash];
            }
        }

        /// <summary>
        /// This is the 'wallet' Inventory id as a hash for quick lookup and compares.
        /// </summary>
        /// <returns>The 'wallet' Inventory id as a hash for quick lookup and compares.</returns>
        internal static int k_WalletInventoryHash { get; } = Tools.StringToHash(InventoryCatalog.k_WalletInventoryDefinitionId);

        /// <summary>
        /// This is a reference to the "wallet" Inventory for the InventoryManager.
        /// </summary>
        /// <returns>The "wallet" Inventory for the InventoryManager.</returns>
        public static Inventory wallet
        {
            get
            {
                ThrowIfNotInitialized();
                return m_Inventories[k_WalletInventoryHash];
            }
        }

        /// <summary>
        /// This is an enumerator for iterating through all Inventories owned by the InventoryManager.
        /// </summary>
        /// <returns>An enumerator for iterating through all Inventories owned by the InventoryManager</returns>
        public static IEnumerable<Inventory> inventories
        {
            get
            {
                ThrowIfNotInitialized();
                return m_Inventories.Values;
            }
        }
        
        private static Dictionary<int, Inventory> m_Inventories = null;

        private const string k_ManagerPersistenceId = "gamefoundation_inventories";

        private static IDataPersistence m_DefaultPersistence = null;

        /// <summary>
        /// This is a Unity event that takes in a single Inventory as the parameter
        /// </summary>
        public class InventoryEvent : UnityEvent<Inventory> {}

        /// <summary>
        /// This is a Unity event that effects the entire InventoryManager
        /// </summary>
        public class InventoryManagerEvent : UnityEvent {}

        static InventoryManager()
        {
        }

        /// <summary>
        /// Initialize the InventoryManager. If a persistence layer object is passed as first parameter, it will be used
        /// to populate built-in Inventories and recreated persisted one. 
        /// If no argument is given, InventoryManager will be initialized with built-in and default inventories definitions.
        /// If the loading fails, onInitializeFailed will be called with an exception.
        /// </summary>
        /// <param name="persistence">An optional persistence layer to load inventories from</param>
        /// <param name="onInitializeFailed">An optional callback that will be called once Inventories have failed to initialize. </param>
        /// <param name="onInitializeCompleted">An optional callback that will be called once Inventories have been initialized from persistence layer</param>
        /// <exception cref="InvalidOperationException">Thrown if the user attempts to initialize after already having done so.</exception>
        public static void Initialize(IDataPersistence persistence = null, Action onInitializeCompleted = null, Action<Exception> onInitializeFailed = null)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Error: InventoryManager.Initialize was called more than once--please use Reset() to reinitialize.");
            }

            m_Inventories = new Dictionary<int, Inventory>();
            
            catalog.VerifyDefaultInventories();

            CreateInventory(InventoryCatalog.k_MainInventoryDefinitionId,
                InventoryCatalog.k_MainInventoryDefinitionId);
            CreateInventory(InventoryCatalog.k_WalletInventoryDefinitionId,
                InventoryCatalog.k_WalletInventoryDefinitionId);
            
            if (persistence == null)
            {
                AddDefaultInventories();
            }
            else
            {
                m_DefaultPersistence = persistence;
                
                persistence.Load<InventoryManagerSerializableData>(k_ManagerPersistenceId, (ISerializableData data) =>
                {
                    FillFromInventoriesData(data);
                    onInitializeCompleted?.Invoke();
                },
                e =>
                {
                    AddDefaultInventories(); // if we failed to fetch data from persistence, we restore default inventories
                    onInitializeFailed?.Invoke(e);
                });
            }
        }

        static void FillFromInventoriesData(ISerializableData data)
        {
            var inventoryManagerData = (InventoryManagerSerializableData) data;
            foreach (var persistedInventory in inventoryManagerData.inventories)
            {
                Inventory currentInventory;
                
                if (string.IsNullOrEmpty(persistedInventory.definitionId))
                {
                    continue;
                }
                
                if (persistedInventory.definitionId == InventoryCatalog.k_MainInventoryDefinitionId || persistedInventory.definitionId == InventoryCatalog.k_WalletInventoryDefinitionId)
                {
                    currentInventory = GetInventory(persistedInventory.definitionId);
                }
                else
                {
                    if (string.IsNullOrEmpty(persistedInventory.inventoryId))
                    {
                        continue;
                    }
                    
                    currentInventory = CreateInventory(persistedInventory.definitionId, persistedInventory.inventoryId);
                }

                if (currentInventory == null)
                    continue;
                
                foreach (var item in persistedInventory.items)
                {
                    if (!currentInventory.ContainsItem(item.definitionId))
                    {
                        currentInventory.AddItem(item.definitionId, item.quantity);
                    }
                    else

                    {
                        currentInventory.SetQuantity(item.definitionId, item.quantity);
                    }
                }
            }
        }

        /// <summary>
        /// Asynchronously loads data from the default local persistence layer and populates managed inventories with it
        /// </summary>
        /// <param name="onLoadCompleted">Called when the loading is completed with success</param>
        /// <param name="onLoadFailed">Called when the loading failed</param>
        public static void Load(Action onLoadCompleted = null, Action<Exception> onLoadFailed = null)
        {
            Load(m_DefaultPersistence, onLoadCompleted, onLoadFailed);
        }

        /// <summary>
        ///  Asynchronously loads data from the persistence layer and populates managed inventories with it
        /// </summary>
        /// <param name="persistence">Persistence layer required to load data from</param>
        /// <param name="onLoadCompleted">Called when the loading is completed with success</param>
        /// <param name="onLoadFailed">Called when the loading failed</param>
        public static void Load(IDataPersistence persistence, Action onLoadCompleted = null, Action<Exception> onLoadFailed = null)
        {
            if (persistence == null)
            {
                Debug.LogWarning("Inventory Manager could not be serialized without IDataPersistence. InventoryManager should be Initialized with IDataPersistence or ‘Load(IDataPersistence persistence)’ method should be called.");
                onLoadFailed?.Invoke(new ArgumentException("Error: Non null persistence param mandatory"));
                return;
            }

            if (!IsInitialized)
            {
                ThrowIfNotInitialized();
            }

            persistence.Load<InventoryManagerSerializableData>(k_ManagerPersistenceId, (data) =>
            {
                // remove non built-in inventories
                RemoveAllInventories();

                //empties built-in
                main.RemoveAll();
                wallet.RemoveAll();

                FillFromInventoriesData(data);
                onLoadCompleted?.Invoke();
            },
            (e) => onLoadFailed?.Invoke(e));
        }

        /// <summary>
        /// Asynchronously saves data through the persistence layer with the default local persistence layer.
        /// </summary>
        /// <param name="onSaveCompleted">Called when the saving is completed with success</param>
        /// <param name="onSaveFailed">Called when the loading failed</param>
        public static void Save(Action onSaveCompleted = null, Action<Exception> onSaveFailed = null)
        {
            Save(m_DefaultPersistence, onSaveCompleted, onSaveFailed);
        }

        /// <summary>
        /// Asynchronously saves data through the persistence layer.
        /// </summary>
        /// <param name="persistence">Persistence layer required to save data into</param>
        /// <param name="onSaveCompleted">Called when the saving is completed with success</param>
        /// <param name="onSaveFailed">Called when the loading failed</param>
        public static void Save(IDataPersistence persistence, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null)
        {
            if (persistence == null)
            {
                Debug.LogWarning("Inventory Manager could not be serialized without IDataPersistence. InventoryManager should be Initialized with IDataPersistence or ‘Save(IDataPersistence persistence)’ method should be called.");
                onSaveFailed?.Invoke(new ArgumentException("Error: Non null persistence param mandatory"));
                return;
            }

            if (!IsInitialized)
            {
                ThrowIfNotInitialized();
            }

            var inventoriesIndex = 0;

            // Temp hands conversion while waiting for a better serialization
            var inventories = new InventorySerializableData[InventoryManager.inventories.Count()];
            foreach (var inventory in InventoryManager.inventories)
            {
                var itemsData = new InventoryItemSerializableData[inventory.items.Count()];
                var itemsIndex = 0;
                foreach (var item in inventory.items)
                {
                    itemsData[itemsIndex] = new InventoryItemSerializableData(item.definitionId, item.quantity);
                    itemsIndex++;
                }

                inventories[inventoriesIndex] = new InventorySerializableData(inventory.definitionId, inventory.id, itemsData);
                inventoriesIndex++;
            }

            var inventoryManagerData = new InventoryManagerSerializableData(BaseDataPersistence.k_SaveVersion, inventories);
            persistence.Save(k_ManagerPersistenceId, inventoryManagerData , onSaveCompleted, onSaveFailed);
        }

        /// <summary>
        /// Returns the current initialization state of the InventoryManager.
        /// </summary>
        /// <returns>The current initialization state of the InventoryManager.</returns>
        public static bool IsInitialized
        {
            get { return !ReferenceEquals(m_Inventories, null); }
        }

        /// <summary>
        /// Throws an exception if the InventoryManager has not been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if not initialized.</exception>
        public static void ThrowIfNotInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Error: InventoryManager.Initialize() MUST be called before the InventoryManager is used.");
            }
        }

        /// <summary>
        /// This is the InventoryCatalog the InventoryManager uses.
        /// </summary>
        /// <returns>The InventoryCatalog the InventoryManager uses.</returns>
        public static InventoryCatalog catalog
        {
            get
            {
                ThrowIfNotInitialized();
                return GameFoundationSettings.inventoryCatalog;
            }
        }

        /// <summary>
        /// Fired whenever a new Inventory is added.
        /// </summary>
        /// <returns>The InventoryEvent fired whenever a new Inventory is added.</returns>
        public static InventoryEvent onInventoryAdded
        {
            get { return m_OnInventoryAdded; }
        }
        private static InventoryEvent m_OnInventoryAdded = new InventoryEvent();

        /// <summary>
        /// Fired whenever the InventoryManager is reset.
        /// </summary>
        /// <returns>The InventoryManagerEvent fired whenever the InventoryManager is reset.</returns>
        public static InventoryManagerEvent onInventoryManagerReset
        {
            get { return m_OnInventoryManagerReset; }
        }
        private static InventoryManagerEvent m_OnInventoryManagerReset = new InventoryManagerEvent();

        /// <summary>
        /// Fired whenever an Inventory is unable to add items because it's full (i.e. max quantity exceeded).
        /// </summary>
        /// <returns>The InventoryEvent fired whenever an Inventory is unable to add items because it's full.</returns>
        public static InventoryEvent onInventoryOverflow
        {
            get { return m_OnInventoryOverflow; }
        }
        private static InventoryEvent m_OnInventoryOverflow = new InventoryEvent();

        /// <summary>
        /// Fired whenever an Inventory is unable to deduct items because it's empty (i.e. attempts to go BELOW 0 qty).
        /// </summary>
        /// <returns>The InventoryEvent fired whenever an Inventory is unable to deduct items because it's empty.</returns>
        public static InventoryEvent onInventoryUnderflow
        {
            get { return m_OnInventoryUnderflow; }
        }
        private static InventoryEvent m_OnInventoryUnderflow = new InventoryEvent();

        /// <summary>
        /// Fired whenever an Inventory is about to be removed.
        /// </summary>
        /// <returns>The InventoryEvent fired whenever an Inventory is about to be removed.</returns>
        public static InventoryEvent onInventoryWillRemove
        {
            get { return m_OnInventoryWillRemove; }
        }
        private static InventoryEvent m_OnInventoryWillRemove = new InventoryEvent();

        /// <summary>
        /// Fired whenever an Inventory is removed.
        /// </summary>
        /// <returns>The InventoryEvent fired whenever an Inventory is removed.</returns>
        public static InventoryEvent onInventoryRemoved
        {
            get { return m_OnInventoryRemoved; }
        }
        private static InventoryEvent m_OnInventoryRemoved = new InventoryEvent();

        /// <summary>
        /// Returns a unique Inventory Id that hasn't been registered to any existing Inventory. Use with CreateInventory().
        /// </summary>
        /// <returns>Unique Inventory Id</returns>
        public static string GetNewInventoryId()
        {
            ThrowIfNotInitialized();

            string inventoryId;
            // find a unique Collection id to use (should only need 1 try, but there's a 1 in 4 billion chance that we're unlucky, so...
            do
            {
                inventoryId = Guid.NewGuid().ToString();
            }
            while (m_Inventories.ContainsKey(Tools.StringToHash(inventoryId)));

            return inventoryId;
        }

        /// <summary>
        /// This will create a new Inventory by specifying what InventoryDefinition id to use.
        /// </summary>
        /// <param name="definitionId">The id of the InventoryDefinition to assign this Inventory.</param>
        /// <param name="inventoryId">The id this inventory will have.</param>
        /// <returns>The newly created Inventory based on specified InventoryDefinition.</returns>
        public static Inventory CreateInventory(string definitionId, string inventoryId)
        {
            ThrowIfNotInitialized();
            
            return CreateInventory(Tools.StringToHash(definitionId), inventoryId);
        }

        /// <summary>
        /// This will create a new Inventory by specifying what InventoryDefinition to use by id hash.
        /// </summary>
        /// <param name="definitionHash">The hash of the InventoryDefinition to assign this Inventory.</param>
        /// <param name="inventoryId">The id this Inventory will have.</param>
        /// <returns>The newly created Inventory based on the specified InventoryDefinition.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an invalid hash is provided.</exception>
        /// <exception cref="ArgumentException">Thrown if the given id is null, empty, or a duplicate.</exception>
        public static Inventory CreateInventory(int definitionHash, string inventoryId)
        {
            ThrowIfNotInitialized();
            
            var definition = catalog.GetCollectionDefinition(definitionHash);
            if (definition == null)
            {
                throw new InvalidOperationException("Provided definition is not in the InventoryCatalog.");
            }

            if (string.IsNullOrEmpty(inventoryId))
            {
                throw new ArgumentException("Inventory Id is null or empty, specify an Inventory Id or generate one via GetNewInventoryId()");
            }

            if (m_Inventories.ContainsKey(Tools.StringToHash(inventoryId)))
            {
                throw new ArgumentException("Provided Inventory ID is one that's already registered in this InventoryManager.");
            }

            var newCollection = definition.CreateCollection(inventoryId, definition.displayName);
            m_Inventories.Add(newCollection.hash, newCollection);
            m_OnInventoryAdded.Invoke(newCollection);
            
            return newCollection;
        }

        /// <summary>
        /// This will create a new Inventory by specifying what InventoryDefinition to use.
        /// </summary>
        /// <param name="definition">The InventoryDefinition to assign this Inventory.</param>
        /// <param name="inventoryId">The id this Inventory will have.</param>
        /// <returns>The newly created Inventory based on the specified InventoryDefinition.</returns>
        public static Inventory CreateInventory(InventoryDefinition definition, string inventoryId)
        {
            ThrowIfNotInitialized();

            if (definition == null)
            {
                Debug.LogWarning("The provided inventory definition is null, this will not be created.");
                return null;
            }

            return CreateInventory(definition.hash, inventoryId);
        }

        /// <summary>
        /// This will return the Inventory using the specified Inventory id.
        /// </summary>
        /// <param name="inventoryId">The id of the Inventory we want.</param>
        /// <returns>The Inventory with the requested id.</returns>
        public static Inventory GetInventory(string inventoryId)
        {
            ThrowIfNotInitialized();
            
            return GetInventory(Tools.StringToHash(inventoryId));
        }

        /// <summary>
        /// This will return the Inventory using the specified Inventory id hash.
        /// </summary>
        /// <param name="hash">The hash of the id of the Inventory we want.</param>
        /// <returns>The Inventory with the requested hash id or null if not found.</returns>
        public static Inventory GetInventory(int hash)
        {
            ThrowIfNotInitialized();

            Inventory collection;
            m_Inventories.TryGetValue(hash, out collection);
            return collection;
        }

        /// <summary>
        /// This method checks if an Inventory exists with the given Inventory id.
        /// </summary>
        /// <param name="inventoryId">The id we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventory(string inventoryId)
        {
            ThrowIfNotInitialized();
            
            return HasInventory(Tools.StringToHash(inventoryId));
        }

        /// <summary>
        /// This method checks if an Inventory exists with the given Inventory id hash.
        /// </summary>
        /// <param name="inventoryHash">The id we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventory(int inventoryHash)
        {
            ThrowIfNotInitialized();

            return m_Inventories.ContainsKey(inventoryHash);
        }

        /// <summary>
        /// This method checks if an Inventory exists with the given InventoryDefinition id.
        /// </summary>
        /// <param name="definitionId">The id we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventoryByDefinition(string definitionId)
        {
            ThrowIfNotInitialized();
            
            return HasInventoryByDefinition(Tools.StringToHash(definitionId));
        }

        /// <summary>
        /// This method checks if an Inventory exists for given InventoryDefinition id hash.
        /// </summary>
        /// <param name="definitionHash">The hash we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventoryByDefinition(int definitionHash)
        {
            ThrowIfNotInitialized();

            foreach (var collection in m_Inventories.Values)
            {
                if (collection.definition.hash.Equals(definitionHash))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// This method checks if an Inventory exists for specified InventoryDefinition.
        /// </summary>
        /// <param name="definition">The InventoryDefinition we are checking for.</param>
        /// <returns>True/False whether or not the Inventory is found in the InventoryManager.</returns>
        public static bool HasInventoryByDefinition(InventoryDefinition definition)
        {
            ThrowIfNotInitialized();

            if (definition == null)
            {
                return false;
            }

            return HasInventoryByDefinition(definition.hash);
        }

        /// <summary>
        /// This method will remove the Inventory with the given instance Id.
        /// </summary>
        /// <param name="inventoryId">The id of the Inventory instance we want to remove.</param>
        /// <returns>Whether or not the Inventory was successfully removed.</returns>
        public static bool RemoveInventory(string inventoryId)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrEmpty(inventoryId))
                return false;
            
            return RemoveInventory(Tools.StringToHash(inventoryId));
        }

        /// <summary>
        /// This method will remove the Inventory with the given Inventory id hash.
        /// </summary>
        /// <param name="inventoryHash">The hash of the Inventory instance we want to remove.</param>
        /// <returns>Whether or not the Inventory was successfully removed.</returns>
        public static bool RemoveInventory(int inventoryHash)
        {
            ThrowIfNotInitialized();

            Inventory inventory;
            if (!m_Inventories.TryGetValue(inventoryHash, out inventory))
            {
                return false;
            }

            m_OnInventoryWillRemove.Invoke(inventory);

            if (!m_Inventories.Remove(inventoryHash))
            {
                return false;
            }

            m_OnInventoryRemoved.Invoke(inventory);

            return true;
        }

        /// <summary>
        /// This method will remove the given Inventory.
        /// </summary>
        /// <param name="inventory">The Inventory instance we want to remove.</param>
        /// <returns>Whether or not the Inventory was successfully removed.</returns>
        public static bool RemoveInventory(Inventory inventory)
        {
            ThrowIfNotInitialized();

            if (inventory == null)
            {
                return false;
            }
            
            return RemoveInventory(inventory.hash);
        }

        /// <summary>
        /// This method will remove the Inventory that uses the InventoryDefinition with the given id.
        /// </summary>
        /// <param name="definitionId">The id of the InventoryDefinition we want to remove.</param>
        /// <returns>The amount of inventories that were removed.</returns>
        public static int RemoveInventoriesByDefinition(string definitionId)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrEmpty(definitionId))
                return 0;
            
            return RemoveInventoriesByDefinition(Tools.StringToHash(definitionId));
        }

        /// <summary>
        /// This method will remove the Inventory that uses the InventoryDefinition with the given hash.
        /// </summary>
        /// <param name="definitionHash">The id hash of the InventoryDefinition we want to remove.</param>
        /// <returns>The amount of inventories that were removed.</returns>
        public static int RemoveInventoriesByDefinition(int definitionHash)
        {
            ThrowIfNotInitialized();

            List<Inventory> collectionsToRemove = new List<Inventory>();
            
            foreach (var collection in m_Inventories.Values)
            {
                if (collection.definition.hash == definitionHash)
                {
                    collectionsToRemove.Add(collection);
                }
            }

            foreach (var collection in collectionsToRemove)
            {
                m_OnInventoryWillRemove.Invoke(collection);
                if (m_Inventories.Remove(collection.hash))
                {
                    m_OnInventoryRemoved.Invoke(collection);
                }
            }

            return collectionsToRemove.Count;
        }

        /// <summary>
        /// This method will remove the Inventory that uses the given InventoryDefinition.
        /// </summary>
        /// <param name="definition">The InventoryDefinition we want to remove.</param>
        /// <returns>The amount of inventories that were removed.</returns>
        public static int RemoveInventoriesByDefinition(InventoryDefinition definition)
        {
            ThrowIfNotInitialized();

            if (definition == null)
            {
                return 0;
            }
            
            return RemoveInventoriesByDefinition(definition.hash);
        }

        /// <summary>
        /// This will simply clear out all Inventories.
        /// </summary>
        /// <param name="removeDefaultInventories">Whether or not default inventories should also be removed.</param>
        /// <returns>The total number of inventories that were removed.</returns>
        public static int RemoveAllInventories(bool removeDefaultInventories = true)
        {
            ThrowIfNotInitialized();

            // gather a list of all Inventories to remove (needed since we can't iterate a dictionary and remove items as we go)
            var inventoriesToRemove = new List<Inventory>();
            foreach (var inventory in m_Inventories.Values)
            {
                bool remove = false;
                if (inventory.hash != mainInventoryHash &&
                    inventory.hash != k_WalletInventoryHash)
                {
                    remove = true;
                    if (!removeDefaultInventories)
                    {
                        foreach (var defaultCollectionDefinition in catalog.m_DefaultCollectionDefinitions)
                        {
                            if (inventory.hash == defaultCollectionDefinition.hash)
                            {
                                remove = false;
                                break;
                            }
                        }
                    }
                }

                if (remove)
                {
                    inventoriesToRemove.Add(inventory);
                }
            }

            foreach (var inventory in inventoriesToRemove)
            {
                RemoveInventory(inventory);
            }

            return inventoriesToRemove.Count;
        }

        /// <summary>
        /// Can be called after Initialize() as many times as needed.
        /// Will reset everything to be as it was after Initialize() was called.
        /// </summary>
        public static void Reset()
        {
            ThrowIfNotInitialized();

            // Reset default Inventories and remove non default instances
            var inventoriesToRemove = new List<Inventory>();
            foreach (Inventory inventory in m_Inventories.Values)
            {
                bool isDefault = false;
                foreach (var defaultCollectionDefinition in catalog.m_DefaultCollectionDefinitions)
                {
                    if (inventory.hash == defaultCollectionDefinition.hash)
                    {
                        isDefault = true;
                        break;
                    }
                }
                
                if (isDefault)
                {
                    inventory.Reset();
                }
                else
                {
                    inventoriesToRemove.Add(inventory);
                }
            }

            foreach (var inventory in inventoriesToRemove) 
            {
                RemoveInventory(inventory);
            }

            AddDefaultInventories();

            // invoke event for entire InventoryManager reset
            onInventoryManagerReset.Invoke();
        }

        // add all default Inventories from the InventoryCatalog
        private static void AddDefaultInventories()
        {
            ThrowIfNotInitialized();

            foreach (var defaultInventoryDefinition in catalog.m_DefaultCollectionDefinitions)
            {
                if (!m_Inventories.ContainsKey(defaultInventoryDefinition.hash))
                {
                    var defaultInventory = defaultInventoryDefinition.collectionDefinition.CreateCollection(defaultInventoryDefinition.id, defaultInventoryDefinition.displayName);
                    m_Inventories[defaultInventory.hash] = defaultInventory;
                }
            }
        }

        /// <summary>
        /// Checks if the given string hashes to unique value and returns hash as out variable, if it is.
        /// </summary>
        /// <param name="inventoryId">The id to checking for.</param>
        /// <returns>True/False whether or not the hash is unique.</returns>
        public static bool IsInventoryHashUnique(string inventoryId)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrEmpty(inventoryId))
                return false;
            
            return !m_Inventories.ContainsKey(Tools.StringToHash(inventoryId));
        }
    }
}
