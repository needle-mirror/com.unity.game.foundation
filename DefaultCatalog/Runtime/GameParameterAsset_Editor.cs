#if UNITY_EDITOR
namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class GameParameterAsset
    {
        protected override void OnItemDestroy()
        {
            if (catalog is null) return;
            (catalog as GameParameterCatalogAsset).Editor_RemoveItem(this);
        }
    }
}
#endif
