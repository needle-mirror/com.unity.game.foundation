using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Maintains Collection of <see cref="BaseTransaction"/> items for a Store and implements methods to retrieve them.
    /// </summary>
    /// <inheritdoc/>
    public class Store : CatalogItem
    {
        [ThreadStatic] static List<BaseTransaction> ts_TempStoreItemList;
        static List<BaseTransaction> s_TempStoreItemList
        {
            get
            {
                if (ts_TempStoreItemList is null) ts_TempStoreItemList = new List<BaseTransaction>();
                return ts_TempStoreItemList;
            }
        }

        [ThreadStatic] static List<Category> ts_TempCategoryList;
        static List<Category> s_TempCategoryList
        {
            get
            {
                if (ts_TempCategoryList is null) ts_TempCategoryList = new List<Category>();
                return ts_TempCategoryList;
            }
        }

        /// <summary>
        /// Available <see cref="Store"/> <see cref="BaseTransaction"/> items for this <see cref="Store"/>.
        /// </summary>
        internal BaseTransaction[] m_Items;

        /// <summary>
        /// Returns an array of the <see cref="BaseTransaction"/> items in this <see cref="Store"/>.
        /// </summary>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this <see cref="Store"/>.</returns>
        public BaseTransaction[] GetStoreItems() => Tools.ToArray(m_Items);

        /// <summary>
        /// Adds all <see cref="BaseTransaction"/> items with any of specified <see cref="Category"/> set to target Collection.
        /// </summary>
        /// <param name="category">Desired <see cref="Category"/> to be queried by this method.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>Number of <see cref="BaseTransaction"/> items added to the target collection.</returns>
        /// <exception cref="ArgumentNullException">Throws if category is null.</exception>
        /// <exception cref="NullReferenceException">Throws if target is null.</exception>
        int GetStoreItemsByCategoryInternal(Category category, ICollection<BaseTransaction> target)
        {
            target.Clear();
            foreach (var storeItem in m_Items)
            {
                if (storeItem.HasCategory(category))
                {
                    target.Add(storeItem);
                }
            }
            return target.Count;
        }

        /// <summary>
        /// Returns an array of all <see cref="BaseTransaction"/> items associated with the specified <see cref="Category"/> in this <see cref="Store"/>.
        /// </summary>
        /// <param name="category">Desired <see cref="Category"/> to be queried by this method.</param>
        /// <returns>Array of all <see cref="BaseTransaction"/> items associated with the specified <see cref="Category"/> in this <see cref="Store"</returns>
        /// <exception cref="ArgumentNullException">Throws if <see cref="Category"/> is null.</exception>
        BaseTransaction[] GetStoreItemsByCategoryInternal(Category category)
        {
            try
            {
                GetStoreItemsByCategoryInternal(category, s_TempStoreItemList);
                return s_TempStoreItemList.ToArray();
            }
            finally
            {
                s_TempStoreItemList.Clear();
            }
        }

        /// <summary>
        /// Adds all <see cref="BaseTransaction"/> items with any of specified <see cref="Category"/> set to target Collection.
        /// </summary>
        /// <param name="categories">Collection of <see cref="Category"/> to accept.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>Number of <see cref="BaseTransaction"/> items added to the target collection.</returns>
        /// <exception cref="NullReferenceException">Throws if categories or target is null.</exception>
        /// <exception cref="ArgumentNullException">Throws if any <see cref="Category"/> in categories Collection is null.</exception>
        int GetStoreItemsByCategoryInternal(ICollection<Category> categories, ICollection<BaseTransaction> target)
        {
            target.Clear();
            foreach (var category in categories)
            {
                foreach (var storeItem in m_Items)
                {
                    if (storeItem.HasCategory(category))
                    {
                        target.Add(storeItem);
                        break;
                    }
                }
            }
            return target.Count;
        }

        /// <summary>
        /// Get an array of all <see cref="BaseTransaction"/> items with any of specified <see cref="Category"/> set.
        /// </summary>
        /// <param name="categories">Collection of <see cref="Category"/> to accept.</param>
        /// <returns>Array of all <see cref="BaseTransaction"/> items with any of specified <see cref="Category"/> set.</returns>
        /// <exception cref="NullReferenceException">Throws if categories is null.</exception>
        /// <exception cref="ArgumentNullException">Throws if any <see cref="Category"/> in categories Collection is null.</exception>
        BaseTransaction[] GetStoreItemsByCategoryInternal(ICollection<Category> categories)
        {
            try
            {
                GetStoreItemsByCategoryInternal(categories, s_TempStoreItemList);
                return s_TempStoreItemList.ToArray();
            }
            finally
            {
                s_TempStoreItemList.Clear();
            }
        }

        /// <summary>
        /// Adds all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with specified <see cref="Category"/> set to Collection.
        /// </summary>
        /// <param name="category">Desired <see cref="Category"/> to be queried by this method.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>The number of <see cref="BaseTransaction"/> items added.</returns>
        /// <exception cref="ArgumentNullException">Throws if category or target is null.</exception>
        public int GetStoreItemsByCategory(Category category, ICollection<BaseTransaction> target)
        {
            Tools.ThrowIfArgNull(category, nameof(category));
            Tools.ThrowIfArgNull(target, nameof(target));

            return GetStoreItemsByCategoryInternal(category, target);
        }

        /// <summary>
        /// Returns an array of the <see cref="BaseTransaction"/> items in this <see cref="Store"/> with specified <see cref="Category"/> set.
        /// </summary>
        /// <param name="category">Desired <see cref="Category"/> to be queried by this method.</param>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this <see cref="Store"/>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <see cref="Category"/> is null.</exception>
        public BaseTransaction[] GetStoreItemsByCategory(Category category)
        {
            Tools.ThrowIfArgNull(category, nameof(category));
            return GetStoreItemsByCategoryInternal(category);
        }

        /// <summary>
        /// Returns an array of the <see cref="BaseTransaction"/> items in this <see cref="Store"/> with specified <see cref="Category"/> set.
        /// </summary>
        /// <param name="id">Desired <see cref="Category"/> id string to be added to the output array.</param>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this <see cref="Store"/>.</returns>
        /// <exception cref="ArgumentException">Throws if the catalog id is null or empty or not found in this <see cref="Store"/>.</exception>
        public BaseTransaction[] GetStoreItemsByCategory(string id)
        {
            Tools.ThrowIfArgNullOrEmpty(id, nameof(id));

            var catalog = GameFoundation.catalogs.transactionCatalog;
            var category = catalog.GetCategoryOrDie(id, nameof(id));

            return GetStoreItemsByCategoryInternal(category);
        }

        /// <summary>
        /// Updates Collection of all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of the specified <see cref="Category"/> set.
        /// </summary>
        /// <param name="categories">Collection of desired <see cref="Category"/> to be added to the output Collection.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>The number of <see cref="BaseTransaction"/> items added.</returns>
        /// <exception cref="ArgumentNullException">Throws if categories or target is null.</exception>
        /// <exception cref="ArgumentException">Throws if any <see cref="Category"/> in categories Collection is null.</exception>
        public int GetStoreItemsByCategory(ICollection<Category> categories, ICollection<BaseTransaction> target)
        {
            Tools.ThrowIfArgNull(categories, nameof(categories));
            Tools.ThrowIfArgNull(target, nameof(target));

            foreach (var category in categories)
            {
                if (category is null)
                {
                    throw new ArgumentException($"{nameof(categories)} cannot contain null values");
                }
            }

            return GetStoreItemsByCategoryInternal(categories, target);
        }

        /// <summary>
        /// Returns an array of all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of the specified <see cref="Category"/> set.
        /// </summary>
        /// <param name="categoryies">Collection of desired <see cref="Category"/> to be added to the output array.</param>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this <see cref="Store"/> matching caregories request.</returns>
        /// <exception cref="ArgumentNullException">Throws if categories is null.</exception>
        /// <exception cref="ArgumentException">Throws if any <see cref="Category"/> in categories Collection is null.</exception>
        public BaseTransaction[] GetStoreItemsByCategory(ICollection<Category> categories)
        {
            Tools.ThrowIfArgNull(categories, nameof(categories));

            foreach (var category in categories)
            {
                if (category is null)
                {
                    throw new ArgumentException($"{nameof(categories)} cannot contain null values");
                }
            }

            return GetStoreItemsByCategoryInternal(categories);
        }

        /// <summary>
        /// Adds all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of the specified <see cref="Category"/> set to Collection.
        /// </summary>
        /// <param name="categoryIds">Collection of desired <see cref="Category"/> id strings to be added to the target Collection.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>The number of <see cref="BaseTransaction"/> items added.</returns>
        /// <exception cref="ArgumentNullException">Throws if either categoryIds or target parameter is null.</exception>
        /// <exception cref="ArgumentException">Throws if any of the catalog ids are null or empty or not found in this <see cref="Store"/>.</exception>
        public int GetStoreItemsByCategory(ICollection<string> categoryIds, ICollection<BaseTransaction> target)
        {
            Tools.ThrowIfArgNull(categoryIds, nameof(categoryIds));
            Tools.ThrowIfArgNull(target, nameof(target));

            var catalog = GameFoundation.catalogs.transactionCatalog;

            var categories = s_TempCategoryList;
            try
            {
                catalog.GetCategoriesOrDie(categoryIds, categories, nameof(categoryIds));

                return GetStoreItemsByCategoryInternal(categories, target);
            }
            finally
            {
                categories.Clear();
            }
        }

        /// <summary>
        /// Returns an array of all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of the specified <see cref="Category"/> set.
        /// </summary>
        /// <param name="categoryIds">Collection of desired <see cref="Category"/> id strings to be added to the output array.</param>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this Store.</returns>
        /// <exception cref="ArgumentNullException">Throws if <see cref="Category"/> ids Collection is null.</exception>
        /// <exception cref="ArgumentException">Throws if any of the catalog ids are null or empty or not found in this <see cref="Store"/>.</exception>
        public BaseTransaction[] GetStoreItemsByCategory(ICollection<string> categoryIds)
        {
            Tools.ThrowIfArgNull(categoryIds, nameof(categoryIds));

            var catalog = GameFoundation.catalogs.transactionCatalog;

            var categories = s_TempCategoryList;
            try
            {
                catalog.GetCategoriesOrDie(categoryIds, categories, nameof(categoryIds));

                return GetStoreItemsByCategoryInternal(categories);
            }
            finally
            {
                categories.Clear();
            }
        }

        /// <summary>
        /// Returns a summary string for this <see cref="Store"/>.
        /// </summary>
        /// <returns>Summary string for this <see cref="Store"/>.</returns>
        public override string ToString()
        {
            return $"{GetType().Name}(Id: '{id}' DisplayName: '{displayName}')";
        }
    }
}
