#if UNITY_EDITOR

using System;
using UnityEditor;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class CategoryAsset
    {
        /// <summary>
        /// Creates a CategoryAsset.
        /// </summary>
        /// <param name="id">The identifier of the
        /// <see cref="CategoryAsset"/>.</param>
        /// <param name="displayName">The friendly name of the
        /// <see cref="CategoryAsset"/></param>
        /// <returns>The newly created <see cref="CategoryAsset"/></returns>
        /// <exception cref="ArgumentException">Thrown if an empty Id is
        /// given.</exception>
        internal static CategoryAsset Editor_Create(string id, string displayName)
        {
            GFTools.ThrowIfArgNullOrEmpty(id, nameof(id));
            GFTools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            if (!Tools.IsValidId(id))
            {
                throw new ArgumentException
                    ($"{nameof(CategoryAsset)} {nameof(CategoryAsset.id)} can only be alphanumeric with optional dashes or underscores.");
            }

            if (GameFoundationDatabaseSettings.database.ContainsCategory(id))
            {
                EditorUtility.DisplayDialog(
                    "Error: Duplicate id",
                    $"A category with id {id} already exist in the database",
                    "OK");

                return null;
            }
            else
            {
                var category = CreateInstance<CategoryAsset>();
                category.m_DisplayName = displayName;
                category.m_Id = id;
                category.name = category.Editor_AssetName;
                return category;
            }
        }

        /// <summary>
        /// Returns the name to assign to the asset file of this
        /// <see cref="CategoryAsset"/> instance.
        /// </summary>
        internal string Editor_AssetName => $"Category_{id}";

        /// <summary>
        /// Sets the friendly name of this <see cref="CategoryAsset"/> instance.
        /// </summary>
        /// <param name="displayName">The friendly name to assign to the
        /// <see cref="CategoryAsset"/> instance.</param>
        internal void Editor_SetDisplayName(string displayName)
        {
            GFTools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            m_DisplayName = displayName;

            EditorUtility.SetDirty(this);
        }

        void OnDestroy()
        {
            if (catalog is null) return;
            //Debug.Log($"Removing {displayName} ({GetType().Name}) from {catalog}");
            catalog.Editor_RemoveCategory(this);
        }
    }
}

#endif
