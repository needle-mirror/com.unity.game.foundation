using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Stores and provides <see cref="TagAsset"/>.
    /// </summary>
    public sealed partial class TagCatalogAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The list of <see cref="TagAsset"/> of this catalog.
        /// </summary>
        [SerializeField]
        internal List<TagAsset> m_Tags;

        /// <inheritdoc cref="database"/>
        [SerializeField, HideInInspector]
        internal GameFoundationDatabase m_Database;

        /// <summary>
        /// A reference to the database owning this catalog.
        /// </summary>
        public GameFoundationDatabase database => m_Database;

        /// <summary>
        /// Initializes the <see cref="BaseCatalogAsset"/> instance.
        /// </summary>
        void Awake()
        {
            if (m_Tags is null)
            { 
                m_Tags = new List<TagAsset>();
            }

            AwakeCatalog();
        }

        /// <summary>
        /// Utility methods getting a <see cref="TagAsset"/> by its
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Tag"/> to
        /// find.</param>
        /// <param name="paramName">The name of the <paramref name="id"/>
        /// parameter from the caller method.
        /// It makes the <see cref="ArgumentException"/> display the name of the
        /// erroneous parameter of the caller instead of the one used in this
        /// utility method.</param>
        /// <returns>Returns the <see cref="TagAsset"/> instance</returns>
        internal TagAsset GetTagOrDie(string id, string paramName)
        {
            GFTools.ThrowIfArgNull(id, paramName);

            var tag = FindTag(id);
            if (tag is null)
            {
                throw new TagNotFoundException(id);
            }
            return tag;
        }

        /// <summary>
        /// Returns specified <see cref="TagAsset"/> by its
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="TagAsset"/>
        /// to find.</param>
        /// <returns>If found, it returns the requested
        /// <see cref="TagAsset"/>, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="id"/>
        /// parameter is null, empty, of whitespace.</exception>
        public TagAsset FindTag(string id)
        {
            GFTools.ThrowIfArgNull(id, nameof(id));

            foreach (var tag in m_Tags)
            {
                if (tag.key == id)
                {
                    return tag;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells whether or not a <see cref="TagAsset"/> with the given
        /// <paramref name="id"/> exists in this catalog.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="TagAsset"/>
        /// to find.</param>
        /// <returns><c>true</c> if found, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="id"/>
        /// parameter is null, empty or whitespace</exception>
        public bool ContainsTag(string id) => FindTag(id) != null;

        /// <summary>
        /// Returns an array of all <see cref="TagAsset"/> in this catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetTags(ICollection{TagAsset})"/> instead.
        /// </remarks>
        /// <returns>An array of all <see cref="TagAsset"/>.</returns>
        public TagAsset[] GetTags() => GFTools.ToArray(m_Tags);

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all the
        /// <see cref="TagAsset"/> of this catalog.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target containerof all the
        /// <see cref="TagAsset"/> instances.</param>
        /// <returns>The number of <see cref="TagAsset"/> instances of this
        /// catalog.</returns>
        public int GetTags(ICollection<TagAsset> target = null) => GFTools.Copy(m_Tags, target);

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the content
        /// of this catalog.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        internal void Configure(CatalogBuilder builder)
        {
            ConfigureCatalog(builder);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() => OnBeforeSerializeCatalog();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_Tags != null)
            {
                foreach (var tag in m_Tags)
                {
                    if (tag is null)
                    {
                        continue;
                    }
                    tag.m_Catalog = this;
                }
            }

            OnAfterDeserializeCatalog();
        }

        void OnBeforeSerializeCatalog() { }

        void OnAfterDeserializeCatalog() { }

        /// <summary>
        /// Override this method to initialize the specifics of the inherited
        /// class.
        /// </summary>
        void AwakeCatalog()
        {
            if (m_Tags is null)
            {
                m_Tags = new List<TagAsset>();
            }
        }

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the
        /// specific content of this catalog.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        void ConfigureCatalog(CatalogBuilder builder)
        {
            foreach (var tagAsset in m_Tags)
            {
                tagAsset.Configure(builder);
            }
        }
    }
}
