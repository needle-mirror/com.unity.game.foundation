namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// The abstract ItemDefinition used to create Items which stores
    /// all needed constant data for an Item.  ItemDefinitions should
    /// only exist in CollectionDefinitions.
    /// </summary>
    /// <typeparam name="T1">The type of CollectinDefinitions this BaseItemDefinition uses.</typeparam>
    /// <typeparam name="T2">The type of ItemDefinitions this BaseItemDefinition uses.</typeparam>
    /// <inheritdoc/>
    public abstract class BaseItemDefinition<T1, T2> : GameItemDefinition
        where T1 : BaseCollectionDefinition<T1, T2>
        where T2 : BaseItemDefinition<T1, T2>
    {
    }
}
