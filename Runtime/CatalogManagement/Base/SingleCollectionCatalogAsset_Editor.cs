#if UNITY_EDITOR

using UnityEditor;
using System.Collections.Generic;
using System;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    internal delegate void Editor_ItemRemovedEventHandler<TItemAsset>
        (SingleCollectionCatalogAsset<TItemAsset> catalog, TItemAsset item)
        where TItemAsset : CatalogItemAsset;

    public partial class SingleCollectionCatalogAsset<TItemAsset>
    {
        internal event Editor_ItemRemovedEventHandler<TItemAsset> editor_ItemRemoved;

        internal void Editor_DispatchItemRemoved(TItemAsset item)
            => editor_ItemRemoved?.Invoke(this, item);

        void Editor_InitializeCatalog()
        {
            var statCatalog = database.statCatalog;
            statCatalog.editor_StatRemoved += Editor_OnStatRemoved;
        }

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

            if (FindItem(item.id) != null)
            {
                throw new ArgumentException
                    ($"The object is already registered within this Catalog. (id: {item.id})");
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

        /// <summary>
        /// Called when a stat is removed from the <see cref="StatCatalogAsset"/>.
        /// It cleans the <see cref="StatDetailAsset"/> by removing the
        /// deleted stat.
        /// </summary>
        /// <param name="catalog">The catalog this stat belongs to</param>
        /// <param name="definition">The removed stat.</param>
        void Editor_OnStatRemoved
            (StatCatalogAsset catalog, StatDefinitionAsset definition)
        {
            var dirty = false;

            foreach (var item in m_Items)
            {
                if (item is null) continue;

                var statDetail = item.GetDetail<StatDetailAsset>();
                if (statDetail is null) continue;

                var removed = statDetail.Editor_RemoveStat(definition);
                //if (removed)
                //{
                //    Debug.Log($"{item.displayName} ({item.GetType().Name}) has a link to {definition.displayName} ({nameof(StatDefinitionAsset)}). Updating…");
                //}
                dirty |= removed;
            }

            if (dirty)
            {
                EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Removes the deleted category from any
        /// <see cref="TItemAsset"/> instance.
        /// </summary>
        /// <param name="category">The deleted category</param>
        internal sealed override void Editor_OnCategoryRemoved
            (CategoryAsset category)
        {
            var dirty = false;
            foreach (var item in m_Items)
            {
                var removed = item.Editor_RemoveCategory(category);
                //if (removed)
                //{
                //    Debug.Log($"{item.displayName} ({item.GetType().Name}) has a link to {category.displayName} ({category.GetType().Name}). Updating…");
                //}
                dirty |= removed;
            }
            if (dirty)
            {
                EditorUtility.SetDirty(this);
            }
        }

        /// <inheritdoc/>
        protected sealed override void Editor_GetCatalogSubAssets
            (ICollection<Object> target)
        {
            if (m_Items != null)
            {
                foreach (var item in m_Items)
                {
                    if (item is null) continue;
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
            (ICollection<Object> target) { }
    }
}

#endif
