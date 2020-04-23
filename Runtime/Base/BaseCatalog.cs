using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This is a class for storing elements for a system.
    /// Each catalog contains its own list of categories.
    /// </summary>
    public abstract class BaseCatalog
    {
        /// <summary>
        /// The list of <see cref="Category"/> we use within this catalog.
        /// </summary>
        internal Category[] m_Categories;


        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCatalog"/> class.
        /// </summary>
        protected BaseCatalog()
        {}

        /// <summary>
        /// Gets a category from its <paramref name="id"/> or throw an
        /// <see cref="ArgumentException"/> if not found.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Category"/> to
        /// find.</param>
        /// <param name="paramName">The name of the <paramref name="id"/>
        /// variable in the caller method.
        /// Used to build a better exception.</param>
        /// <returns>The <see cref="Category"/> instance with the specified
        /// <paramref name="id"/>.</returns>
        internal Category GetCategoryOrDie(string id, string paramName)
        {
            Tools.ThrowIfArgNullOrEmpty(id, paramName);

            foreach (var category in m_Categories)
            {
                if (category.id == id)
                {
                    return category;
                }
            }

            throw new ArgumentException
                ($"{nameof(Category)} {id} not found", paramName);
        }

        /// <summary>
        /// Gets a collection of categories from their <paramref name="ids"/> or
        /// throw an <see cref="ArgumentException"/> if not found.
        /// </summary>
        /// <param name="ids">The identifiers of the <see cref="Category"/>
        /// instances to find.</param>
        /// <param name="paramName">The name of the <paramref name="ids"/>
        /// variable in the caller method.
        /// Used to build a better exception.</param>
        /// <param name="target">The target collection for the categories</param>
        internal void GetCategoriesOrDie(
            ICollection<string> ids,
            ICollection<Category> target,
            string paramName)
        {
            Tools.ThrowIfArgNull(ids, paramName);

            foreach (var id in ids)
            {
                var category = GetCategoryOrDie(id, paramName);
                target.Add(category);
            }
        }

        /// <summary>
        /// Looks for a <see cref="Category"/> by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The <see cref="Category.id"/> of the
        /// <see cref="Category"/> instance to find.</param>
        /// <returns>The requested <see cref="Category"/> instance, or
        /// <pre>null</pre> if not found.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="id"/>
        /// parameter is <c>null</c>.</exception>
        public Category FindCategory(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));

            foreach(var category in m_Categories)
            {
                if(category.id == id)
                {
                    return category;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells whether or not this catalog contains a <see cref="Category"/>
        /// instance with the specified <paramref name="id"/> as
        /// <see cref="Category.id"/>.
        /// </summary>
        /// <param name="id">The <see cref="Category.id"/> to find.</param>
        /// <returns><c>true</c> if a <see cref="Category"/> instance exists in
        /// this cataglog with the specified <paramref name="id"/>, <c>false</c>
        /// otherwise</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="id"/>
        /// parameter is <c>null</c>.</exception>
        public bool ContainsCategory(string id) => FindCategory(id) != null;

        /// <summary>
        /// Tells whether or not this catalog contains the specified
        /// <see cref="Category"/> instance.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> to find.</param>
        /// <returns><c>true</c> if a <see cref="Category"/> instance exists in
        /// this cataglog with the specified <paramref name="id"/>, <c>false</c>
        /// otherwise</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="category"/> parameter is <c>null</c>.</exception>
        public bool ContainsCategory(Category category)
        {
            Tools.ThrowIfArgNull(category, nameof(category));
            return Array.IndexOf(m_Categories, category) >= 0;
        }

        /// <summary>
        /// Returns an array of all the <see cref="Category"/> instances of this
        /// catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetCategories(ICollection{Category})"/> instead.
        /// </remarks>
        /// <returns>An array of all the categories.</returns>
        public Category[] GetCategories() => Tools.ToArray(m_Categories);

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all the
        /// <see cref="Category"/> instances of this catalog, and returns their
        /// count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <see cref="Category"/> instances.</param>
        /// <returns>The number of <see cref="Category"/> instances of this
        /// catalog.</returns>
        public int GetCategories(ICollection<Category> target = null)
            => Tools.Copy(m_Categories, target);
    }
}
