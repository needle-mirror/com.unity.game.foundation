using System;
using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Description for a <see cref="ItemExchangeDefinition"/>
    /// </summary>
    [Serializable]
    public class ItemExchangeDefinitionObject
    {
        /// <inheritdoc cref="item"/>
        [SerializeField]
        internal InventoryItemDefinitionAsset m_Item;

        /// <inheritdoc cref="amount"/>
        [SerializeField]
        internal long m_Amount;

        /// <summary>
        /// The <see cref="InventoryItemDefinitionAsset"/> of the exchange.
        /// </summary>
        public InventoryItemDefinitionAsset item => m_Item;

        /// <summary>
        /// The amount of the item defintion of the exchange.
        /// </summary>
        public long amount => m_Amount;

        /// <summary>
        /// Creates a <see cref="ItemExchangeDefinitionConfig"/> from the data of this
        /// <see cref="ItemExchangeDefinitionObject"/>.
        /// </summary>
        /// <returns>The config.</returns>
        internal ItemExchangeDefinitionConfig Configure() =>
            new ItemExchangeDefinitionConfig
            {
                item = m_Item.key,
                amount = m_Amount
            };
    }
}
