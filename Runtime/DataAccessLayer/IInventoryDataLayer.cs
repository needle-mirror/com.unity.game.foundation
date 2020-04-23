using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.Promise;

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
        /// Request to create a new item with the given id.
        /// </summary>
        /// <param name="definitionId">Id of the definition used to create the item.</param>
        /// <param name="itemId">Id to give to the created item.</param>
        /// <param name="completer">The handle to settle the promise with.</param>
        void CreateItem(string definitionId, string itemId, Completer completer);
        
        /// <summary>
        /// Request to delete the item matching the given definition from the given inventory.
        /// </summary>
        /// <param name="itemId">The id of the item we want to delete.</param>
        /// <param name="completer">The handle to settle the promise with.</param>
        void DeleteItem(string itemId, Completer completer);
    }
}
