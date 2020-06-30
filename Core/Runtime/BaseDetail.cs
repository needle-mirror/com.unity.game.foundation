namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// <see cref="BaseDetail"/> is the base container for the additional static
    /// data of a <see cref="CatalogItem"/>.
    /// </summary>
    public abstract class BaseDetail
    {
        /// <summary>
        /// A reference to the <see cref="CatalogItem"/> instance owning this
        /// detail.
        /// </summary>
        /// <returns>The <see cref="CatalogItem"/> instance this detail is
        /// attached to.</returns>
        public CatalogItem owner { get; internal set; }
    }
}
