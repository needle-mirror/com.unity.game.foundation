using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Preset values and rules for an InventoryItem.
    /// During runtime, it may be useful to refer back to the InventoryItemDefinition for
    /// the presets and rules, but the values cannot be changed at runtime.
    /// InventoryItemDefinitions are also used as factories to create InventoryItems.
    /// </summary>
    /// <inheritdoc/>
    public class InventoryItemDefinition : BaseItemDefinition<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        /// <summary>
        /// Constructor to build an InventoryItemDefinition object.
        /// </summary>
        /// <param name="id">The string id value for this InventoryItemDefinition. Throws error if null, empty or invalid.</param>
        /// <param name="displayName">The readable string display name value for this InventoryItemDefinition. Throws error if null or empty.</param>
        /// <param name="referenceDefinition">The reference GameItemDefinition for this InventoryItemDefinition. Null is an allowed value.</param>
        /// <param name="categories">The list of CategoryDefinition hashes that are the categories applied to this InventoryItemDefinition. If null value is passed in an empty list will be created.</param>
        /// <param name="detailDefinitions">The dictionary of Type, BaseDetailDefinition pairs that are the detail definitions applied to this InventoryItemDefinition. If null value is passed in an empty dictionary will be created.</param>
        /// <exception cref="System.ArgumentException">Throws if id or displayName are null or empty or if the id is not valid. Valid ids are alphanumeric with optional dashes or underscores.</exception>
        internal InventoryItemDefinition(string id, string displayName, GameItemDefinition referenceDefinition = null, List<int> categories = null, Dictionary<Type, BaseDetailDefinition> detailDefinitions = null)
            : base(id, displayName, referenceDefinition, categories, detailDefinitions)
        {
        }
        
        internal override InventoryItem CreateItem(BaseCollection<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem> owner, int gameItemId = 0)
        {
            return new InventoryItem(this, owner as Inventory, gameItemId);
        }

        /// <summary>
        /// Gets the category definition that matches the given hash.
        /// </summary>
        /// <param name="categoryHash">The hash to look for.</param>
        /// <returns>Reference to the category definition of the requested hash.</returns>
        protected override CategoryDefinition GetCategoryDefinition(int categoryHash)
        {
            return CatalogManager.inventoryCatalog.GetCategory(categoryHash);
        }
    }
}
