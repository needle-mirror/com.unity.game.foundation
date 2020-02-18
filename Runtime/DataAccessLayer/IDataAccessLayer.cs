using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contract for objects providing data to all Game Foundation's
    /// systems (GameItemLookup, InventoryManager, StatManager, ...).
    /// Note that usually they are also responsible for persisting these data.
    /// </summary>
    public interface IDataAccessLayer :
        IInventoryDataLayer,
        IStatDataLayer,
        IGameItemLookupDataLayer
    {
        /// <summary>
        /// Initialize this data layer.
        /// </summary>
        /// <param name="completer">When done, this completer is resolved or rejected</param>
        void Initialize(Completer completer);
    }
}
