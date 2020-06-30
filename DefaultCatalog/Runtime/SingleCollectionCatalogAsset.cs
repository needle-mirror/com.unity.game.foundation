using System.Collections.Generic;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
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
        protected static readonly List<TItemAsset> s_TempList = new List<TItemAsset>();

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
            InitializeSingleCollectionCatalog();
        }

        protected virtual void InitializeSingleCollectionCatalog() { }

        /// <summary>
        /// Tells whether an item with the specified <paramref name="key"/>
        /// exists or not.
        /// </summary>
        /// <param name="key">The identifier of the <typeparamref name="TItemAsset"/>
        /// instance to find.</param>
        /// <returns><c>true</c> if the item is found, <c>false</c>
        /// otherwise.</returns>
        public bool ContainsItem(string key) => FindItem(key) != null;

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
        public int GetItems(ICollection<TItemAsset> target)
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
        /// <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The identifier of the
        /// <typeparamref name="TItemAsset"/> to find.</param>
        /// <returns>The requested <typeparamref name="TItemAsset"/>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="key"/>
        /// parameter is null, empty or whitespace</exception>
        public TItemAsset FindItem(string key)
        {
            GFTools.ThrowIfArgNull(key, nameof(key));

            foreach (var item in m_Items)
            {
                if (item.key == key)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the given <paramref name="tag"/> and returns their
        /// count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="tag">The <see cref="TagAsset"/> instance used
        /// as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TInheritedItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the tag filter in this catalog.</returns>
        internal int FindItemsByTagInternal<TInheritedItemAsset>
            (TagAsset tag, ICollection<TItemAsset> target = null)
            where TInheritedItemAsset : TItemAsset
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            var count = 0;

            target?.Clear();

            foreach (var item in m_Items)
            {
                if (item.HasTag(tag))
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
        /// given <paramref name="tag"/> and returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="tag">The <see cref="Tag"/> instance used as
        /// a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/> instances
        /// matching the tag filter in this catalog.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="tag"/> parameter is null</exception>
        public int FindItemsByTag
            (TagAsset tag, ICollection<TItemAsset> target = null)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));
            return FindItemsByTagInternal<TItemAsset>(tag, target);
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItemAsset"/> instances of this catalog matching
        /// the <see cref="TagAsset"/> by its <paramref name="key"/> and
        /// returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="key">The identifier of the <see cref="TagAsset"/>
        /// instance used as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/> instances
        /// matching the tag filter in this catalog.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="key"/> parameter is null, empty, or
        /// whitespace.</exception>
        public int FindItemsByTag
            (string key, ICollection<TItemAsset> target = null)
        {
            var tag = GameFoundationDatabaseSettings.database.tagCatalog.GetTagOrDie(key, nameof(key));
            return FindItemsByTagInternal<TItemAsset>(tag, target);
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the given <paramref name="tag"/> and returns their
        /// count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="tag">The <see cref="TagAsset"/> instance used
        /// as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TInheritedItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the tag filter in this catalog.</returns>
        public int FindItemsByTag<TInheritedItemAsset>
            (TagAsset tag, ICollection<TItemAsset> target = null)
            where TInheritedItemAsset : TItemAsset
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));
            return FindItemsByTagInternal<TInheritedItemAsset>(tag, target);
        }

        /// <summary>
        /// Returns an array of all the
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the given <paramref name="tag"/>
        /// </summary>
        /// <param name="tag">The <see cref="TagAsset"/> instance used
        /// as a filter</param>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the tag filter in this catalog.</returns>
        public TItemAsset[] FindItemsByTag<TInheritedItemAsset>
            (TagAsset tag) where TInheritedItemAsset : TItemAsset
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            try
            {
                FindItemsByTagInternal<TInheritedItemAsset>
                    (tag, s_TempList);

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
        /// matching the <paramref name="tag"/> by its
        /// <paramref name="key"/> and returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="tag">The <see cref="TagAsset"/> instance used
        /// as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TInheritedItemAsset"/> instances.</param>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the tag filter in this catalog.</returns>
        public int FindItemsByTag<TInheritedItemAsset>
            (string key, ICollection<TItemAsset> target = null)
            where TInheritedItemAsset : TItemAsset
        {
            var tag = GameFoundationDatabaseSettings.database.tagCatalog.GetTagOrDie(key, nameof(key));
            return FindItemsByTagInternal<TInheritedItemAsset>
                (tag, target);
        }

        /// <summary>
        /// Returns an array of all the
        /// <typeparamref name="TInheritedItemAsset"/> instances of this catalog
        /// matching the <see cref="TagAsset"/> by its <paramref name="key"/>
        /// </summary>
        /// <param name="key">The <see cref="TagAsset"/> instance used
        /// as a filter</param>
        /// <typeparam name="TInheritedItemAsset"></typeparam>
        /// <returns>The number of <typeparamref name="TInheritedItemAsset"/>
        /// instances matching the tag filter in this catalog.</returns>
        public TItemAsset[] FindItemsByTag<TInheritedItemAsset>
            (string key) where TInheritedItemAsset : TItemAsset
        {
            var tag = GameFoundationDatabaseSettings.database.tagCatalog.GetTagOrDie(key, nameof(key));
            try
            {
                FindItemsByTagInternal<TInheritedItemAsset>
                    (tag, s_TempList);

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
        /// matching the given <paramref name="tag"/>
        /// </summary>
        /// <param name="id">The <see cref="TagAsset"/> instance used
        /// as a filter</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/>
        /// instances matching the tag filter in this catalog.</returns>
        public TItemAsset[] FindItemsByTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            try
            {
                FindItemsByTag(tag, s_TempList);
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
        /// matching the <see cref="TagAsset"/> by its <paramref name="key"/>
        /// </summary>
        /// <param name="key">The <see cref="TagAsset"/> instance used
        /// as a filter</param>
        /// <returns>The number of <typeparamref name="TItemAsset"/>
        /// instances matching the tag filter in this catalog.</returns>
        public TItemAsset[] FindItemsByTag(string key)
        {
            var tag = GameFoundationDatabaseSettings.database.tagCatalog.GetTagOrDie(key, nameof(key));
            return FindItemsByTag(tag);
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
