namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This is one entry in the list of possible stats an item could have.
    /// </summary>
    public class StatDefinition
    {
        /// <summary>
        /// Enum to determine the value type for this StatDefinition
        /// </summary>
        public enum StatValueType
        {
            /// <summary>
            /// Stats that use ints.
            /// </summary>
            Int,
            
            /// <summary>
            /// Stats that use floats.
            /// </summary>
            Float,
        }
        
        /// <summary>
        /// Id for this Stat definition.
        /// </summary>
        /// <returns>id for this Stat definition.</returns>
        public string id { get; }

        /// <summary>
        /// Hash for Id string for this Stat definition.
        /// </summary>
        /// <returns>Hash for Id string for this Stat definition.</returns>
        public int idHash { get; }

        /// <summary>
        /// Custom string attached to this Stat definition.
        /// </summary>
        /// <returns>Custom string attached to this Stat definition.</returns>
        public string displayName { get; }

        /// <summary>
        /// Stat value type for this Stat definition.
        /// </summary>
        /// <returns>Stat value type for this Stat definition.</returns>
        public StatValueType statValueType { get; }

        /// <summary>
        /// This is one entry in the list of possible Stats an item could have.
        /// </summary>
        /// <param name="id">The Id this StatDefinition will use.</param>
        /// <param name="displayName">The name this StatDefinition will use.</param>
        /// <param name="statValueType">The value type this StatDefinition will hold.</param>
        /// <exception cref="System.ArgumentException">Throws if id parameter is null, empty, or not valid. Valid ids are alphanumeric with optional dashes or underscores.</exception>
        internal StatDefinition(string id, string displayName, StatValueType statValueType)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentException("StatDefinition cannot have null or empty ids.");
            }

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("StatDefinition must be alphanumeric. Dashes (-) and underscores (_) allowed.");
            }
            
            this.id = id;
            idHash = Tools.StringToHash(id);
            this.displayName = displayName;
            this.statValueType = statValueType;
        }

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
