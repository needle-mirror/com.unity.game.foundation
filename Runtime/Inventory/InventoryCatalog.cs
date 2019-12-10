using UnityEditor;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Definitions for InventoryItemDefinitions and InventoryDefinitions.
    /// The Catalog serves as a way to find references to Definitions, as needed.
    /// </summary>
    /// <inheritdoc/>
    public class InventoryCatalog : BaseCatalog<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        internal static readonly string k_MainInventoryDefinitionId = "main";
        internal static readonly string k_MainInventoryDefinitionName = "Main";
        internal static readonly string k_WalletInventoryDefinitionId = "wallet";
        internal static readonly string k_WalletInventoryDefinitionName = "Wallet";

        /// <summary>
        /// Default constructor.
        /// </summary>
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
        internal override void VerifyDefaultCollections()
        {
            if (GetCollectionDefinition(InventoryManager.mainInventoryHash) == null)
            {
                var mainInventoryDefinition = InventoryDefinition.Create(k_MainInventoryDefinitionId, k_MainInventoryDefinitionName);
                AddCollectionDefinition(mainInventoryDefinition);

                // the Scriptable Object name that appears in the Project window
                mainInventoryDefinition.name = $"{k_MainInventoryDefinitionId}_Inventory";

                AssetDatabase.AddObjectToAsset(mainInventoryDefinition, this);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GetCollectionDefinition(InventoryManager.walletInventoryHash) == null)
            {
                var walletInventoryDefinition = InventoryDefinition.Create(k_WalletInventoryDefinitionId, k_WalletInventoryDefinitionName);
                AddCollectionDefinition(walletInventoryDefinition);

                // the Scriptable Object name that appears in the Project window
                walletInventoryDefinition.name = $"{k_WalletInventoryDefinitionId}_Inventory";

                AssetDatabase.AddObjectToAsset(walletInventoryDefinition, this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
#endif

        /// <summary>
        /// Removes the given collection definition from this catalog.
        /// </summary>
        /// <param name="collectionDefinition">The collection definition to remove.</param>
        /// <returns>Whether or not it was successfully removed.</returns>
        public override bool RemoveCollectionDefinition(InventoryDefinition collectionDefinition)
        {
            if (collectionDefinition.id == k_WalletInventoryDefinitionId || collectionDefinition.id == k_MainInventoryDefinitionId)
            {
                Debug.LogWarning("Main or Wallet inventory definitions cannot be removed from InventoryCatalog.");
                return false;
            }
            
            return base.RemoveCollectionDefinition(collectionDefinition);
        }
    }
}
