#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    internal delegate void Editor_TagRemovedEventHandler(TagCatalogAsset catalog, TagAsset item);

    public partial class TagCatalogAsset
    {
        internal event Editor_TagRemovedEventHandler editor_TagRemoved;

        internal void Editor_DispatchItemRemoved(TagAsset item) => editor_TagRemoved?.Invoke(this, item);

        /// <summary>
        /// Adds the given <paramref name="tag"/> to this Catalog.
        /// </summary>
        /// <param name="tag">
        /// The <see cref="TagAsset"/> instance to add.
        /// </param>
        /// <returns>
        /// Whether or not the <paramref name="tag"/> was added successfully.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if a duplicate entry is given.
        /// </exception>
        internal bool Editor_AddTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            if (FindTag(tag.key) != null)
            {
                throw new ArgumentException
                    ($"The object is already registered within this Catalog. (id: {tag.key}");
            }

            m_Tags.Add(tag);
            tag.m_Catalog = this;

            EditorUtility.SetDirty(this);
            return true;
        }

        /// <summary>
        /// Removes the given <paramref name="tag"/> from this catalog.
        /// </summary>
        /// <param name="tag">
        /// The <see cref="TagAsset"/> to remove.
        /// </param>
        /// <returns>
        /// Whether or not the <paramref name="tag"/> was successfully removed.
        /// </returns>
        internal bool Editor_RemoveTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            var removed = m_Tags.Remove(tag);
            if (removed)
            {
                database.Editor_OnTagRemoved(tag);
                Editor_DispatchItemRemoved(tag);
                EditorUtility.SetDirty(this);
            }
            return removed;
        }

        /// <summary>
        /// Gets all the subassets of this catalog.
        /// </summary>
        /// <param name="target">
        /// The target collection to where subassets are added.
        /// </param>
        internal void Editor_GetSubAssets(ICollection<Object> target)
        {
            if (m_Tags == null) return;

            foreach (var item in m_Tags)
            {
                if (item is null)
                {
                    continue;
                }
                target.Add(item);
            }
        }
    }
}
#endif
