using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    /// Exception thrown when a wrong <see cref="PropertyType"/> is given to a property.
    /// </summary>
    public class PropertyInvalidCastException : Exception
    {
        /// <summary>
        /// Key of the property that received the wrong property type.
        /// </summary>
        public string propertyKey { get; }

        /// <summary>
        /// The expected property type.
        /// </summary>
        public PropertyType expectedPropertyType { get; }

        /// <summary>
        /// The given property type.
        /// </summary>
        public PropertyType givenPropertyType { get; }

        public PropertyInvalidCastException(string key, PropertyType expectedType, PropertyType givenType)
        {
            propertyKey = key;
            expectedPropertyType = expectedType;
            givenPropertyType = givenType;
        }

        /// <inheritdoc/>
        public override string Message =>
            $"Trying to set a \"{givenPropertyType}\" value to the property \"{propertyKey}\" that only handle \"{expectedPropertyType}\" values.";
    }
}
