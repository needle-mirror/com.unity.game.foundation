namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    /// Base configurator for a <see cref="BaseDetail"/> instance.
    /// </summary>
    public abstract class BaseDetailConfig
    {
        /// <summary>
        /// A reference to the <see cref="BaseDetail"/> to build.
        /// </summary>
        internal BaseDetail runtimeDetail;

        /// <summary>
        /// Checks the configuration and creates the <see cref="BaseDetail"/>
        /// instance.
        /// This method doesn't check the references this detail could contain.
        /// <seealso cref="Link(CatalogBuilder, CatalogItem)"/>
        /// </summary>
        /// <returns>The newly create <see cref="BaseDetail"/>
        /// instance.</returns>
        internal void Compile() => runtimeDetail = CompileGeneric();

        /// <inheritdoc cref="LinkGeneric(CatalogBuilder)"/>
        /// <param name="owner">The <see cref="CatalogItem"/> instance owning
        /// this detail.</param>
        internal void Link(CatalogBuilder builder, CatalogItem owner)
        {
            LinkGeneric(builder);
            runtimeDetail.owner = owner;
        }

        /// <inheritdoc cref="Compile"/>
        protected abstract BaseDetail CompileGeneric();

        /// <summary>
        /// Resolves the possible references the <see cref="BaseDetail"/> may
        /// contain and builds them.
        /// </summary>
        /// <param name="builder">The builder of the catalogs, where the
        /// references can be found</param>
        protected virtual void LinkGeneric(CatalogBuilder builder) { }
    }

    /// <summary>
    /// Base configurator for a <see cref="TDetail"/> instance.
    /// </summary>
    /// <typeparam name="TDetail">The type of the detail to build.</typeparam>
    public abstract class BaseDetailConfig<TDetail> : BaseDetailConfig
        where TDetail : BaseDetail
    {
        /// <summary>
        /// Gets the <typeparamref name="TDetail"/> built by this configurator.
        /// </summary>
        internal new TDetail runtimeDetail => base.runtimeDetail as TDetail;

        /// <inheritdoc/>
        protected sealed override BaseDetail CompileGeneric()
            => CompileDetail();

        /// <inheritdoc/>
        protected sealed override void LinkGeneric(CatalogBuilder builder)
            => LinkDetail(builder);

        /// <summary>
        /// Checks the data and creates the <typeparamref name="TDetail"/>
        /// instance.
        /// </summary>
        /// <returns>The newly created <typeparamref name="TDetail"/>
        /// instance.</returns>
        protected abstract TDetail CompileDetail();

        /// <summary>
        /// Resolves the possible references the <typeparamref name="TDetail"/>
        /// can contain and builds them.
        /// </summary>
        /// <param name="builder">The builder of the catalogs, where the
        /// references can be found</param>
        protected virtual void LinkDetail(CatalogBuilder builder) { }
    }
}
