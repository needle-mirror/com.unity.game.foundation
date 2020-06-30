#if UNITY_EDITOR

using System;
using UnityEditor;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class TagAsset
    {
        /// <summary>
        /// Returns the prefix used to give a name to the asset.
        /// </summary>
        internal string Editor_AssetPrefix => "Tag";

        /// <summary>
        /// Returns the name to assign to the asset.
        /// </summary>
        internal string Editor_AssetName => $"{Editor_AssetPrefix}_{key}";

        /// <summary>
        /// Removes the deleted currency from the catalog.
        /// </summary>
        void OnItemDestroy()
        {
            if (catalog is null) return;
            catalog.Editor_RemoveTag(this);
        }

        /// <summary>
        /// Creates a TagAsset.
        /// </summary>
        /// <param name="key">The identifier of the
        /// <see cref="TagAsset"/>.</param>
        /// <returns>The newly created <see cref="TagAsset"/></returns>
        /// <exception cref="ArgumentException">Thrown if an empty Id is
        /// given.</exception>
        internal static TagAsset Editor_Create(string key)
        {
            GFTools.ThrowIfArgNullOrEmpty(key, nameof(key));

            if (!Tools.IsValidKey(key))
            {
                throw new ArgumentException
                    ($"{nameof(TagAsset)} {nameof(TagAsset.key)} can only be alphanumeric with optional dashes or underscores.");
            }

            var tag = CreateInstance<TagAsset>();
            tag.m_Key = key;
            tag.name = tag.Editor_AssetName;
            tag.m_Catalog = GameFoundationDatabaseSettings.database.tagCatalog;
            return tag;
        }

        /// <summary>
        /// Initializes the id and object name.
        /// </summary>
        /// <param name="id">
        /// The id of the definition.
        /// </param>
        /// <param name="name">
        /// The name of the asset.
        /// </param>
        internal void Editor_Initialize(TagCatalogAsset catalog, string id)
        {
            GFTools.ThrowIfArgNull(catalog, nameof(catalog));
            GFTools.ThrowIfArgNullOrEmpty(id, nameof(id));

            m_Catalog = catalog;
            Editor_SetId(id);
            name = Editor_AssetName;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Sets the id of <paramref name="this"/>.
        /// </summary>
        /// <param name="id">
        /// The identifier to assign to the definition.
        /// </param>
        internal void Editor_SetId(string id)
        {
            GFTools.ThrowIfArgNullOrEmpty(id, nameof(id));

            if (!GFTools.IsValidId(id))
            {
                throw new ArgumentException
                    ("GameItemDefinition can only be alphanumeric with optional dashes or underscores.");
            }

            m_Key = id;

            EditorUtility.SetDirty(this);
        }

        public static bool operator==(TagAsset a, TagAsset b)
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

        public static bool operator!=(TagAsset a, TagAsset b)
        {
            return !(a == b);
        }

        public override bool Equals(object other)
        {
            if (other is TagAsset tag)
            {
                return this == tag;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        void OnDestroy()
        {
            if (catalog is null) return;
            catalog.Editor_RemoveTag(this);

            OnItemDestroy();
        }
    }
}

#endif
