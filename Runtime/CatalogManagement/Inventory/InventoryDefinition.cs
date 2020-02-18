#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Describes preset values and rules for an Inventory. During runtime, it may
    /// be useful to refer back to the InventoryDefinition for the presets and rules,
    /// but the values cannot be changed at runtime.  The InventoryDefinition is
    /// also responsible for creating Inventories based on preset properties.
    /// </summary>
    /// <inheritdoc/>
    public class InventoryDefinition : BaseCollectionDefinition<InventoryDefinition, InventoryItemDefinition>
    {
        /// <summary>
        /// This creates a new InventoryDefinition.
        /// </summary>
        /// <param name="id">The Id of this InventoryDefinition.</param>
        /// <param name="displayName">The name this InventoryDefinition will have.</param>
        /// <returns>Reference to the InventoryDefinition that was created.</returns>
        public new static InventoryDefinition Create(string id, string displayName)
        {
            Tools.ThrowIfPlayMode("Cannot create an InventoryDefinition in play mode.");

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("InventoryDefinition Id can only be alphanumeric with optional dashes or underscores.");
            }

            var inventoryDefinition = ScriptableObject.CreateInstance<InventoryDefinition>();
            inventoryDefinition.Initialize(id, displayName);
            inventoryDefinition.name = $"{id}_Inventory";

            return inventoryDefinition;
        }

        /// <summary>
        /// Adds the given default item to this InventoryDefinition. 
        /// Note: this thows if item without a CurrencyDetailDefinition is added to the wallet.
        /// </summary>
        /// <param name="itemDefinition">The default InventoryItemDefinition to add.</param>
        /// <param name="quantity">Quantity of items to add (defaults to 0).</param>
        /// <returns>Whether or not the adding was successful.</returns>
        public override bool AddDefaultItem(InventoryItemDefinition itemDefinition, int quantity = 0)
        {
            if (!IsWalletCompatible(itemDefinition))
            {
                return false;
            }

            return base.AddDefaultItem(itemDefinition, quantity);
        }

        /// <summary>
        /// Adds the given default item to this InventoryDefinition. 
        /// Note: this thows if item without a CurrencyDetailDefinition is added to the wallet.
        /// </summary>
        /// <param name="defaultItem">The DefaultItem to add.</param>
        /// <returns>Whether or not the DefaultItem was added successfully.</returns>
        public override bool AddDefaultItem(DefaultItem defaultItem)
        {
            InventoryItemDefinition defaultItemDefinition =
                GameFoundationDatabaseSettings.database.inventoryCatalog.GetItemDefinition(defaultItem.definitionHash);

            if (!IsWalletCompatible(defaultItemDefinition))
            {
                return false;
            }

            return base.AddDefaultItem(defaultItem);
        }

        /// <summary>
        /// Helper method to make sure the given item definition is valid if this is the wallet.
        /// </summary>
        /// <param name="itemDefinition">The item definition we are checking.</param>
        /// <returns>Whether or not it is valid.</returns>
        private bool IsWalletCompatible(InventoryItemDefinition itemDefinition)
        {
            if (hash == Tools.StringToHash(InventoryCatalog.k_WalletInventoryDefinitionId))
            {
                if (itemDefinition == null)
                {
                    Debug.LogError("Invalid InventoryItemDefinition passed for default item to add to the wallet Inventory.");
                    return false;
                }

                if (itemDefinition.GetDetailDefinition<CurrencyDetailDefinition>() == null)
                {
                    Debug.LogError("It is not possible to add an item to the wallet that does NOT have a CurrencyDetailDefinition attached.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a summary string for this InventoryDefinition.
        /// </summary>
        /// <returns>Summary string for this InventoryDefinition.</returns>
        public override string ToString()
        {
            return $"InventoryDefinition(Id: '{id}' DisplayName: '{displayName}')";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // trying to access Game Foundation singletons while any of these is happening can cause Unity Editor to crash

            if (EditorApplication.isCompiling
                || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            // migrate data from 0.2.0 to 0.3.0
            // loop through and see if there are any items with hash values but no id values, then correct that

            for (var i = 0; i < m_DefaultItems.Count; i++)
            {
                var defaultItem = m_DefaultItems[i];

                if (defaultItem.definitionHash != 0
                    && string.IsNullOrEmpty(defaultItem.definitionId))
                {
                    var newDefinitionId = GameFoundationDatabaseSettings.database.inventoryCatalog.GetItemDefinition(defaultItem.definitionHash)?.id;

                    if (!string.IsNullOrEmpty(newDefinitionId))
                    {
                        m_DefaultItems[i] = new DefaultItem(newDefinitionId, defaultItem.quantity);
                    }
                }
            }
        }
#endif

    }
}
