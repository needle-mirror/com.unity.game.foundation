using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    /// Base configurator of a <see cref="CatalogItem"/> instance.
    /// </summary>
    public abstract class CatalogItemConfig
    {
        /// <summary>
        /// The <see cref="CatalogItem"/> built by this configurator.
        /// </summary>
        internal CatalogItem runtimeItem;

        /// <summary>
        /// The identifier of the item.
        /// </summary>
        public string id { get; internal set; }

        /// <summary>
        /// The friendly name of the item.
        /// </summary>
        public string displayName;

        /// <summary>
        /// The idenfiers of the categories the item will be linked to.
        /// </summary>
        public readonly List<string> categories = new List<string>();

        /// <summary>
        /// The list of configurators of the details the item will own.
        /// </summary>
        public readonly List<BaseDetailConfig> details = new List<BaseDetailConfig>();

        /// <summary>
        /// Checks the configuration and creates the <see cref="CatalogItem"/>
        /// instance.
        /// This method doesn't check the references this item could contain.
        /// <seealso cref="Link(CatalogBuilder)"/>
        /// </summary>
        /// <returns>The newly create <see cref="BaseDetail"/>
        /// instance.</returns>
        internal void Compile()
        {
            if (!Tools.IsValidId(id))
            {
                throw new Exception
                    ($"{nameof(id)} {id} invalid");
            }

            Tools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            runtimeItem = CompileGeneric();
            runtimeItem.id = id;
            runtimeItem.displayName = displayName;
            runtimeItem.m_Categories = new Category[categories.Count];
            runtimeItem.m_Details = new Dictionary<Type, BaseDetail>(details.Count);

            foreach (var detailConfig in details)
            {
                detailConfig.Compile();
            }
        }

        /// <summary>
        /// Resolves the possible references the <see cref="CatalogItem"/> may
        /// contain and builds them.
        /// </summary>
        /// <param name="builder">The builder of the catalogs, where the
        /// references can be found</param>
        internal void Link(CatalogBuilder builder)
        {
            var categories = new Category[this.categories.Count];
            runtimeItem.m_Categories = categories;
            {
                var index = 0;
                foreach (var categoryId in this.categories)
                {
                    var categoryConfig = builder.GetCategoryOrDie(categoryId);
                    if (categoryConfig is null)
                    {
                        throw new Exception
                            ($"{nameof(CategoryConfig)} {categoryId} not found");
                    }
                    categories[index++] = categoryConfig.runtimeCategory;
                }
            }

            var details = new Dictionary<Type, BaseDetail>();
            runtimeItem.m_Details = details;
            foreach (var detailConfig in this.details)
            {
                detailConfig.Link(builder, runtimeItem);
                var detail = detailConfig.runtimeDetail;
                details.Add(detail.GetType(), detail);
            }

            LinkGeneric(builder);
        }

        /// <summary>
        /// This method is called by <see cref="Compile"/> to checks the
        /// specific configuration of the inherited types.
        /// </summary>
        /// <inheritdoc cref="Compile"/>
        internal protected abstract CatalogItem CompileGeneric();

        /// <summary>
        /// This method is called by <see cref="Compile"/> to checks the
        /// links of the inherited types.
        /// </summary>
        /// <inheritdoc cref="Link(CatalogBuilder)"/>
        internal protected virtual void LinkGeneric(CatalogBuilder builder)
        {}
    }

    /// <inheritdoc/>
    /// <typeparam name="TRuntimeItem">The <typeparamref name="TRuntimeItem"/>
    /// built by this configurator.</typeparam>
    public abstract class CatalogItemConfig<TRuntimeItem> : CatalogItemConfig
        where TRuntimeItem : CatalogItem
    {
        /// <inheritdoc cref="CatalogItemConfig.runtimeItem"/>
        internal new TRuntimeItem runtimeItem
            => base.runtimeItem as TRuntimeItem;

        /// <inheritdoc/>
        protected internal sealed override CatalogItem CompileGeneric()
            => CompileItem();

        /// <inheritdoc/>
        protected internal sealed override void LinkGeneric(CatalogBuilder builder)
            => LinkItem(builder);

        /// <summary>
        /// Checks the configuration and builds the
        /// <typeparamref name="TRuntimeItem"/> instance.
        /// </summary>
        protected internal abstract TRuntimeItem CompileItem();

        /// <summary>
        /// Resolves the possible referenes the
        /// <typeparamref name="TRuntimeItem"/> instance may contain.
        /// </summary>
        /// <param name="builder">The builder where the references can be
        /// found</param>
        protected internal virtual void LinkItem(CatalogBuilder builder)
        {}
    }
}