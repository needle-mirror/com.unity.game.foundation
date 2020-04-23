using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// <see cref="Category"/> instance are used as filter arguments when
    /// querying <see cref="CatalogItem"/> instances within their catalog.
    /// Each catalog is responsible for containing and managing its own set of
    /// <see cref="Category"/> instance.
    /// </summary>
    public sealed class Category : IEquatable<Category>, IComparable<Category>
    {
        /// <summary>
        /// The identifier of this <see cref="Category"/>.
        /// </summary>
        public string id { get; }

        /// <summary>
        /// The readable name of this <see cref="Category"/> instance.
        /// It is used to make the Editor more comfortable, but it can also be
        /// used at runtime to display a readable information to the players.
        /// </summary>
        public string displayName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        /// </summary>
        /// <param name="id">The identifier of this <see cref="Category"/>.</param>
        /// <param name="displayName">The readable name of this
        /// <see cref="Category"/>.</param>
        internal Category(string id, string displayName)
        {
            this.displayName = displayName;
            this.id = id;
        }

        /// <summary>
        /// == Overload.
        /// If <see cref="id"/> and <see cref="displayName"/> match then the
        /// <see cref="Category"/> instances are deemed equal.
        /// </summary>
        /// <remarks>Two <c>null</c> objects are considered equal.</remarks>
        /// <param name="x">A <see cref="Category"/> instance to compare.</param>
        /// <param name="y">The <see cref="Category"/> instance to compare to.</param>
        /// <returns><c>true</c> if both <see cref="Category"/> are the
        /// same.</returns>
        public static bool operator ==(Category x, Category y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.id == y.id && x.displayName == y.displayName;
        }

        /// <summary>
        /// != Overload.
        /// If <see cref="id"/> or <see cref="displayName"/> don't match then
        /// the <see cref="Category"/> instances. are deemed not equal.
        /// </summary>
        /// <remarks>Two <c>null</c> objects are considered equal.</remarks>
        /// <param name="x">A <see cref="Category"/> instance to compare.</param>
        /// <param name="y">The <see cref="Category"/> instance to compare to.</param>
        /// <returns><c>true</c> if both <see cref="Category"/> are not the
        /// same.</returns>
        public static bool operator !=(Category x, Category y) => !(x == y);

        /// <inheritdoc/>
        public override bool Equals(object obj) => ReferenceEquals(this, obj);

        /// <inheritdoc/>
        public override int GetHashCode() => id.GetHashCode();

        /// <inheritdoc/>
        bool IEquatable<Category>.Equals(Category other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        int IComparable<Category>.CompareTo(Category other)
            => id.CompareTo(other.id);
    }
}
