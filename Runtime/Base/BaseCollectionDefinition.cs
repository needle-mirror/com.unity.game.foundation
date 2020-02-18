using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes preset values and rules for a Collection by using a CollectionDefinition.
    /// During runtime, it may be useful to refer back to the CollectionDefinition for the presets and rules, 
    /// but the values cannot be changed at runtime (your system may, for example, bypass the presets, 
    /// or calculate new values on the fly with modifiers).
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this CollectionDefinition uses.</typeparam>
    /// <typeparam name="T4">The type of Items this CollectionDefinition uses.</typeparam>
    /// <inheritdoc/>
    public abstract class BaseCollectionDefinition<T1, T2, T3, T4> : GameItemDefinition
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T1, T2, T3, T4>
        where T4 : BaseItem<T1, T2, T3, T4>
    {
        /// <summary>
        /// Items that are added when the collection is created
        /// </summary>
        protected List<DefaultItemDefinition> defaultItems { get; }

        /// <summary>
        /// Constructor to build a BaseCollectionDefinition object.
        /// </summary>
        /// <param name="id">The string id value for the collection definition. Throws error if null, empty or invalid.</param>
        /// <param name="displayName">The readable string display name value for the collection definition. Throws error if null or empty.</param>
        /// <param name="referenceDefinition">The reference GameItemDefinition for this collection definition. Null is an allowed value.</param>
        /// <param name="categories">The list of CategoryDefinition hashes that are the categories applied to this collection definition. If null value is passed in an empty list will be created.</param>
        /// <param name="detailDefinitions">The dictionary of Type, BaseDetailDefinition pairs that are the detail definitions applied to this collection definition. If null value is passed in an empty dictionary will be created.</param>
        /// <param name="defaultItems">The list of DefaultItemDefinitions that are the item definitions that will be automatically instantiated and added to a runtime instance of this collection at its instantiation. If null value is passed in an empty list will be created.</param>
        /// <exception cref="System.ArgumentException">Throws if id or displayName are null or empty or if the id is not valid. Valid ids are alphanumeric with optional dashes or underscores.</exception>
        internal BaseCollectionDefinition(string id, string displayName, GameItemDefinition referenceDefinition = null, List<int> categories = null, Dictionary<Type, BaseDetailDefinition> detailDefinitions = null, List<DefaultItemDefinition> defaultItems = null)
            : base(id, displayName, referenceDefinition, categories, detailDefinitions)
        {
            this.defaultItems = defaultItems ?? new List<DefaultItemDefinition>();
        }

        /// <summary>
        /// Returns an array of the default items in this collection definition.
        /// </summary>
        /// <returns>An array of the default items in this collection definition.</returns>
        public DefaultItemDefinition[] GetDefaultItems()
        {
            return defaultItems?.ToArray();
        }

        /// <summary>
        /// Fills the given list with all default items in this collection definition.
        /// Note: this returns the current state of default items in this collection definition.
        /// To ensure that there are no invalid or duplicate entries, the 'defaultItem' list will
        /// always be cleared and 'recycled' (i.e. updated) with current data from the definition.
        /// </summary>
        /// <param name="defaultItems">The list to clear and write all default items into.</param>
        public void GetDefaultItems(List<DefaultItemDefinition> defaultItems)
        {
            if (defaultItems == null)
            {
                return;
            }

            defaultItems.Clear();

            if (this.defaultItems == null)
            {
                return;
            }
            
            defaultItems.AddRange(this.defaultItems);
        }

        /// <summary>
        /// Spawns an instance of a Collection that is based off of this CollectionDefinition.
        /// </summary>
        /// <returns>The reference to the newly created Collection.</returns>
        internal abstract T2 CreateCollection(string collectionId, string displayName, int gameItemId = 0);
    }
}
