using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// The abstract ItemDefinition used to create Items which stores
    /// all needed constant data for an Item.  ItemDefinitions should
    /// only exist in CollectionDefinitions.
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this BaseItemDefinition uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this BaseItemDefinition uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this BaseItemDefinition uses.</typeparam>
    /// <typeparam name="T4">The type of Items this BaseItemDefinition uses.</typeparam>
    /// <inheritdoc/>
    public abstract class BaseItemDefinition<T1, T2, T3, T4> : GameItemDefinition
        where T1 : BaseCollectionDefinition<T1,T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T1, T2, T3, T4>
        where T4 : BaseItem<T1, T2, T3, T4>
    {
        /// <summary>
        /// Constructor to build a BaseItemDefinition object.
        /// </summary>
        /// <param name="id">The string id value for this BaseItemDefinition. Throws error if null, empty or invalid.</param>
        /// <param name="displayName">The readable string display name value for this BaseItemDefinition. Throws error if null or empty.</param>
        /// <param name="referenceDefinition">The reference GameItemDefinition for this BaseItemDefinition. Null is an allowed value.</param>
        /// <param name="categories">The list of CategoryDefinition hashes that are the categories applied to this BaseItemDefinition. If null value is passed in an empty list will be created.</param>
        /// <param name="detailDefinitions">The dictionary of Type, BaseDetailDefinition pairs that are the detail definitions applied to this BaseItemDefinition. If null value is passed in an empty dictionary will be created.</param>
        /// <exception cref="System.ArgumentException">Throws if id or displayName are null or empty or if the id is not valid. Valid ids are alphanumeric with optional dashes or underscores.</exception>
        internal BaseItemDefinition(string id, string displayName, GameItemDefinition referenceDefinition = null, List<int> categories = null, Dictionary<Type, BaseDetailDefinition> detailDefinitions = null)
            : base(id, displayName, referenceDefinition, categories, detailDefinitions)
        {
        }
        
        /// <summary>
        /// This will spawn a new Item based off of this ItemDefinition.
        /// </summary>
        /// <returns>Reference to the newly created Item.</returns>
        internal abstract T4 CreateItem(BaseCollection<T1, T2, T3, T4> owner, int gameItemId = 0);
    }
}
