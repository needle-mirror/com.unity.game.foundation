using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// DefaultCollectionDefinitions contain preset values and rules for a 
    /// Collection by using a CollectionDefinition. During runtime, it may 
    /// be useful to refer back to the DefaultCollectionDefinition for the 
    /// presets and rules, but the values cannot be changed at runtime (your 
    /// system may, for example, bypass the presets, or calculate new values 
    /// on the fly with modifiers).
    /// </summary>
    public class DefaultCollectionDefinition
    {
        /// <summary>
        /// The string id of this DefaultCollectionDefinition.
        /// </summary>
        /// <returns>The string Id of this DefaultCollectionDefinition.</returns>
        public string id { get; }

        /// <summary>
        /// The Hash of this DefaultCollectionDefinition's Id.
        /// </summary>
        /// <returns>The Hash of this DefaultCollectionDefinition's Id.</returns>
        public int hash { get; }

        /// <summary>
        /// The name of this DefaultCollectionDefinition for the user to display.
        /// </summary>
        /// <returns>The name of this DefaultCollectionDefinition for the user to display.</returns>
        public string displayName { get; }

        /// <summary>
        /// CollectionDefinition used for this DefaultCollectionDefinition.
        /// </summary>
        /// <returns>CollectionDefinition used for this DefaultCollectionDefinition.</returns>
        public int collectionDefinitionHash { get; }

        /// <summary>
        /// Constructor for setting up the id, display name, and the hash.
        /// </summary>
        /// <param name="id">The id this will use.</param>
        /// <param name="displayName">The display name this will use.</param>
        /// <param name="baseCollectionDefinitionHash">The hash of the collectionDefinition referenced by this defaultCollectionDefinition.</param>
        /// <exception cref="ArgumentException">Thrown if given id or display name is null or empty or if the id is invalid.  Valid ids are alphanumeric with optional dashes or underscores.</exception>
        internal DefaultCollectionDefinition(string id, string displayName, int baseCollectionDefinitionHash)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("DefaultCollectionDefinition cannot have null or empty id.");
            }

            if (!Tools.IsValidId(id))
            {
                throw new ArgumentException("DefaultCollectionDefinition id must be alphanumeric. Dashes (-) and underscores (_) allowed.");
            }

            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentException("DefaultCollectionDefinition cannot have null or empty displayName.");
            }

            this.displayName = displayName;
            this.id = id;
            hash = Tools.StringToHash(this.id);
            collectionDefinitionHash = baseCollectionDefinitionHash;
        }
    }
}
