using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Preset values and rules for an InventoryItem. 
    /// During runtime, it may be useful to refer back to the InventoryItemDefinition for 
    /// the presets and rules, but the values cannot be changed at runtime.
    /// InventoryItemDefinitions are also used as factories to create InventoryItems.
    /// </summary>
    public class InventoryItemDefinition : BaseItemDefinition<InventoryItemDefinition, InventoryItem>
    {
        internal override InventoryItem CreateItem()
        {
            return new InventoryItem(this);
        }

        /// <summary>
        /// Creates a new InventoryItemDefinition.
        /// </summary>
        /// <param name="id">The id this InventoryItemDefinition will use.</param>
        /// <param name="displayName">The display name of the InventoryItemDefinition.</param>
        /// <returns>Reference to the newly made InventoryItemDefinition.</returns>
        public new static InventoryItemDefinition Create(string id, string displayName)
        {
            Tools.ThrowIfPlayMode("Cannot make an InventoryItemDefinition while in play mode.");
            
            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("DefaultCollectionDefinition can only be alphanumeric with optional dashes or underscores.");
            }
            
            InventoryItemDefinition definition = ScriptableObject.CreateInstance<InventoryItemDefinition>();
            definition.Initialize(id, displayName);

            return definition;
        }

        protected override CategoryDefinition GetCategoryDefinition(int hash)
        {
            return GameFoundationSettings.inventoryCatalog.GetCategory(hash);
        }
    }
}
