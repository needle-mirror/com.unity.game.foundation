
namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// InventoryDefaultCollectionDefinitions define DefaultCollectionDefinitions for the Inventory system.
    /// </summary>
    public class InventoryDefaultCollectionDefinition : DefaultCollectionDefinition<InventoryDefinition, Inventory, InventoryItemDefinition, InventoryItem>
    {
        internal InventoryDefaultCollectionDefinition(string id, string displayName, InventoryDefinition inventoryDefinition) : base(id, displayName, inventoryDefinition)
        {
        }
    }
}
