using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// The base class of the catalog storing only one kind of
    /// <see cref="CatalogItem"/>.
    /// This kind is obviously described by <typeparamref name="TItem"/>.
    /// Most of the catalogs are single collections at the moment.
    /// </summary>
    /// <typeparam name="TItem">The type of <see cref="CatalogItem"/> stored in
    /// this catalog.</typeparam>
    public abstract class SingleCollectionCatalog<TItem>
        where TItem : CatalogItem
    {
        /// <inheritdoc cref="s_ItemList" />
        [ThreadStatic]
        static List<TItem> ts_ItemList;

        /// <summary>
        /// A list used internally to temporarily store <see cref="TItem"/>
        /// instances.
        /// </summary>
        static List<TItem> s_ItemList
        {
            get
            {
                if (ts_ItemList is null) ts_ItemList = new List<TItem>();
                return ts_ItemList;
            }
        }

        /// <summary>
        /// The list of <see cref="TItem"/> this catalog stores.
        /// </summary>
        internal Dictionary<string, TItem> m_Items;

        /// <summary>
        /// Looks for a <typeparamref name="TItem"/> instance by its
        /// <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The identifier of the <typeparamref name="TItem"/>
        /// to find.</param>
        /// <returns>The requested <typeparamref name="TItem"/>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="key"/>
        /// parameter is null, empty or whitespace</exception>
        public TItem FindItem(string key)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            foreach (var item in m_Items.Values)
            {
                if (item.key.Equals(key))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells whether an item with the specified <paramref name="key"/>
        /// exists or not.
        /// </summary>
        /// <param name="key">The id of the <typeparamref name="TItem"/> instance
        /// to find.</param>
        /// <returns><c>true</c> if the item is found, <c>false</c>
        /// otherwise.</returns>
        public bool ContainsItem(string key) => FindItem(key) != null;

        /// <summary>
        /// Returns an array of all the <typeparamref name="TItem"/> isntances
        /// of this catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetItems(ICollection{TItem})"/> instead.
        /// </remarks>
        /// <returns>An array of all item definitions.</returns>
        public TItem[] GetItems() => Tools.ToArray(m_Items.Values);

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItem"/> instances of this catalog and returns
        /// their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <typeparamref name="TItem"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItem"/> instances of
        /// this catalog.</returns>
        public int GetItems(ICollection<TItem> target = null)
            => Tools.Copy(m_Items.Values, target);

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItem"/> instances of this catalog matching the
        /// given <paramref name="tag"/> and returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="tag">The <see cref="Tag"/> instance used as
        /// a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItem"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItem"/> instances
        /// matching the tag filter in this catalog.</returns>
        internal int FindItemsByTagInternal
            (Tag tag, ICollection<TItem> target = null)
        {
            var count = 0;

            if (target != null) target.Clear();

            foreach (var item in m_Items.Values)
            {
                if (item.HasTag(tag))
                {
                    count++;
                    if (target != null) target.Add(item);
                }
            }

            return count;
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItem"/> instances of this catalog matching the
        /// given <paramref name="tag"/> and returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="tag">The <see cref="Tag"/> instance used as
        /// a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItem"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItem"/> instances
        /// matching the tag filter in this catalog.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="tag"/> parameter is null</exception>
        public int FindItemsByTag
            (Tag tag, ICollection<TItem> target = null)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));

            return FindItemsByTagInternal(tag, target);
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItem"/> instances of this catalog matching the
        /// <see cref="Tag"/> by its <paramref name="key"/> and returns
        /// their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="key">The identifier of the <see cref="Tag"/> used
        /// as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItem"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItem"/> instances
        /// matching the tag filter in this catalog.</returns>
        /// <exception cref="ArgumentException">If the
        /// <paramref name="key"/> parameter is null, empty or
        /// whitespace</exception>
        /// <exception cref="ArgumentException">If there is no
        /// <see cref="Tag"/> instance with the given
        /// <paramref name="key"/></exception>
        public int FindItemsByTag(string key, ICollection<TItem> target = null)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            var tag = GameFoundation.catalogs.tagCatalog.GetTagOrDie(key, nameof(key));

            return FindItemsByTagInternal(tag, target);
        }

        /// <summary>
        /// Returns the array of all the <typeparamref name="TItem"/> instances
        /// of this catalog matching the given <see cref="Tag"/>.
        /// </summary>
        /// <param name="tag">The <see cref="Tag"/> instance used as a
        /// filter</param>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="FindItemsByTag(Tag, ICollection{TItem})"/>
        /// instead.
        /// </remarks>
        /// <returns>The array of all the <typeparamref name="TItem"/> instances
        /// matching the tag filter in this catalog.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="tag"/> parameter is null.</exception>
        public TItem[] FindItemsByTag(Tag tag)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));

            try
            {
                FindItemsByTagInternal(tag, s_ItemList);
                return s_ItemList.ToArray();
            }
            finally
            {
                s_ItemList.Clear();
            }
        }

        /// <summary>
        /// Returns an array of all the <typeparamref name="TItem"/> instances
        /// of this catalog matching the <see cref="Tag"/> by its
        /// <paramref name="key"/>.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="FindItemsByTag(string, ICollection{TItem})"/>
        /// instead.
        /// </remarks>
        /// <param name="key">The identifier of the <see cref="Tag"/> used
        /// as a filter</param>
        /// <returns>The array of all the <typeparamref name="TItem"/> instances
        /// matching the tag filter in this catalog.</returns>
        /// <exception cref="ArgumentException">If the
        /// <paramref name="key"/> parameter is null, empty or
        /// whitespace</exception>
        /// <exception cref="ArgumentException">If there is no
        /// <see cref="Tag"/> instance with the given
        /// <paramref name="key"/></exception>
        public TItem[] FindItemsByTag(string key)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            var tag = GameFoundation.catalogs.tagCatalog.GetTagOrDie(key, nameof(key));

            try
            {
                FindItemsByTagInternal(tag, s_ItemList);
                return s_ItemList.ToArray();
            }
            finally
            {
                s_ItemList.Clear();
            }
        }

        /// <summary>
        /// Requests key be removed from every item in the catalog.
        /// </summary>
        /// <param name="key">Tag key id string to remove from every item in the catalog.</param>
        internal virtual void OnRemoveTag(string key)
        {
            Tools.ThrowIfArgNull(key, nameof(key));

            foreach (var kvp in m_Items)
            {
                kvp.Value.OnRemoveTag(key);
            }
        }
    }
}
