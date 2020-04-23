using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contract for objects providing data to all Game Foundation's
    /// systems (InventoryManager, StatManager, ...).
    /// </summary>
    public interface IDataAccessLayer :
        ICatalogConfigurator,
        IInventoryDataLayer,
        IStatDataLayer,
        IWalletDataLayer,
        ITransactionDataLayer
    {
        /// <summary>
        /// Initialize this data layer.
        /// </summary>
        /// <param name="completer">When done, this completer is resolved or rejected</param>
        void Initialize(Completer completer);
    }
}
