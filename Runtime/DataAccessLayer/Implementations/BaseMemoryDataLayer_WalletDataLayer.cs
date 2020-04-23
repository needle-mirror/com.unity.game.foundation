using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DataAccessLayers
{
    public partial class BaseMemoryDataLayer
    {
        /// <summary>
        /// Part of the data layer dedicated to the wallet management.
        /// </summary>
        internal WalletDataLayer m_WalletDataLayer;

        /// <summary>
        /// Initializes the data layer for <see cref="WalletManager"/>.
        /// </summary>
        /// <param name="data">Wallet's serializable data.</param>
        /// <param name="catalog">The currency catalog used as source of truth.</param>
        protected void InitializeWalletDataLayer(WalletSerializableData data, CurrencyCatalogAsset catalog)
        {
            m_WalletDataLayer = new WalletDataLayer(data, catalog);
        }

        /// <inheritdoc />
        WalletSerializableData IWalletDataLayer.GetData() =>
            (m_WalletDataLayer as IWalletDataLayer).GetData();

        /// <inheritdoc />
        void IWalletDataLayer.SetBalance
            (string currencyId, long value, Completer completer) =>

            (m_WalletDataLayer as IWalletDataLayer)
                .SetBalance(currencyId, value, completer);

        /// <inheritdoc />
        void IWalletDataLayer.AddBalance
            (string currencyId, long value, Completer<long> completer) =>

            (m_WalletDataLayer as IWalletDataLayer)
                .AddBalance(currencyId, value, completer);

        /// <inheritdoc />
        void IWalletDataLayer.RemoveBalance
            (string currencyId, long value, Completer<long> completer) =>

            (m_WalletDataLayer as IWalletDataLayer)
                .RemoveBalance(currencyId, value, completer);
    }
}
