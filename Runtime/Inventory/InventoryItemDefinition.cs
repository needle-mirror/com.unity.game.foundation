namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Preset values and rules for an InventoryItem.
    /// During runtime, it may be useful to refer back to the InventoryItemDefinition for
    /// the presets and rules, but the values cannot be changed at runtime.
    /// InventoryItemDefinitions are also used as factories to create InventoryItems.
    /// </summary>
    /// <inheritdoc/>
    public class InventoryItemDefinition : CatalogItem
    {}
}
