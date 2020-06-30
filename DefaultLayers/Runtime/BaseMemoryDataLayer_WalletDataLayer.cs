using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation.DefaultLayers
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
        protected void InitializeWalletDataLayer(WalletData data, CurrencyCatalogAsset catalog)
        {
            m_WalletDataLayer = new WalletDataLayer(data, catalog);
        }

        /// <inheritdoc />
        WalletData IWalletDataLayer.GetData() =>
            (m_WalletDataLayer as IWalletDataLayer).GetData();

        /// <inheritdoc />
        void IWalletDataLayer.SetBalance
            (string currencyKey, long value, Completer completer) =>
            (m_WalletDataLayer as IWalletDataLayer)
            .SetBalance(currencyKey, value, completer);

        /// <inheritdoc />
        void IWalletDataLayer.AddBalance
            (string currencyKey, long value, Completer<long> completer) =>
            (m_WalletDataLayer as IWalletDataLayer)
            .AddBalance(currencyKey, value, completer);

        /// <inheritdoc />
        void IWalletDataLayer.RemoveBalance
            (string currencyKey, long value, Completer<long> completer) =>
            (m_WalletDataLayer as IWalletDataLayer)
            .RemoveBalance(currencyKey, value, completer);
    }
}
