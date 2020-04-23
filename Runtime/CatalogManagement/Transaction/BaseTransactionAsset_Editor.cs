#if UNITY_EDITOR

using UnityEditor;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class BaseTransactionAsset
    {
        /// <summary>
        /// Called when an <see cref="InventoryItemDefinitionAsset"/> is removed
        /// from the <see cref="InventoryCatalogAsset"/>.
        /// It cleans the <see cref="BaseTransactionAsset"/> by removing all the
        /// <see cref="ItemExchangeDefinitionObject"/> using this item.
        /// </summary>
        /// <param name="item">The item removed.</param>
        internal void Editor_OnItemRemoved(InventoryItemDefinitionAsset item)
        {
            var itemExchanges = m_Rewards.m_Items;
            for (var i = 0; i < itemExchanges.Count;)
            {
                var exchange = itemExchanges[i];
                if (exchange.item == item)
                {
                    //Debug.Log($"{displayName} ({GetType().Name}) has a link to {item.displayName} ({item.GetType().Name}). Updating…");
                    itemExchanges.RemoveAt(i);
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    i++;
                }
            }

            Editor_HandleItemRemoved(item);
        }

        /// <summary>
        /// Called when an <see cref="CurrencyAsset"/> is removed
        /// from the <see cref="CurrencyCatalogAsset"/>.
        /// It cleans the <see cref="BaseTransactionAsset"/> by removing all the
        /// <see cref="CurrencyExchangeObject"/> using this currency.
        /// </summary>
        /// <param name="currency">The currency removed.</param>
        internal void Editor_OnCurrencyRemoved(CurrencyAsset currency)
        {
            var currencyExchanges = m_Rewards.m_Currencies;
            for (var i = 0; i < currencyExchanges.Count;)
            {
                var exchange = currencyExchanges[i];
                if (exchange.currency == currency)
                {
                    //Debug.Log($"{displayName} ({GetType().Name}) has a link to {currency.displayName} ({currency.GetType().Name}). Updating…");
                    currencyExchanges.RemoveAt(i);
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    i++;
                }
            }

            Editor_HandleCurrencyRemoved(currency);
        }

        protected virtual void Editor_HandleItemRemoved
            (InventoryItemDefinitionAsset item) { }

        protected virtual void Editor_HandleCurrencyRemoved
            (CurrencyAsset currency) { }

        /// <inheritdoc/>
        protected sealed override void OnItemDestroy()
        {
            if (catalog is null) return;

            //Debug.Log($"Removing {displayName} ({GetType().Name}) from {catalog}");
            (catalog as TransactionCatalogAsset).Editor_RemoveItem(this);
        }
    }
}

#endif
