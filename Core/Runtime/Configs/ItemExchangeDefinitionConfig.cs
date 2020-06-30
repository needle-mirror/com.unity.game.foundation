using System;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    /// Configurator for an <see cref="ItemExchangeDefinition"/> data.
    /// </summary>
    public struct ItemExchangeDefinitionConfig
    {
        /// <summary>
        /// The identifier of an <see cref="InventoryItemDefinition"/>.
        /// </summary>
        public string item;

        /// <summary>
        /// The amount of this item definition.
        /// </summary>
        public long amount;

        /// <summary>
        /// Checks the configuration and builds the <see cref="ItemExchangeDefinition"/>
        /// data.
        /// </summary>
        /// <returns>The newly created <see cref="ItemExchangeDefinition"/> data.</returns>
        internal ItemExchangeDefinition Compile()
        {
            Tools.ThrowIfArgNullOrEmpty(item, nameof(item));
            Tools.ThrowIfArgNegative(amount, nameof(amount));

            var exchange = new ItemExchangeDefinition();
            exchange.amount = amount;
            return exchange;
        }

        /// <summary>
        /// Resolves the possible references the <see cref="ItemExchangeDefinition"/> may
        /// contain.
        /// </summary>
        /// <param name="builder">The builder where the references can be
        /// found.</param>
        /// <param name="exchange">The <see cref="ItemExchangeDefinition"/> data to
        /// link.</param>
        internal void Link(CatalogBuilder builder, ref ItemExchangeDefinition exchange)
        {
            var catalogItem = builder.GetItemOrDie(item);

            if (!(catalogItem is InventoryItemDefinitionConfig inventoryItemConfig))
            {
                throw new Exception
                    ($"{nameof(CatalogItemConfig)} {item} is not a valid {nameof(InventoryItemDefinitionConfig)}");
            }

            exchange.item = inventoryItemConfig.runtimeItem;
        }
    }
}
