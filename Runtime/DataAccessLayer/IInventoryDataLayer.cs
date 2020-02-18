using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contract for objects providing data to the <see cref="InventoryManager"/>.
    /// </summary>
    public interface IInventoryDataLayer
    {
        /// <summary>
        /// Get InventoryManager's serializable data.
        /// </summary>
        InventoryManagerSerializableData GetData();

        /// <summary>
        /// Request to create a new inventory with the given id.
        /// </summary>
        /// <param name="definitionId">Id of the definition used to create the inventory.</param>
        /// <param name="inventoryId">Id to give to the created inventory.</param>
        /// <param name="displayName">Friendly name of the created inventory that will be displayed.</param>
        /// <param name="gameItemId">Id of the item use by GameItemLookup.</param>
        /// <param name="completer">The handle to settle the promise with.</param>
        void CreateInventory(string definitionId, string inventoryId, string displayName, int gameItemId, Completer completer);

        /// <summary>
        /// Request the deletion of the given inventory. 
        /// </summary>
        /// <param name="inventoryId">Id of the inventory to delete.</param>
        /// <param name="completer">The handle to settle the promise with.</param>
        void DeleteInventory(string inventoryId, Completer completer);

        /// <summary>
        /// Request to update the given quantity for the item matching the given definition in the given inventory.
        /// </summary>
        /// <param name="inventoryId">The id of the inventory to update.</param>
        /// <param name="itemDefinitionId">The id of the definition of the item we want to update the quantity of.</param>
        /// <param name="quantity">
        /// Quantity of item to set.
        /// Note that 0 or negative values will not remove the item from the inventory.
        /// </param>
        /// <param name="gameItemId">Id of the item use by GameItemLookup.</param>
        /// <param name="completer">The handle to settle the promise with.</param>
        void SetItemQuantity(string inventoryId, string itemDefinitionId, int quantity, int gameItemId, Completer completer);

        /// <summary>
        /// Request to delete the item matching the given definition from the given inventory.
        /// </summary>
        /// <param name="inventoryId">The id of the inventory to update.</param>
        /// <param name="itemDefinitionId">The id of the definition of the item we want to delete.</param>
        /// <param name="completer">The handle to settle the promise with.</param>
        void DeleteItem(string inventoryId, string itemDefinitionId, Completer completer);
    }
}
