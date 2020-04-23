using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// This is a class for storing Definitions for a system that the user setup in the editor.
    /// Derived classes will specify each generic to specify which classes are used by their Catalog.
    /// </summary>
    public abstract partial class BaseCatalogAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The list of <see cref="CategoryAsset"/> of this catalog.
        /// </summary>
        [SerializeField]
        internal List<CategoryAsset> m_Categories;

        /// <inheritdoc cref="database"/>
        [SerializeField, HideInInspector]
        internal GameFoundationDatabase m_Database;

        /// <summary>
        /// A reference to the database owning this catalog.
        /// </summary>
        public GameFoundationDatabase database => m_Database;


        /// <summary>
        /// Initializes this <see cref="BaseCatalogAsset"/> instance.
        /// </summary>
        internal void Initialize()
        {
            InitializeCatalog();
        }

        /// <summary>
        /// Initializes the specifics of the inherited type.
        /// </summary>
        protected virtual void InitializeCatalog() {}


        /// <summary>
        /// Initializes the <see cref="BaseCatalogAsset"/> instance.
        /// </summary>
        protected void Awake()
        {
            if (m_Categories is null)
            {
                m_Categories = new List<CategoryAsset>();
            }

            AwakeCatalog();
        }

        /// <summary>
        /// Override this method to initialize the specifics of the inherited
        /// class.
        /// </summary>
        protected virtual void AwakeCatalog() { }

        /// <summary>
        /// Utility methods getting a <see cref="CategoryAsset"/> by its
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Category"/> to
        /// find.</param>
        /// <param name="paramName">The name of the <paramref name="id"/>
        /// parameter from the caller method.
        /// It makes the <see cref="ArgumentException"/> display the name of the
        /// erroneous parameter of the caller instead of the one used in this
        /// utility method.</param>
        /// <returns>Returns the <see cref="CategoryAsset"/> instance</returns>
        protected CategoryAsset GetCategoryOrDie(string id, string paramName)
        {
            GFTools.ThrowIfArgNull(id, paramName);

            var category = FindCategory(id);
            if (category is null)
            {
                throw new CategoryNotFoundException(id);
            }
            return category;
        }

        /// <summary>
        /// Returns specified <see cref="CategoryAsset"/> by its
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="CategoryAsset"/>
        /// to find.</param>
        /// <returns>If found, it returns the requested
        /// <see cref="CategoryAsset"/>, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="id"/>
        /// parameter is null, empty, of whitespace.</exception>
        public CategoryAsset FindCategory(string id)
        {
            GFTools.ThrowIfArgNull(id, nameof(id));

            foreach (var category in m_Categories)
            {
                if (category.id == id)
                {
                    return category;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells whether or not a <see cref="CategoryAsset"/> with the given
        /// <paramref name="id"/> exists in this catalog.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="CategoryAsset"/>
        /// to find.</param>
        /// <returns><c>true</c> if found, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="id"/>
        /// parameter is null, empty or whitespace</exception>
        public bool ContainsCategory(string id) => FindCategory(id) != null;

        /// <summary>
        /// Returns an array of all <see cref="CategoryAsset"/> in this catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetCategories(ICollection{CategoryAsset})"/> instead.
        /// </remarks>
        /// <returns>An array of all <see cref="CategoryAsset"/>.</returns>
        public CategoryAsset[] GetCategories() => GFTools.ToArray(m_Categories);

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all the
        /// <see cref="CategoryAsset"/> of this catalog.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target containerof all the
        /// <see cref="CategoryAsset"/> instances.</param>
        /// <returns>The number of <see cref="CategoryAsset"/> instances of this
        /// catalog.</returns>
        public int GetCategories(ICollection<CategoryAsset> target = null)
            => GFTools.Copy(m_Categories, target);

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the content
        /// of this catalog.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        internal void Configure(CatalogBuilder builder)
        {
            foreach (var categoryAsset in m_Categories)
            {
                categoryAsset.Configure(builder);
            }

            ConfigureCatalog(builder);
        }

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the
        /// specific content of this catalog.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        protected virtual void ConfigureCatalog(CatalogBuilder builder)
        {}

        void ISerializationCallbackReceiver.OnBeforeSerialize()
            => OnBeforeSerializeCatalog();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_Categories != null)
            {
                foreach (var category in m_Categories)
                {
                    if (category is null) continue;
                    category.catalog = this;
                }
            }

            OnAfterDeserializeCatalog();
        }

        protected virtual void OnBeforeSerializeCatalog() { }

        protected virtual void OnAfterDeserializeCatalog() { }
    }
}
