using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes preset values and rules for an Inventory. During runtime, it may
    /// be useful to refer back to the InventoryDefinition for the presets and rules,
    /// but the values cannot be changed at runtime.  The InventoryDefinition is
    /// also responsible for creating Inventories based on preset properties.
    /// </summary>
    /// <inheritdoc/>
    public class InventoryDefinition : BaseCollectionDefinition<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        /// <summary>
        /// Constructor to build an InventoryDefinition object.
        /// </summary>
        /// <param name="id">The string id value for this InventoryDefinition. Throws error if null, empty or invalid.</param>
        /// <param name="displayName">The readable string display name value for this InventoryDefinition. Throws error if null or empty.</param>
        /// <param name="referenceDefinition">The reference GameItemDefinition for this InventoryDefinition. Null is an allowed value.</param>
        /// <param name="categories">The list of CategoryDefinition hashes that are the categories applied to this InventoryDefinition. If null value is passed in an empty list will be created.</param>
        /// <param name="detailDefinitions">The dictionary of Type, BaseDetailDefinition pairs that are the detail definitions applied to this InventoryDefinition. If null value is passed in an empty dictionary will be created.</param>
        /// <param name="defaultItems">The list of DefaultItemDefinitions that are the item definitions that will be automatically instantiated and added to a runtime instance of this inventory at its instantiation. If null value is passed in an empty list will be created.</param>
        /// <exception cref="System.ArgumentException">Throws if id or displayName are null or empty or if the id is not valid. Valid ids are alphanumeric with optional dashes or underscores.</exception>
        internal InventoryDefinition(string id, string displayName, GameItemDefinition referenceDefinition = null, List<int> categories = null, Dictionary<Type, BaseDetailDefinition> detailDefinitions = null, List<DefaultItemDefinition> defaultItems = null)
            : base(id, displayName, referenceDefinition, categories, detailDefinitions, defaultItems)
        {
        }

        internal override Inventory CreateCollection(string collectionId, string displayName = null, int gameItemId = 0)
        {
            return new Inventory(this, collectionId, displayName, gameItemId);
        }

        /// <summary>
        /// Helper method to make sure the given item definition is valid if this is the wallet.
        /// </summary>
        /// <param name="itemDefinition">The item definition we are checking.</param>
        /// <returns>Whether or not it is valid.</returns>
        private bool IsWalletCompatible(InventoryItemDefinition itemDefinition)
        {
            if (hash == InventoryManager.walletInventoryHash)
            {
                if (itemDefinition == null)
                {
                    Debug.LogError("Invalid InventoryItemDefinition passed for default item to add to the wallet Inventory.");
                    return false;
                }

                if (itemDefinition.GetDetailDefinition<CurrencyDetailDefinition>() == null)
                {
                    Debug.LogError("It is not possible to add an item to the wallet that does not have a CurrencyDetailDefinition attached.");
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
    }
}
