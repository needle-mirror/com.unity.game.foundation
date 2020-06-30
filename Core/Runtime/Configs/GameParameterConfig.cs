namespace UnityEngine.GameFoundation.Configs
{
    public class GameParameterConfig : CatalogItemConfig<GameParameter>
    {
        /// <inheritdoc/>
        protected internal override GameParameter CompileItem() => new GameParameter();
    }
}
