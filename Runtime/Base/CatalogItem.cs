using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Base class for most of the static data in Game Foundation.
    /// Game Foundation objects are linked to <see cref="Category"/> instances
    /// available in their catalog.
    /// </summary>
    public abstract class CatalogItem : IEquatable<CatalogItem>, IComparable<CatalogItem>
    {
        /// <summary>
        /// The <see cref="Category"/> instances this item is linked to.
        /// Those categories are stored in the same catalog than the one storing
        /// this item.
        /// </summary>
        internal Category[] m_Categories;

        /// <summary>
        /// The details of this item.
        /// </summary>
        internal Dictionary<Type, BaseDetail> m_Details;


        /// <summary>
        /// The readable name of this <see cref="CatalogItem"/> instance.
        /// It is used to make the Editor more comfortable, but it can also be
        /// used at runtime to display a readable information to the players.
        /// </summary>
        public string displayName { get; internal set; }

        /// <summary>
        /// The unique identifier of this <see cref="CatalogItem"/>.
        /// </summary>
        public string id { get; internal set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogItem"/> class.
        /// </summary>
        internal CatalogItem() {}

        /// <summary>
        /// Looks for a <see cref="Category"/>, linked to this
        /// <see cref="CatalogItem"/> instance, by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The <see cref="Category.id"/> of the
        /// <see cref="Category"/> to find.</param>
        /// <returns>If found, returns the <see cref="Category"/> instance,
        /// othewise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/>
        /// cannot be null as a <see cref="Category.id"/> cannot be
        /// null.</exception>
        public Category FindCategory(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));

            foreach (var category in m_Categories)
            {
                if (category.id == id) return category;
            }

            return null;
        }

        /// <summary>
        /// Returns an array of all the <see cref="Category"/> instances
        /// linked to this <see cref="CatalogItem"/> instance.
        /// catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetCategories(ICollection{Category})"/> instead.
        /// </remarks>
        /// <returns>An array of all the categories linked to this
        /// <see cref="CatalogItem"/> instance.</returns>
        public Category[] GetCategories() => Tools.ToArray(m_Categories);

        /// <summary>
        /// Fills the given <paramref name="target"/> collection with all the
        /// <see cref="Category"/> instances linked to this
        /// <see cref="CatalogItem"/> instance.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <see cref="Category"/> instances.</param>
        /// <returns>The number of <see cref="Category"/> instances linked to
        /// this <see cref="CatalogItem"/> instance.</returns>
        public int GetCategories(ICollection<Category> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));
            return Tools.Copy(m_Categories, target);
        }

        /// <summary>
        /// Tells whether or not the given <paramref name="category"/> is within
        /// this <see cref="CatalogItem"/> instance.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> instance. to
        /// search for.</param>
        /// <returns>Whether or not this <see cref="CatalogItem"/> instance has
        /// the specified <see cref="Category"/> included.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="category"/> is <c>null</c>.</exception>
        public bool HasCategory(Category category)
        {
            Tools.ThrowIfArgNull(category, nameof(category));
            return Array.IndexOf(m_Categories, category) >= 0;
        }

        /// <summary>
        /// Tells whether or not a <see cref="Category"/> instance with the
        /// given <paramref name="id"/> is within this <see cref="CatalogItem"/>
        /// instance.
        /// </summary>
        /// <param name="id">The identifier of a <see cref="Category"/>
        /// instance.</param>
        /// <returns>Whether or not this <see cref="CatalogItem"/> instance has
        /// a <see cref="Category"/> with the given <paramref name="id"/>
        /// included.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="id"/> is <c>null</c>.</exception>
        public bool HasCategory(string id) => FindCategory(id) != null;

        /// <summary>
        /// Returns an array of all the <see cref="BaseDetail"/> instances of
        /// this <see cref="CatalogItem"/> instance.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetDetails(ICollection{BaseDetail})"/> instead.
        /// </remarks>
        /// <returns>An array of all the details.</returns>
        public BaseDetail[] GetDetails() => Tools.ToArray(m_Details.Values);

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all the
        /// <see cref="BaseDetail"/> instance of this <see cref="CatalogItem"/>
        /// instance.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <see cref="BaseDetail"/> instances.</param>
        /// <return>The number of <see cref="BaseDetail"/> of this
        /// <see cref="CatalogItem"/> instance.</return>
        public int GetDetails(ICollection<BaseDetail> target = null)
            => Tools.Copy(m_Details.Values, target);

        /// <summary> 
        /// Gets the <typeparamref name="TDetail"/> instance by its type.
        /// </summary>
        /// <typeparam name="TDetail">The type of detail requested.</typeparam>
        /// <returns>The <typeparamref name="TDetail"/> instance stored in this
        /// <see cref="CatalogItem"/> instance.</returns>
        public TDetail GetDetail<TDetail>() where TDetail : BaseDetail
        {
            m_Details.TryGetValue(typeof(TDetail), out var definition);
            return definition as TDetail;
        }

        /// <inheritdoc />
        public override int GetHashCode() => id.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        bool IEquatable<CatalogItem>.Equals(CatalogItem other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        int IComparable<CatalogItem>.CompareTo(CatalogItem other)
            => id.CompareTo(other.id);
    }
}
