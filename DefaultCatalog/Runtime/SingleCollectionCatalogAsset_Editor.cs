#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class SingleCollectionCatalogAsset<TItemAsset>
    {
        internal event Action<SingleCollectionCatalogAsset<TItemAsset>, TItemAsset> editor_ItemRemoved;

        internal void Editor_DispatchItemRemoved(TItemAsset item)
            => editor_ItemRemoved?.Invoke(this, item);

        /// <summary>
        /// Adds the <paramref name="item"/> to this catalog.
        /// </summary>
        /// <param name="item">The <see cref="TItemAsset"/> to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate definition
        /// is given.</exception>
        internal bool Editor_AddItem(TItemAsset item)
        {
            GFTools.ThrowIfArgNull(item, nameof(item));

            if (FindItem(item.key) != null)
            {
                throw new ArgumentException
                    ($"The object is already registered within this Catalog. (key: {item.key})");
            }

            m_Items.Add(item);

            EditorUtility.SetDirty(this);
            return true;
        }

        /// <summary>
        /// Removes the <paramref name="item"/> from this catalog.
        /// </summary>
        /// <param name="item">The <see cref="TItemAsset"/> to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        internal bool Editor_RemoveItem(TItemAsset item)
        {
            GFTools.ThrowIfArgNull(item, nameof(item));

            if (!m_Items.Contains(item))
            {
                return false;
            }

            var removed = m_Items.Remove(item);

            if (removed)
            {
                EditorUtility.SetDirty(this);
                Editor_DispatchItemRemoved(item);
            }

            return removed;
        }

        /// <inheritdoc/>
        protected sealed override void Editor_GetCatalogSubAssets
            (ICollection<Object> target)
        {
            if (m_Items != null)
            {
                foreach (var item in m_Items)
                {
                    if (item is null)
                        continue;
                    target.Add(item);
                    item.Editor_GetSubAssets(target);
                }
            }

            Editor_GetSingleCollectionCatalogAssets(target);
        }

        /// <summary>
        /// Gets all the subassets of this
        /// <see cref="SingleCollectionCatalogAsset{TItem}"/>
        /// </summary>
        /// <param name="target">The target collection to where subassets are
        /// added.</param>
        protected virtual void Editor_GetSingleCollectionCatalogAssets
            (ICollection<Object> target)
        { }


        /// <summary>
        /// Can be overridden if removing a <paramref name="tag"/> has a
        /// consequence for the inherited catalog.
        /// </summary>
        /// <param name="tag">The <see cref="TagAsset"/>
        /// removed.</param>
        internal virtual void Editor_OnTagRemoved(TagAsset tag)
        {
            foreach (var item in m_Items)
            {
                item.Editor_RemoveTag(tag); 
            }
        }
    }
}

#endif
