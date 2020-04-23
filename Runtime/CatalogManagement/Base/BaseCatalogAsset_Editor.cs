#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class BaseCatalogAsset
    {
        /// <summary>
        /// Can be overridden if removing a <paramref name="category"/> has a
        /// consequence for the inherited catalog.
        /// </summary>
        /// <param name="category">The <see cref="CategoryAsset"/>
        /// removed.</param>
        internal virtual void Editor_OnCategoryRemoved(CategoryAsset category) { }

        /// <summary>
        /// Adds the given <paramref name="category"/> to this Catalog.
        /// </summary>
        /// <param name="category">The <see cref="CategoryAsset"/> instance to
        /// add.</param>
        /// <returns>Whether or not the <paramref name="category"/> was added
        /// successfully.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate entry is
        /// given.</exception>
        internal bool Editor_AddCategory(CategoryAsset category)
        {
            GFTools.ThrowIfArgNull(category, nameof(category));

            if (FindCategory(category.id) != null)
            {
                throw new ArgumentException
                    ($"The object is already registered within this Catalog. (id: {category.id}");
            }

            m_Categories.Add(category);
            category.catalog = this;

            EditorUtility.SetDirty(this);
            return true;
        }

        /// <summary>
        /// Removes the given <paramref name="category"/> from this catalog.
        /// </summary>
        /// <param name="category">The <see cref="CategoryAsset"/> to
        /// remove.</param>
        /// <returns>Whether or not the <paramref name="category"/> was
        /// successfully removed.</returns>
        internal bool Editor_RemoveCategory(CategoryAsset category)
        {
            GFTools.ThrowIfArgNull(category, nameof(category));

            if (m_Categories.Remove(category))
            {
                Editor_OnCategoryRemoved(category);
                EditorUtility.SetDirty(this);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all the subassets of this catalog.
        /// </summary>
        /// <param name="target">The target collection to where subassets are
        /// added.</param>
        internal void Editor_GetSubAssets(ICollection<Object> target)
        {
            if (m_Categories != null)
            {
                foreach (var category in m_Categories)
                {
                    if (category is null) continue;
                    target.Add(category);
                }
            }

            Editor_GetCatalogSubAssets(target);
        }

        /// <inheritdoc cref="Editor_GetCatalogSubAssets(ICollection{Object})"/>
        protected virtual void Editor_GetCatalogSubAssets(ICollection<Object> target) { }
    }
}

#endif
