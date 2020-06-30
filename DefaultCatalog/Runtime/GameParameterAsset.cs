using UnityEngine.GameFoundation.Configs;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class GameParameterAsset : CatalogItemAsset
    {
#if UNITY_EDITOR
        internal override string Editor_AssetPrefix => "GameParameter";
#endif

        protected override CatalogItemConfig ConfigureItem(CatalogBuilder builder)
            => builder.Create<GameParameterConfig>(key);
    }
}
