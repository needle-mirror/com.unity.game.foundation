namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Enum to specify the type of value stored in this StatDefinition.
    /// </summary>
    public enum StatValueType : int
    {
        /// <summary>
        /// Stat value is of type int.
        /// </summary>
        Int,

        /// <summary>
        /// Stat value is of type float.
        /// </summary>
        Float,
    }

    /// <summary>
    /// The definition of a stat, attached to <see cref="CatalogItem"/>.
    /// </summary>
    public class StatDefinition
    {        
        /// <summary>
        /// The identifier of this <see cref="StatDefinition"/> instance.
        /// </summary>
        public string id { get; }

        /// <summary>
        /// Custom display name (string) attached to this
        /// <see cref="StatDefinition"/> instance.
        /// </summary>
        public string displayName { get; }

        /// <summary>
        /// The type of this for this <see cref="StatDefinition"/> instance.
        /// Determines if stat stores an int or a float.
        /// </summary>
        public StatValueType statValueType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatDefinition"/>
        /// class.
        /// </summary>
        /// <param name="id">The identifier this <see cref="StatDefinition"/>
        /// instance.</param>
        /// <param name="displayName">Custom display name (string) attached to
        /// this <see cref="StatDefinition"/> instance.</param>
        /// <param name="statValueType">The type of this for this
        /// <see cref="StatDefinition"/> instance.
        /// Determines if stat stores an int or a float.</param>
        /// <exception cref="System.ArgumentException">Throws if id parameter is
        /// null, empty, or not valid. Valid ids are alphanumeric with optional
        /// dashes or underscores.</exception>
        internal StatDefinition(string id, string displayName, StatValueType statValueType)
        {
            this.id = id;
            this.displayName = displayName;
            this.statValueType = statValueType;
        }

        /// <summary>
        /// Tells whether or not the given <paramref name="type"/> matches the
        /// internal <see cref="statValueType"/>.
        /// </summary>
        /// <param name="type">The type of test.</param>
        /// <returns><c>true</c> if the <paramref name="type"/> matches,
        /// <c>false</c> otherwise.</returns>
        internal bool DoesValueTypeMatch(System.Type type)
        {
            switch (statValueType)
            {
                case StatValueType.Int:
                    return type == typeof(int);
                case StatValueType.Float:
                    return type == typeof(float);
                default:
                    throw new System.InvalidOperationException("Invalid type passed to DoesValueTypeMatch");
            }
        }
    }
}
