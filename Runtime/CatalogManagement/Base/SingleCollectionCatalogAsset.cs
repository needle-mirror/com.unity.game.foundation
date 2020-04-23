using System.Collections.Generic;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// The base class of the catalog storing only one kind of
    /// <see cref="CatalogItemAsset"/>.
    /// This kind is obviously described by <typeparamref name="TItemAsset"/>.
    /// Most of the catalogs are single collections at the moment.
    /// </summary>
    /// <typeparam name="TItemAsset">The type of
    /// <see cref="CatalogItemAsset"/> this catalog provides</typeparam>
    public abstract partial class SingleCollectionCatalogAsset<TItemAsset>
        : BaseCatalogAsset where TItemAsset : CatalogItemAsset
    {
        /// <summary>
        /// Temporary list of <see cref="TItem"/> for internal optimization.
        /// </summary>
        static protected readonly List<TItemAsset> s_TempList = new List<TItemAsset>();

        /// <summary>
        /// The list of <see cref="TItemAsset"/> this catalog stores.
        /// </summary>
        [SerializeField]
        internal List<TItemAsset> m_Items;

        /// <inheritdoc />
        protected sealed override void AwakeCatalog()
        {
            if (m_Items is null)
            {
                m_Items = new List<TItemAsset>();
            }
        }

        /// <summary>
        /// Initializes the catalog.
        /// </summary>
        protected sealed override void InitializeCatalog()
        {
#if UNITY_EDITOR
            Editor_InitializeCatalog();
#endif

            InitializeSingleCollectionCatalog();
        }

        protected virtual void InitializeSingleCollectionCatalog() { }

        /// <summary>
        /// Tells whether an item with the specified <paramref name="id"/>
        /// exists or not.
        /// </summary>
        /// <param name="id">The id of the <typeparamref name="TItemAsset"/>
        /// instance to find.</param>
        /// <returns><c>true</c> if the item is found, <c>false</c>
        /// otherwise.</returns>
        public bool ContainsItem(string id) => FindItem(id) != null;

        /// <summary>
        /// Returns an array of all the <typeparamref name="TItemAsset"/>
        /// instances of this catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetItems(ICollection{TItemAsset})"/> instead.
        /// </remarks>
        /// <returns>An array of all item definitions.</returns>
        public TItemAsset[] GetItems() => GFTools.ToArray(m_Items);

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItemAsset"/> instances of this catalog and
        /// returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <typeparamref name="TItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/> instances
        /// of this catalog.</returns>
        public int GetItems(ICollection<TItemAsset> target = null)
            => GFTools.Copy(m_Items, target);

        /// <summary>
        /// Returns an array of all the
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog.
        /// </summary>
        /// <typeparam name="TInheritedItemAsset">The type of items to
        /// find.</typeparam>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetItems{TInheritedItemAsset}(ICollection{TInheritedItemAsset})"/>
        /// instead.
        /// </remarks>
        /// <returns>An array of all item definitions.</returns>
        public TInheritedItemAsset[] GetItems<TInheritedItemAsset>()
            where TInheritedItemAsset : TItemAsset
        {
            var count = GetItems<TInheritedItemAsset>(null);
            var items = new List<TInheritedItemAsset>(count);
            GetItems(items);
            return items.ToArray();
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItemAssetAsset"/> instances of this catalog and
        /// returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <typeparamref name="TItemAssetAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/> instances
        /// of this catalog.</returns>
        public int GetItems<TInheritedItemAsset>
            (ICollection<TInheritedItemAsset> target = null)
        {
            target?.Clear();

            var count = 0;
            foreach (var item in m_Items)
            {
                if (item is TInheritedItemAsset typedItem)
                {
                    count++;
                    target?.Add(typedItem);
                }
            }

            return count;
        }

        /// <summary>
        /// Looks for a <typeparamref name="TItemAsset"/> instance by its
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier of the
        /// <typeparamref name="TItemAsset"/> to find.</param>
        /// <returns>The requested <typeparamref name="TItemAsset"/>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="id"/>
        /// parameter is null, empty or whitespace</exception>
        public TItemAsset FindItem(string id)
        {
            GFTools.ThrowIfArgNull(id, nameof(id));

            foreach (var item in m_Items)
            {
                if (item.id == id)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the given <paramref name="category"/> and returns their
        /// count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="category">The <see cref="CategoryAsset"/> instance used
        /// as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TInheritedItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the category filter in this catalog.</returns>
        internal int FindItemsByCategoryInternal<TInheritedItemAsset>
            (CategoryAsset category, ICollection<TItemAsset> target = null)
            where TInheritedItemAsset : TItemAsset
        {
            GFTools.ThrowIfArgNull(category, nameof(category));

            var count = 0;

            target?.Clear();

            foreach (var item in m_Items)
            {
                if (item.HasCategory(category))
                {
                    if (item is TInheritedItemAsset typedItem)
                    {
                        count++;
                        target?.Add(typedItem);
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItemAsset"/> instances of this catalog matching the
        /// given <paramref name="category"/> and returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> instance used as
        /// a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/> instances
        /// matching the category filter in this catalog.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="category"/> parameter is null</exception>
        public int FindItemsByCategory
            (CategoryAsset category, ICollection<TItemAsset> target = null)
        {
            GFTools.ThrowIfArgNull(category, nameof(category));
            return FindItemsByCategoryInternal<TItemAsset>(category, target);
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItemAsset"/> instances of this catalog matching
        /// the <see cref="CategoryAsset"/> by its <paramref name="id"/> and
        /// returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="CategoryAsset"/>
        /// instance used as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/> instances
        /// matching the category filter in this catalog.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="id"/> parameter is null, empty, or
        /// whitespace.</exception>
        public int FindItemsByCategory
            (string id, ICollection<TItemAsset> target = null)
        {
            var category = GetCategoryOrDie(id, nameof(id));
            return FindItemsByCategoryInternal<TItemAsset>(category, target);
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the given <paramref name="category"/> and returns their
        /// count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="category">The <see cref="CategoryAsset"/> instance used
        /// as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TInheritedItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the category filter in this catalog.</returns>
        public int FindItemsByCategory<TInheritedItemAsset>
            (CategoryAsset category, ICollection<TItemAsset> target = null)
            where TInheritedItemAsset : TItemAsset
        {
            GFTools.ThrowIfArgNull(category, nameof(category));
            return FindItemsByCategoryInternal<TInheritedItemAsset>(category, target);
        }

        /// <summary>
        /// Returns an array of all the
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the given <paramref name="category"/>
        /// </summary>
        /// <param name="category">The <see cref="CategoryAsset"/> instance used
        /// as a filter</param>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the category filter in this catalog.</returns>
        public TItemAsset[] FindItemsByCategory<TInheritedItemAsset>
            (CategoryAsset category) where TInheritedItemAsset : TItemAsset
        {
            GFTools.ThrowIfArgNull(category, nameof(category));

            try
            {
                FindItemsByCategoryInternal<TInheritedItemAsset>
                    (category, s_TempList);

                return s_TempList.ToArray();
            }
            finally
            {
                s_TempList.Clear();
            }
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the <paramref name="category"/> by its
        /// <paramref name="id"/> and returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="category">The <see cref="CategoryAsset"/> instance used
        /// as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TInheritedItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the category filter in this catalog.</returns>
        public int FindItemsByCategory<TInheritedItemAsset>
            (string id, ICollection<TItemAsset> target = null)
            where TInheritedItemAsset : TItemAsset
        {
            var category = GetCategoryOrDie(id, nameof(id));
            return FindItemsByCategoryInternal<TInheritedItemAsset>
                (category, target);
        }

        /// <summary>
        /// Returns an array of all the
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the <see cref="CategoryAsset"/> by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">The <see cref="CategoryAsset"/> instance used
        /// as a filter</param>
        /// <typeparam name="TInheritedItemAsset"></typeparam>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the category filter in this catalog.</returns>
        public TItemAsset[] FindItemsByCategory<TInheritedItemAsset>
            (string id) where TInheritedItemAsset : TItemAsset
        {
            var category = GetCategoryOrDie(id, nameof(id));
            try
            {
                FindItemsByCategoryInternal<TInheritedItemAsset>
                    (category, s_TempList);

                return s_TempList.ToArray();
            }
            finally
            {
                s_TempList.Clear();
            }
        }

        /// <summary>
        /// Returns an array of all the
        /// <typeparamref name="TItemAsset"/> instances of this catalog
        /// matching the given <paramref name="category"/>
        /// </summary>
        /// <param name="id">The <see cref="CategoryAsset"/> instance used
        /// as a filter</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/>
        /// instances matching the category filter in this catalog.</returns>
        public TItemAsset[] FindItemsByCategory(CategoryAsset category)
        {
            GFTools.ThrowIfArgNull(category, nameof(category));

            try
            {
                FindItemsByCategory(category, s_TempList);
                return s_TempList.ToArray();
            }
            finally
            {
                s_TempList.Clear();
            }
        }

        /// <summary>
        /// Returns an array of all the
        /// <typeparamref name="TItemAsset"/> instances of this catalog
        /// matching the <see cref="CategoryAsset"/> by its <paramref name="id"/>
        /// </summary>
        /// <param name="id">The <see cref="CategoryAsset"/> instance used
        /// as a filter</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/>
        /// instances matching the category filter in this catalog.</returns>
        public TItemAsset[] FindItemsByCategory(string id)
        {
            var category = GetCategoryOrDie(id, nameof(id));
            return FindItemsByCategory(category);
        }

        /// <inheritdoc />
        protected sealed override void ConfigureCatalog(CatalogBuilder builder)
        {
            foreach (var itemAsset in m_Items)
            {
                itemAsset.Configure(builder);
            }

            ConfigureSingleCollectionCatalog(builder);
        }

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the
        /// specific content of this <see cref="SingleCollectionCatalog{TItem}"/>
        /// isntance.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        protected virtual void ConfigureSingleCollectionCatalog(CatalogBuilder builder) { }
    }
}
