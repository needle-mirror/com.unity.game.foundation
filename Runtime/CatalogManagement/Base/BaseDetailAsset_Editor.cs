#if UNITY_EDITOR

using System.Collections.Generic;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class BaseDetailAsset
    {
        /// <summary>
        /// The name of the detail.
        /// Used in Editor to assign a name to the asset of this detail.
        /// </summary>
        internal abstract string Editor_Detail_Name { get; }

        /// <summary>
        /// Returns the name to assign to this asset.
        /// </summary>
        /// <returns></returns>
        internal string Editor_AssetName => $"Detail_{itemDefinition.id}_{Editor_Detail_Name}";

        /// <summary>
        /// Gets all the subassets of this detail.
        /// </summary>
        /// <param name="target">The target collection to where subassets are
        /// added.</param>
        internal void Editor_GetSubAssets(ICollection<Object> target)
            => Editor_GetDetailSubAssets(target);

        /// <inheritdoc cref="Editor_GetSubAssets(ICollection{Object})"/>
        protected virtual void Editor_GetDetailSubAssets(ICollection<Object> target) { }
    }
}

#endif
