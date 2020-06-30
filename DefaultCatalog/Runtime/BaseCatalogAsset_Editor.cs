#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class BaseCatalogAsset
    {
        /// <summary>
        /// Gets all the subassets of this catalog.
        /// </summary>
        /// <param name="target">The target collection to where subassets are
        /// added.</param>
        internal void Editor_GetSubAssets(ICollection<Object> target)
        {
            Editor_GetCatalogSubAssets(target);
        }

        /// <inheritdoc cref="Editor_GetCatalogSubAssets(ICollection{Object})"/>
        protected virtual void Editor_GetCatalogSubAssets(ICollection<Object> target) { }
    }
}

#endif
