namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    /// Configurator for an <see cref="InventoryItemDefinition"/> instance.
    /// </summary>
    public sealed class InventoryItemDefinitionConfig : CatalogItemConfig<InventoryItemDefinition>
    {
        /// <inheritdoc/>
        protected internal override InventoryItemDefinition CompileItem()
            => new InventoryItemDefinition();
    }
}