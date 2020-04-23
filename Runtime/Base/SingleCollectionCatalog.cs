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
    public abstract class SingleCollectionCatalog<TItem> : BaseCatalog
        where TItem : CatalogItem
    {
        /// <inheritdoc cref="s_ItemList" />
        [ThreadStatic] static List<TItem> ts_ItemList;

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
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier of the <typeparamref name="TItem"/>
        /// to find.</param>
        /// <returns>The requested <typeparamref name="TItem"/>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="id"/>
        /// parameter is null, empty or whitespace</exception>
        public TItem FindItem(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));

            foreach(var item in m_Items.Values)
            {
                if(item.id.Equals(id))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells whether an item with the specified <paramref name="id"/>
        /// exists or not.
        /// </summary>
        /// <param name="id">The id of the <typeparamref name="TItem"/> instance
        /// to find.</param>
        /// <returns><c>true</c> if the item is found, <c>false</c>
        /// otherwise.</returns>
        public bool ContainsItem(string id) => FindItem(id) != null;

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
        /// given <paramref name="category"/> and returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> instance used as
        /// a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItem"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItem"/> instances
        /// matching the category filter in this catalog.</returns>
        internal int FindItemsByCategoryInternal
            (Category category, ICollection<TItem> target = null)
        {
            var count = 0;

            if (target != null) target.Clear();

            foreach(var item in m_Items.Values)
            {
                if(item.HasCategory(category))
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
        /// given <paramref name="category"/> and returns their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> instance used as
        /// a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItem"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItem"/> instances
        /// matching the category filter in this catalog.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="category"/> parameter is null</exception>
        public int FindItemsByCategory
            (Category category, ICollection<TItem> target = null)
        {
            Tools.ThrowIfArgNull(category, nameof(category));

            return FindItemsByCategoryInternal(category, target);
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all
        /// <typeparamref name="TItem"/> instances of this catalog matching the
        /// <see cref="Category"/> by its <paramref name="id"/> and returns
        /// their count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Category"/> used
        /// as a filter</param>
        /// <param name="target">The target container of all the matching
        /// <typeparamref name="TItem"/> instances.</param>
        /// <returns>The number of <typeparamref name="TItem"/> instances
        /// matching the category filter in this catalog.</returns>
        /// <exception cref="ArgumentException">If the
        /// <paramref name="id"/> parameter is null, empty or
        /// whitespace</exception>
        /// <exception cref="ArgumentException">If there is no
        /// <see cref="Category"/> instance with the given
        /// <paramref name="id"/></exception>
        public int FindItemsByCategory(string id, ICollection<TItem> target = null)
        {
            Tools.ThrowIfArgNull(id, nameof(id));

            var category = FindCategory(id);
            if(category is null)
            {
                throw new ArgumentException($"{id} not found", nameof(id));
            }

            return FindItemsByCategoryInternal(category, target);
        }

        /// <summary>
        /// Returns the array of all the <typeparamref name="TItem"/> instances
        /// of this catalog matching the given <see cref="Category"/>.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> instance used as a
        /// filter</param>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="FindItemsByCategory(Category, ICollection{TItem})"/>
        /// instead.
        /// </remarks>
        /// <returns>The array of all the <typeparamref name="TItem"/> instances
        /// matching the category filter in this catalog.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="category"/> parameter is null.</exception>
        public TItem[] FindItemsByCategory(Category category)
        {
            Tools.ThrowIfArgNull(category, nameof(category));

            try
            {
                FindItemsByCategoryInternal(category, s_ItemList);
                return s_ItemList.ToArray();
            }
            finally
            {
                s_ItemList.Clear();
            }
        }

        /// <summary>
        /// Returns an array of all the <typeparamref name="TItem"/> instances
        /// of this catalog matching the <see cref="Category"/> by its
        /// <paramref name="id"/>.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="FindItemsByCategory(string, ICollection{TItem})"/>
        /// instead.
        /// </remarks>
        /// <param name="id">The identifier of the <see cref="Category"/> used
        /// as a filter</param>
        /// <returns>The array of all the <typeparamref name="TItem"/> instances
        /// matching the category filter in this catalog.</returns>
        /// <exception cref="ArgumentException">If the
        /// <paramref name="id"/> parameter is null, empty or
        /// whitespace</exception>
        /// <exception cref="ArgumentException">If there is no
        /// <see cref="Category"/> instance with the given
        /// <paramref name="id"/></exception>
        public TItem[] FindItemsByCategory(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));

            var category = FindCategory(id);
            if(category is null)
            {
                throw new ArgumentException($"{id} not found", nameof(id));
            }

            try
            {
                FindItemsByCategoryInternal(category, s_ItemList);
                return s_ItemList.ToArray();
            }
            finally
            {
                s_ItemList.Clear();
            }
        }
    }
}
