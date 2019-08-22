using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Definitions for InventoryItemDefinitions and InventoryDefinitions.
    /// The Catalog serves as a way to find references to Definitions, as needed.
    /// </summary>
    [CreateAssetMenu(fileName = "InventoryCatalog.asset", menuName = "Game Foundation/Catalog/Inventory Catalog")]
    public class InventoryCatalog : BaseCatalog<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        internal static readonly string k_MainInventoryDefinitionId = "main";
        internal static readonly string k_MainInventoryDefinitionName = "Main";
        internal static readonly string k_WalletInventoryDefinitionId = "wallet";
        internal static readonly string k_WalletInventoryDefinitionName = "Wallet";

        protected InventoryCatalog()
        {
        }

        /// <summary>
        /// Creates a new InventoryCatalog.
        /// </summary>
        /// <returns>Reference to the newly made InventoryCatalog.</returns>
        public static InventoryCatalog Create()
        {
            Tools.ThrowIfPlayMode("Cannot create an InventoryCatalog while in play mode.");
            
            var inventoryCatalog = ScriptableObject.CreateInstance<InventoryCatalog>();

            return inventoryCatalog;
        }

#if UNITY_EDITOR
        /// <summary>
        /// This will make sure main and wallet exist and are setup, and fix things if they aren't.
        /// </summary>
        public void VerifyDefaultInventories()
        {
            if (!HasMainCollection())
            {
                var mainInventoryDefinition = InventoryDefinition.Create(k_MainInventoryDefinitionId, k_MainInventoryDefinitionName);
                m_CollectionDefinitions.Add(mainInventoryDefinition);

                AssetDatabase.AddObjectToAsset(mainInventoryDefinition, this);
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            if (!HasMainDefaultCollection())
            {
                var main = GetCollectionDefinition(k_MainInventoryDefinitionId);
                var mainInventoryDefaultCollectionDefinition = new InventoryDefaultCollectionDefinition(k_MainInventoryDefinitionId, k_MainInventoryDefinitionName, main);
                m_DefaultCollectionDefinitions.Add(mainInventoryDefaultCollectionDefinition);
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (!HasWalletCollection())
            {
                var walletInventoryDefinition = InventoryDefinition.Create(k_WalletInventoryDefinitionId, k_WalletInventoryDefinitionName);
                m_CollectionDefinitions.Add(walletInventoryDefinition);
                
                AssetDatabase.SaveAssets();
                AssetDatabase.AddObjectToAsset(walletInventoryDefinition, this);
            }

            if (!HasWalletDefaultCollection())
            {
                var walletInventoryDefaultCollectionDefinition = new InventoryDefaultCollectionDefinition(k_WalletInventoryDefinitionId, k_WalletInventoryDefinitionName, GetCollectionDefinition(k_WalletInventoryDefinitionId));
                m_DefaultCollectionDefinitions.Add(walletInventoryDefaultCollectionDefinition);
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
#endif

        private bool HasMainCollection()
        {
            foreach (InventoryDefinition inventoryDefinition in m_CollectionDefinitions)
            {
                if (inventoryDefinition.id == k_MainInventoryDefinitionId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasMainDefaultCollection()
        {
            foreach (InventoryDefaultCollectionDefinition inventoryDefinition in m_DefaultCollectionDefinitions)
            {
                if (inventoryDefinition.id == k_MainInventoryDefinitionId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasWalletCollection()
        {
            foreach (InventoryDefinition inventoryDefinition in m_CollectionDefinitions)
            {
                if (inventoryDefinition.id == k_WalletInventoryDefinitionId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasWalletDefaultCollection()
        {
            foreach (InventoryDefaultCollectionDefinition inventoryDefinition in m_DefaultCollectionDefinitions)
            {
                if (inventoryDefinition.id == k_WalletInventoryDefinitionId)
                {
                    return true;
                }
            }

            return false;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// This class uses the AssetPostProcessor to verify a InventoryCatalog's default Inventories right after it is created as an asset.
    /// </summary>
    class InventoryCatalogImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            VerifyPaths(importedAssets);
            VerifyPaths(movedAssets);
            VerifyPaths(movedFromAssetPaths);
        }

        private static void VerifyPaths(string[] assetPaths)
        {
            foreach (string importedAsset in assetPaths)
            {
                var catalog = AssetDatabase.LoadAssetAtPath<InventoryCatalog>(importedAsset);
                if (catalog != null)
                {
                    catalog.VerifyDefaultInventories();
                }
            }
        }
    }
#endif
}
