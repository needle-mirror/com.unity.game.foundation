using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
{
    /// <summary>
    ///     Straightforward implementation of <see cref="IInventoryDataLayer" />.
    /// </summary>
    class InventoryDataLayer : IInventoryDataLayer
    {
        /// <summary>
        ///     Stores the data of all the item instances.
        ///     Key: item's id.
        ///     Value: item's data.
        /// </summary>
        Dictionary<string, InventoryItemData> m_Items;

        /// <summary>
        ///     Catalog containing all definitions.
        /// </summary>
        InventoryCatalogAsset m_Catalog;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InventoryDataLayer" />
        ///     class with the given <paramref name="data" />.
        /// </summary>
        /// <param name="data">
        ///     InventoryManager's serializable data.
        /// </param>
        /// <param name="catalog">
        ///     The catalog used as source of truth.
        /// </param>
        public InventoryDataLayer(InventoryManagerData data, InventoryCatalogAsset catalog)
        {
            m_Catalog = catalog;

            var validatedItems = GetValidItems(data, catalog);

            m_Items = new Dictionary<string, InventoryItemData>(validatedItems.Count);

            foreach (var item in validatedItems)
            {
                m_Items.Add(item.id, item);
            }
        }

        /// <summary>
        ///     Try to find the <paramref name="item" /> with the given <paramref name="id" />.
        /// </summary>
        /// <param name="id">
        ///     The item's id to look for.
        /// </param>
        /// <param name="item">
        ///     The item with the given <paramref name="id" /> if it exists.
        /// </param>
        /// <returns>
        ///     Return true if the item exists;
        ///     return false otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the given <paramref name="id" /> is null.
        /// </exception>
        internal bool TryGetItem(string id, out InventoryItemData item)
        {
            return m_Items.TryGetValue(id, out item);
        }

        /// <summary>
        ///     Get all ids of items created from the definition with the given <paramref name="key" />.
        /// </summary>
        /// <param name="key">
        ///     The definition's identifier to look items of.
        /// </param>
        /// <param name="target">
        ///     The collection to fill with the found items.
        /// </param>
        /// <returns>
        ///     The number of items found.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key" /> parameter is null, empty, or whitespace.
        /// </exception>
        internal int GetItemsByDefinition(string key, ICollection<string> target = null)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));

            var count = 0;
            target?.Clear();

            foreach (var item in m_Items.Values)
            {
                if (item.definitionKey == key)
                {
                    count++;
                    target?.Add(item.id);
                }
            }

            return count;
        }

        /// <inheritdoc cref="CreateItem(string, string)" />
        internal InventoryItemData CreateItem(string key)
        {
            var itemId = Guid.NewGuid().ToString();

            return CreateItem(key, itemId);
        }

        /// <summary>
        ///     Create a new item.
        /// </summary>
        /// <param name="key">
        ///     The definition's identifier used to create the item.
        /// </param>
        /// <param name="itemId">
        ///     The item's id to use.
        /// </param>
        /// <returns>
        ///     The id of the newly created item.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If either of the given <paramref name="key" /> or
        ///     <paramref name="itemId" /> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="CatalogItemNotFoundException">
        ///     If there is definition with the given <paramref name="key" /> in the catalog.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     If the <paramref name="itemId" /> is already used by another item.
        /// </exception>
        internal InventoryItemData CreateItem(string key, string itemId)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            Tools.ThrowIfArgNullOrEmpty(itemId, nameof(itemId));

            var itemDefinition = m_Catalog.FindItem(key);
            if (itemDefinition == null)
                throw new CatalogItemNotFoundException(key);

            if (m_Items.ContainsKey(itemId))
                throw new InvalidOperationException($"An Item with the id \"{itemId}\" already exists.");

            return CreateItemNoCheck(itemDefinition, itemId);
        }

        /// <summary>
        ///     Create an item from the given <paramref name="itemDefinition" /> with
        ///     the given <paramref name="itemId" /> without checking their validity nor the item existence.
        /// </summary>
        /// <param name="itemDefinition">
        ///     The definition to use to create the item.
        /// </param>
        /// <param name="itemId">
        ///     The id to give to the created item.
        /// </param>
        /// <returns>
        ///     Return the created item.
        /// </returns>
        InventoryItemData CreateItemNoCheck(InventoryItemDefinitionAsset itemDefinition, string itemId)
        {
            var item = new InventoryItemData
            {
                id = itemId,
                definitionKey = itemDefinition.key
            };

            //Setup properties.
            {
                item.properties = new PropertyData[itemDefinition.properties.Count];
                var i = 0;
                foreach (var propertyEntry in itemDefinition.properties)
                {
                    item.properties[i] = new PropertyData
                    {
                        key = propertyEntry.Key,
                        value = propertyEntry.Value
                    };
                    ++i;
                }
            }

            m_Items.Add(itemId, item);

            return item;
        }

        /// <summary>
        ///     Delete the item with the given <paramref name="id" />.
        /// </summary>
        /// <param name="id">
        ///     Identifier of the item to delete.
        /// </param>
        /// <returns>
        ///     <c>true</c> if deleted; <c>false</c> otherwise.
        /// </returns>
        internal bool DeleteItem(string id) => m_Items.Remove(id);

        /// <summary>
        ///     Create a list of valid items from the given <paramref name="data" />
        ///     using the given <paramref name="catalog" /> as a source of truth.
        /// </summary>
        /// <param name="data">
        ///     Data to parse and check for validity.
        /// </param>
        /// <param name="catalog">
        ///     Source of truth for items' definition.
        /// </param>
        /// <returns>
        ///     Return a list all items from the given <paramref name="data" /> that have a valid definition.
        ///     Their properties have also been validated to match their matching definition.
        /// </returns>
        static List<InventoryItemData> GetValidItems(InventoryManagerData data, InventoryCatalogAsset catalog)
        {
            int numDataItems = data.items.Length;
            var validatedItems = new List<InventoryItemData>(numDataItems);

            if (numDataItems > 0)
            {
                var validatedProperties = new List<PropertyData>();

                for (var itemIndex = 0; itemIndex < numDataItems; itemIndex++)
                {
                    var item = data.items[itemIndex];

                    //Check item's definition.
                    Exception error;
                    var itemDefinition = catalog.FindItem(item.definitionKey);
                    if (itemDefinition == null)
                    {
                        error = new CatalogItemNotFoundException(item.definitionKey);
                        Debug.LogWarning($"\"{item.id}\" has been skipped.\nReason: {error}");
                        continue;
                    }

                    var isItemIdAlreadyUsed = false;
                    foreach (var validatedItem in validatedItems)
                    {
                        if (validatedItem.id == item.id)
                        {
                            isItemIdAlreadyUsed = true;
                            break;
                        }
                    }

                    if (isItemIdAlreadyUsed)
                    {
                        error = new Exception("An item with the id \"{item.id}\" already exist.");
                        Debug.LogWarning($"\"{item.id}\" has been skipped.\nReason: {error}");
                        continue;
                    }

                    // get item definition's default properties
                    var defaultProperties = itemDefinition.properties;

                    // validate properties
                    if (item.properties != null)
                    {
                        for (var propertyIndex = 0; propertyIndex < item.properties.Length; propertyIndex++)
                        {
                            var property = item.properties[propertyIndex];
                            var propertyKey = property.key;

                            //Check property's default value.
                            if (!defaultProperties.TryGetValue(propertyKey, out var defaultProperty))
                            {
                                error = new PropertyNotFoundException(item.id, propertyKey);
                                Debug.LogWarning(
                                    $"\"{item.id}\"'s \"{propertyKey}\" property has been skipped.\nReason: {error}");
                                continue;
                            }

                            //Check property's type compatibility.
                            if (defaultProperty.type != property.value.type)
                            {
                                error = new PropertyInvalidCastException(
                                    propertyKey, defaultProperty.type, property.value.type);
                                Debug.LogWarning(
                                    $"\"{item.id}\"'s \"{propertyKey}\" property has been reset.\nReason: {error}");
                                property.value = defaultProperties[propertyKey];
                            }

                            validatedProperties.Add(property);
                        }
                    }

                    // add missing default properties.
                    foreach (var defaultProperty in defaultProperties)
                    {
                        var isDefaultPropertyValidated = false;
                        foreach (var validatedProperty in validatedProperties)
                        {
                            if (defaultProperty.Key == validatedProperty.key)
                            {
                                isDefaultPropertyValidated = true;
                                break;
                            }
                        }

                        if (!isDefaultPropertyValidated)
                        {
                            var validProperty = new PropertyData
                            {
                                key = defaultProperty.Key,
                                value = defaultProperty.Value
                            };
                            validatedProperties.Add(validProperty);
                        }
                    }

                    // set item properties
                    item.properties = validatedProperties.ToArray();

                    // clear validated properties list for reuse
                    validatedProperties.Clear();

                    // add this item to list of validated items
                    validatedItems.Add(item);
                }
            }

            return validatedItems;
        }

        /// <inheritdoc />
        InventoryManagerData IInventoryDataLayer.GetData()
        {
            var items = new InventoryItemData[m_Items.Count];
            m_Items.Values.CopyTo(items, 0);

            var data = new InventoryManagerData
            {
                items = items
            };

            return data;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key" /> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="CatalogItemNotFoundException">
        ///     If there is definition with the given <paramref name="key" /> in the catalog.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     If the <paramref name="itemId" /> is already used by another item.
        /// </exception>
        void IInventoryDataLayer.CreateItem(string key, string itemId, Completer completer)
        {
            Tools.RejectIfArgNullOrEmpty(key, nameof(key), completer);
            Tools.RejectIfArgNullOrEmpty(itemId, nameof(itemId), completer);

            var itemDefinition = m_Catalog.FindItem(key);
            if (itemDefinition == null)
            {
                var reason = new CatalogItemNotFoundException(key);
                completer.Reject(reason);
            }

            if (m_Items.ContainsKey(itemId))
            {
                var reason = new InvalidOperationException(
                    $"An Item with the id \"{itemId}\" already exists.");
                completer.Reject(reason);
            }

            CreateItemNoCheck(itemDefinition, itemId);

            completer.Resolve();
        }

        /// <inheritdoc />
        void IInventoryDataLayer.DeleteItem(string itemId, Completer completer)
        {
            //Requesting the deletion of a non existing item is a silent error.
            DeleteItem(itemId);

            completer.Resolve();
        }

        /// <inheritdoc />
        Property IInventoryDataLayer.GetPropertyValue(string itemId, string propertyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(itemId, nameof(itemId));
            Tools.ThrowIfArgNullOrEmpty(propertyKey, nameof(propertyKey));

            if (!m_Items.TryGetValue(itemId, out var item))
                throw new InventoryItemNotFoundException(itemId);

            foreach (var itemProperty in item.properties)
            {
                if (itemProperty.key == propertyKey)
                    return itemProperty.value;
            }

            throw new PropertyNotFoundException(itemId, propertyKey);
        }

        /// <inheritdoc />
        bool IInventoryDataLayer.TryGetPropertyValue(string itemId, string propertyKey, out Property propertyValue)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || string.IsNullOrWhiteSpace(propertyKey)
                || !m_Items.TryGetValue(itemId, out var item))
            {
                propertyValue = default;

                return false;
            }

            foreach (var itemProperty in item.properties)
            {
                if (itemProperty.key == propertyKey)
                {
                    propertyValue = itemProperty.value;

                    return true;
                }
            }

            propertyValue = default;

            return false;
        }

        /// <inheritdoc />
        void IInventoryDataLayer.SetPropertyValue(string itemId, string propertyKey, Property value, Completer completer)
        {
            if (Tools.RejectIfArgNullOrEmpty(itemId, nameof(itemId), completer))
                return;

            if (Tools.RejectIfArgNullOrEmpty(propertyKey, nameof(propertyKey), completer))
                return;

            if (!m_Items.TryGetValue(itemId, out var item))
            {
                completer.Reject(new InventoryItemNotFoundException(itemId));

                return;
            }

            for (var i = 0; i < item.properties.Length; i++)
            {
                var itemProperty = item.properties[i];
                if (itemProperty.key == propertyKey)
                {
                    if (itemProperty.value.type != value.type)
                    {
                        completer.Reject(
                            new PropertyInvalidCastException(
                                propertyKey, itemProperty.value.type, value.type));
                    }
                    else
                    {
                        //Don't forget to reapply values to collections since we are working with structs.
                        itemProperty.value = value;
                        item.properties[i] = itemProperty;
                        m_Items[item.id] = item;

                        completer.Resolve();
                    }

                    return;
                }
            }

            completer.Reject(new PropertyNotFoundException(itemId, propertyKey));
        }
    }
}
