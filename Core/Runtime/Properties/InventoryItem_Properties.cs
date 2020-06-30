using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    public partial class InventoryItem
    {
        /// <summary>
        ///     Event triggered when a property is updated.
        /// </summary>
        public event Action<PropertyChangedEventArgs> propertyChanged;

        /// <summary>
        ///     Get all properties stored in this item.
        /// </summary>
        /// <returns>
        ///     Return a new dictionary containing all properties stored in this item.
        ///     The returned dictionary is never null.
        /// </returns>
        public Dictionary<string, Property> GetProperties()
        {
            AssertActive();

            var defaultProperties = definition.defaultProperties;
            var properties = new Dictionary<string, Property>(defaultProperties.Count);
            foreach (var defaultEntry in defaultProperties)
            {
                var key = defaultEntry.Key;
                var value = GameFoundation.dataLayer.GetPropertyValue(id, key);
                properties.Add(key, value);
            }

            return properties;
        }

        /// <summary>
        ///     Get all properties stored in this item.
        /// </summary>
        /// <param name="target">
        ///     The dictionary to fill with the properties stored in this item.
        ///     It is cleared before being filled.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the given <paramref name="target" /> is null.
        /// </exception>
        public void GetProperties(Dictionary<string, Property> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));
            AssertActive();

            target.Clear();

            var defaultProperties = definition.defaultProperties;
            foreach (var defaultEntry in defaultProperties)
            {
                var key = defaultEntry.Key;
                var value = GameFoundation.dataLayer.GetPropertyValue(id, key);
                target.Add(key, value);
            }
        }

        /// <summary>
        ///     Check if this item has a property for the given <paramref name="key" />.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the property to look for.
        /// </param>
        /// <returns>
        ///     <c>true</c> if this item has a property with the given <paramref name="key" />;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key" /> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        public bool HasProperty(string key)
        {
            AssertActive();

            //No need to check the argument, InventoryItemDefinition.HasProperty already does.
            return m_Definition.HasProperty(key);
        }

        /// <summary>
        ///     Get the value of the property with the given <paramref name="key" />.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the property to get the value of.
        /// </param>
        /// <returns>
        ///     The value of the property with the given <paramref name="key" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key" /> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no property with the given <paramref name="key" /> in this item.
        /// </exception>
        public Property GetProperty(string key)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            AssertActive();

            return GameFoundation.dataLayer.GetPropertyValue(m_Id, key);
        }

        /// <summary>
        ///     Try to get the value of the property with the given <paramref name="key" />.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the property to get the value of.
        /// </param>
        /// <param name="property">
        ///     The value of the searched property, if found.
        /// </param>
        /// <returns>
        ///     Returns true if a property with the given <paramref name="key" /> exists on this item;
        ///     returns false otherwise.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        public bool TryGetProperty(string key, out Property property)
        {
            AssertActive();

            return GameFoundation.dataLayer.TryGetPropertyValue(m_Id, key, out property);
        }

        /// <summary>
        ///     Set the property with the given <paramref name="key" />
        ///     with the given <paramref name="value" />.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the property to get the value of.
        /// </param>
        /// <param name="value">
        ///     The value to assign to the property.
        /// </param>
        /// <returns>
        ///     The new value of the property.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key" /> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no property with the given <paramref name="key" /> in this item.
        /// </exception>
        /// <exception cref="PropertyInvalidCastException">
        ///     If the given <paramref name="value" /> type is different from the expected type.
        /// </exception>
        public Property SetProperty(string key, Property value)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            AssertActive();

            if (!m_Definition.TryGetDefaultProperty(key, out var storedProperty))
                throw new PropertyNotFoundException(m_Id, key);

            if (storedProperty.type != value.type)
                throw new PropertyInvalidCastException(key, storedProperty.type, value.type);

            SynchronizedSetProperty(key, value);

            return value;
        }

        /// <summary>
        ///     Adjust the value of the property with the given <paramref name="key" />
        ///     by adding the given <paramref name="change" /> to its current value.
        ///     Work only with numeric properties.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the property to get the value of.
        /// </param>
        /// <param name="change">
        ///     Change to apply to the current value of the property.
        /// </param>
        /// <returns>
        ///     The new value of the property.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="key" /> parameter is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="NullReferenceException">
        ///     If this item has been discarded.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no property with the given <paramref name="key" /> in this item.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the property's type isn't a numeric type.
        /// </exception>
        public Property AdjustProperty(string key, Property change)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            AssertActive();

            var storedProperty = GameFoundation.dataLayer.GetPropertyValue(m_Id, key);

            if (!storedProperty.type.IsNumber())
                throw new InvalidOperationException(
                    $"The property \"{key}\" can't be adjusted because it is a \"{storedProperty.type}\" type.");

            if (!change.type.IsNumber())
                throw new InvalidOperationException(
                    "The given property change isn't a numeric type.");

            storedProperty += change;

            SynchronizedSetProperty(key, storedProperty);

            return storedProperty;
        }

        /// <summary>
        ///     Reset the property with the given <paramref name="key" /> to its default value.
        /// </summary>
        /// <param name="key">
        ///     The identifier of the property to reset.
        /// </param>
        /// <returns>
        ///     Return the new property's value after its reset.
        /// </returns>
        /// <exception cref="NullReferenceException">
        ///     If this has been discarded.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="key" /> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="PropertyNotFoundException">
        ///     If there is no property with the given <paramref name="key" /> in this item.
        /// </exception>
        public Property ResetProperty(string key)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));
            AssertActive();

            if (!m_Definition.defaultProperties.TryGetValue(key, out var defaultValue))
                throw new PropertyNotFoundException(m_Id, key);

            SynchronizedSetProperty(key, defaultValue);

            return defaultValue;
        }

        /// <inheritdoc cref="InventoryItemDefinition.GetDefaultProperties()" />
        public Dictionary<string, Property> GetDefaultProperties()
        {
            AssertActive();

            return definition.GetDefaultProperties();
        }

        /// <inheritdoc cref="InventoryItemDefinition.GetDefaultProperties(Dictionary{string, Property})" />
        public void GetDefaultProperties(Dictionary<string, Property> target)
        {
            AssertActive();

            definition.GetDefaultProperties(target);
        }

        /// <inheritdoc cref="InventoryItemDefinition.GetDefaultProperty" />
        public Property GetDefaultProperty(string propertyKey)
        {
            AssertActive();

            return definition.GetDefaultProperty(propertyKey);
        }

        /// <inheritdoc cref="InventoryItemDefinition.TryGetDefaultProperty" />
        public bool TryGetDefaultProperty(string propertyKey, out Property property)
        {
            AssertActive();

            return definition.TryGetDefaultProperty(propertyKey, out property);
        }

        /// <summary>
        ///     Clean properties related event and members.
        /// </summary>
        void CleanProperties()
        {
            propertyChanged = null;
        }

        /// <summary>
        ///     Set the value of the property with the given <paramref name="key" />,
        ///     synchronize it with the Data Access Layer,
        ///     and raise the <see cref="propertyChanged" /> event.
        /// </summary>
        /// <param name="key">
        ///     Identifier of the property to get the value of.
        /// </param>
        /// <param name="value">
        ///     The value to assign to the property.
        /// </param>
        void SynchronizedSetProperty(string key, Property value)
        {
            GameFoundation.dataLayer.SetPropertyValue(m_Id, key, value, Completer.None);

            var args = new PropertyChangedEventArgs(this, key, value);
            propertyChanged?.Invoke(args);
        }
    }
}
