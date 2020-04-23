#if UNITY_EDITOR

using UnityEditor;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class VirtualTransactionAsset
    {
        /// <inheritdoc/>
        internal override string Editor_AssetPrefix => "VirtualTransaction";

        /// <summary>
        /// It cleans the <see cref="VirtualTransactionAsset"/> by removing all
        /// the <see cref="ItemExchangeDefinitionObject"/> using this item.
        /// </summary>
        /// <param name="item"></param>
        protected override void Editor_HandleItemRemoved
            (InventoryItemDefinitionAsset item)
        {
            var itemExchanges = m_Costs.m_Items;
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
        }

        /// <summary>
        /// It cleans the <see cref="VirtualTransactionAsset"/> by removing all
        /// the <see cref="CurrencyExchangeObject"/> using this currency.
        /// </summary>
        /// <param name="item"></param>
        protected override void Editor_HandleCurrencyRemoved
            (CurrencyAsset currency)
        {
            var currencyExchanges = m_Costs.m_Currencies;
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
        }
    }
}

#endif
