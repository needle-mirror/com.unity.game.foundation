using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Preset values and rules for an InventoryItem.
    ///     During runtime, it may be useful to refer back to the InventoryItemDefinition for
    ///     the presets and rules, but the values cannot be changed at runtime.
    ///     InventoryItemDefinitions are also used as factories to create InventoryItems.
    /// </summary>
    public class InventoryItemDefinition : CatalogItem
    {
        /// <summary>
        ///     Stores all properties default values for inventory items.
        ///     Key: Property's key.
        ///     Value: Property's type & default value.
        /// </summary>
        internal Dictionary<string, Property> defaultProperties { get; }

        internal InventoryItemDefinition()
        {
            defaultProperties = new Dictionary<string, Property>();
        }

        internal InventoryItemDefinition(IDictionary<string, Property> properties)
        {
            defaultProperties = new Dictionary<string, Property>(properties);
        }

        /// <summary>
        ///     Get all default properties stored in this definition.
        /// </summary>
        /// <returns>
        ///     Return a new dictionary containing all properties stored in this definition.
        ///     The returned dictionary is never null.
        /// </returns>
        public Dictionary<string, Property> GetDefaultProperties()
        {
            var propertiesData = new Dictionary<string, Property>(defaultProperties);

            return propertiesData;
        }

        /// <summary>
        ///     Get all default properties stored in this definition.
        /// </summary>
        /// <param name="target">
        ///     The dictionary to fill with the properties stored in this definition.
        ///     It is cleared before being filled.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the given <paramref name="target" /> is null.
        /// </exception>
        public void GetDefaultProperties(Dictionary<string, Property> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));

            target.Clear();

            foreach (var propertyEntry in defaultProperties)
            {
                target.Add(propertyEntry.Key, propertyEntry.Value);
            }
        }

        /// <summary>
        ///     Check if this definition has a property with the given <paramref name="propertyKey" />.
        /// </summary>
        /// <param name="propertyKey">
        ///     The identifier of the property to look for.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this definition has a property with the given <paramref name="propertyKey" />;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the given <paramref name="propertyKey" /> is null, empty, or whitespace.
        /// </exception>
        public bool HasProperty(string propertyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(propertyKey, nameof(propertyKey));

            return defaultProperties.ContainsKey(propertyKey);
        }

        /// <summary>
        ///     Get the default value of the property with the given <paramref name="propertyKey" />.
        /// </summary>
        /// <param name="propertyKey">
        ///     Identifier of the property to get the value of.
        /// </param>
        /// <returns>
        ///     The default value of the property with the given <paramref name="propertyKey" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the given <paramref name="propertyKey" /> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     Thrown if there is no property with the given <paramref name="propertyKey" /> in this item.
        /// </exception>
        public Property GetDefaultProperty(string propertyKey)
        {
            Tools.ThrowIfArgNullOrEmpty(propertyKey, nameof(propertyKey));

            if (!defaultProperties.TryGetValue(propertyKey, out var property))
                throw new PropertyNotFoundException(key, propertyKey);

            return property;
        }

        /// <summary>
        ///     Try to get the default value of the property with the given <paramref name="propertyKey" />.
        /// </summary>
        /// <param name="propertyKey">
        ///     Identifier of the property to get the value of.
        /// </param>
        /// <param name="property">
        ///     The default value of the searched property, if found.
        /// </param>
        /// <returns>
        ///     Returns true if a property with the given <paramref name="propertyKey" /> exists on this item;
        ///     returns false otherwise.
        /// </returns>
        public bool TryGetDefaultProperty(string propertyKey, out Property property)
        {
            if (propertyKey == null)
            {
                property = default;

                return false;
            }

            return defaultProperties.TryGetValue(propertyKey, out property);
        }
    }
}
