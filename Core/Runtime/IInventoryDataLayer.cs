using System;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Contract for objects providing data to the <see cref="InventoryManager" />.
    /// </summary>
    public interface IInventoryDataLayer
    {
        /// <summary>
        ///     Get InventoryManager's serializable data.
        /// </summary>
        InventoryManagerData GetData();

        /// <summary>
        ///     Request to create a new item with the given <paramref name="key" /> and <paramref name="id" />.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the definition used to create the item.
        /// </param>
        /// <param name="id">
        ///     Identifier to give to the created item.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void CreateItem(string key, string id, Completer completer);

        /// <summary>
        ///     Request to delete the item matching the given <paramref name="id" />.
        /// </summary>
        /// <param name="id">
        ///     The identifier of the item we want to delete.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void DeleteItem(string id, Completer completer);

        /// <summary>
        ///     Get the property with the given <paramref name="propertyKey" />
        ///     of the item with the given <paramref name="itemId" />.
        /// </summary>
        /// <param name="itemId">
        ///     The item's identifier.
        /// </param>
        /// <param name="propertyKey">
        ///     The property's identifier.
        /// </param>
        /// <returns>
        ///     The property's value.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If either <paramref name="itemId" /> or <paramref name="propertyKey" />
        ///     is null, empty or whitespace.
        /// </exception>
        /// <exception cref="InventoryItemNotFoundException">
        ///     If there is no item with the given <paramref name="itemId" />.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no property with the given <paramref name="propertyKey" /> on the item.
        /// </exception>
        Property GetPropertyValue(string itemId, string propertyKey);

        /// <summary>
        ///     Get the property with the given <paramref name="propertyKey" />
        ///     of the item with the given <paramref name="itemId" />.
        /// </summary>
        /// <param name="itemId">
        ///     The item's identifier.
        /// </param>
        /// <param name="propertyKey">
        ///     The property's identifier.
        /// </param>
        /// <param name="propertyValue">
        ///     The property's value found for the given identifiers.
        /// </param>
        /// <returns>
        ///     True if a property with the given <paramref name="propertyKey" /> exists
        ///     in the item with the given <paramref name="itemId" />;
        ///     false otherwise.
        /// </returns>
        bool TryGetPropertyValue(string itemId, string propertyKey, out Property propertyValue);

        /// <summary>
        ///     Request to update the property with the given <paramref name="propertyKey" />
        ///     of the item with the given <paramref name="itemId" />.
        /// </summary>
        /// <param name="itemId">
        ///     Item's id to update the property of.
        /// </param>
        /// <param name="propertyKey">
        ///     property's key to update.
        /// </param>
        /// <param name="value">
        ///     Value to set to the property.
        /// </param>
        /// <param name="completer">
        ///     The handle to settle the promise with.
        /// </param>
        void SetPropertyValue(string itemId, string propertyKey, Property value, Completer completer);
    }
}
