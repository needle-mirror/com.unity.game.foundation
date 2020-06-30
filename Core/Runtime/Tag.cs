using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// <see cref="Tag"/> instance are used as filter arguments when
    /// querying <see cref="CatalogItem"/> instances within a catalog.
    /// </summary>
    public sealed class Tag : IEquatable<Tag>, IComparable<Tag>
    {
        /// <summary>
        /// The identifier of this <see cref="Tag"/>.
        /// </summary>
        public string key { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="key">The identifier of this <see cref="Tag"/>.</param>
        /// <param name="displayName">The readable name of this
        /// <see cref="Tag"/>.</param>
        internal Tag(string key)
        {
            this.key = key;
        }

        /// <summary>
        /// == Overload.
        /// If <see cref="key"/> and <see cref="displayName"/> match then the
        /// <see cref="Tag"/> instances are deemed equal.
        /// </summary>
        /// <remarks>Two <c>null</c> objects are considered equal.</remarks>
        /// <param name="a">A <see cref="Tag"/> instance to compare.</param>
        /// <param name="b">The <see cref="Tag"/> instance to compare to.</param>
        /// <returns><c>true</c> if both <see cref="Tag"/> are the
        /// same.</returns>
        public static bool operator ==(Tag a, Tag b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            if (a is null || b is null)
            {
                return false;
            }
            return a.key == b.key;
        }

        /// <summary>
        /// != Overload.
        /// If <see cref="key"/> or <see cref="displayName"/> don't match then
        /// the <see cref="Tag"/> instances. are deemed not equal.
        /// </summary>
        /// <remarks>Two <c>null</c> objects are considered equal.</remarks>
        /// <param name="x">A <see cref="Tag"/> instance to compare.</param>
        /// <param name="y">The <see cref="Tag"/> instance to compare to.</param>
        /// <returns><c>true</c> if both <see cref="Tag"/> are not the
        /// same.</returns>
        public static bool operator !=(Tag x, Tag y) => !(x == y);

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is Tag tag)
            {
                return key == tag.key;
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => key.GetHashCode();

        /// <inheritdoc/>
        bool IEquatable<Tag>.Equals(Tag other)
        {
            if (other is null)
            {
                return this is null;
;
            }
            return key == other.key;
        }

        /// <inheritdoc />
        int IComparable<Tag>.CompareTo(Tag other)
            => key.CompareTo(other.key);
    }
}
