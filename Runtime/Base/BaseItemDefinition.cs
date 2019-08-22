using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// The abstract ItemDefinition used to create Items which stores
    /// all needed constant data for an Item.  ItemDefinitions should
    /// only exist in CollectionDefinitions.
    /// </summary>
    /// <typeparam name="T3">The type of ItemDefinitions this BaseItemDefinition uses.</typeparam>
    /// <typeparam name="T4">The type of Items this BaseItemDefinition uses.</typeparam>
    public abstract class BaseItemDefinition<T3, T4> : GameItemDefinition
        where T3 : BaseItemDefinition<T3, T4>
        where T4 : BaseItem<T3, T4>
    {        
        /// <summary>
        /// This will spawn a new Item based off of this ItemDefinition.
        /// </summary>
        /// <returns>Reference to the newly created Item.</returns>
        internal abstract T4 CreateItem();
    }
}
