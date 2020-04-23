using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contract for objects providing data to the <see cref="WalletManager"/>.
    /// </summary>
    public interface IWalletDataLayer
    {
        /// <summary>
        /// Get Wallet's serializable data.
        /// </summary>
        WalletSerializableData GetData();

        /// <summary>
        /// Defines a new balance for the given <paramref name="currencyId"/>
        /// </summary>
        /// <param name="currencyId">The identifier of the currency to update.</param>
        /// <param name="value">The new balance.</param>
        /// <param name="completer">The operation result</param>
        void SetBalance(string currencyId, long value, Completer completer);

        /// <summary>
        /// Increase the balance for the given <paramref name="currencyId"/>
        /// </summary>
        /// <param name="currencyId">The identifier of the currency to update.</param>
        /// <param name="value">The amount to add.</param>
        /// <param name="completer">The operation result.</param>
        void AddBalance(string currencyId, long value, Completer<long> completer);

        /// <summary>
        /// Decrease the balance for the given <paramref name="currencyId"/>
        /// </summary>
        /// <param name="currencyId">The identifier of the currency to update.</param>
        /// <param name="value">The amount to remove.</param>
        /// <param name="completer">The operation result.</param>
        void RemoveBalance(string currencyId, long value, Completer<long> completer);
    }
}
