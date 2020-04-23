#if UNITY_EDITOR

using UnityEditor;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class AssetsDetailAsset
    {
        /// <inheritdoc/>
        internal override string Editor_Detail_Name => "Assets";

        /// <summary>
        /// Add an asset's name and resources path to detail.
        /// </summary>
        /// <param name="assetName">The name of an asset.</param>
        /// <param name="resourcesPath">The path that assets is stored in a resources folder.</param>
        internal void Editor_AddAsset(string assetName, string resourcesPath)
        {
            GFTools.ThrowIfArgNull(assetName, nameof(assetName));
            GFTools.ThrowIfArgNull(resourcesPath, nameof(resourcesPath));

            m_Names.Add(assetName);
            m_Values.Add(resourcesPath);

            EditorUtility.SetDirty(this);
        }
    }
}

#endif
