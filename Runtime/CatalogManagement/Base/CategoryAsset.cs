using System;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// <see cref="CategoryAsset"/> instance are used as filter arguments when
    /// querying <see cref="CatalogItemAsset"/> instances within their catalog.
    /// Each catalog is responsible for containing and managing its own set of
    /// <see cref="CategoryAsset"/> instance.
    /// </summary>
    public sealed partial class CategoryAsset : ScriptableObject
    {
        /// <inheritdoc cref="id"/>
        [SerializeField]
        internal string m_Id;

        /// <inheritdoc cref="displayName"/>
        [SerializeField]
        internal string m_DisplayName;

        /// <summary>
        /// Reference to the catalog containing this
        /// <see cref="CategoryAsset"/> instance.
        /// </summary>
        [field: NonSerialized]
        public BaseCatalogAsset catalog { get; internal set; }

        /// <summary>
        /// The identifier of this <see cref="CategoryAsset"/>.
        /// </summary>
        public string id => m_Id;

        /// <summary>
        /// The readable name of this <see cref="CategoryAsset"/> instance.
        /// It is used to make the Editor more comfortable, but it can also be
        /// used at runtime to display a readable information to the players.
        /// </summary>
        public string displayName => m_DisplayName;

        /// <summary>
        /// == Overload. If <see cref="id"/> and <see cref="displayName"/> match
        /// then the <see cref="CategoryAsset"/> are deemed equal.
        /// Note: Two null objects are considered equal.
        /// </summary>
        /// <param name="x">A <see cref="CategoryAsset"/> to compare.</param>
        /// <param name="y">A <see cref="CategoryAsset"/> to compare to.</param>
        /// <returns><c>true</c> if both <see cref="CategoryAsset"/> are the
        /// same, otherwise <c>false</c>.</returns>
        public static bool operator ==(CategoryAsset x, CategoryAsset y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.id == y.id && x.displayName == y.displayName;
        }

        /// <summary>
        /// != Overload. If <see cref="id"/> and <see cref="displayName"/> don't
        /// match the <see cref="CategoryAsset"/> are deemed not equal.
        /// Note: Two null objects are considered equal.
        /// </summary>
        /// <param name="x">A <see cref="CategoryAsset"/> to compare.</param>
        /// <param name="y">A <see cref="CategoryAsset"/> to compare to.</param>
        /// <returns><c>false</c> if both <see cref="CategoryAsset"/> are the
        /// same, otherwise <c>true</c>.</returns>
        public static bool operator !=(CategoryAsset x, CategoryAsset y)
        {
            return !(x == y);
        }

        /// <summary>
        /// If <see cref="id"/> and <see cref="displayName"/> match then the
        /// <see cref="CategoryAsset"/> are deemed equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><c>true</c> if both <see cref="CategoryAsset"/> are the
        /// same, otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var category = obj as CategoryAsset;
            return category != null && this == category;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => id.GetHashCode();

        /// <summary>
        /// Configures a category.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        internal void Configure(CatalogBuilder builder)
        {
            var config = builder.CreateCategory(id);
            config.displayName = displayName;
        }
    }
}
