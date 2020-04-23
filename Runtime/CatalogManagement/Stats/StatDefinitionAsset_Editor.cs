#if UNITY_EDITOR

using UnityEditor;
using System;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public sealed partial class StatDefinitionAsset
    {
        /// <summary>
        /// Creates and initializes a new <see cref="StatDefinitionAsset"/>.
        /// </summary>
        /// <param name="id">Id of the new <see cref="StatDefinitionAsset"/></param>
        /// <param name="displayName">Diplay name of the new
        /// <see cref="StatDefinitionAsset"/></param>
        /// <param name="statValueType">Type of the new
        /// <see cref="StatDefinitionAsset"/></param>
        /// <returns>The new <see cref="StatDefinitionAsset"/></returns>
        internal static StatDefinitionAsset Editor_Create
            (string id, string displayName, StatValueType statValueType)
        {
            GFTools.ThrowIfArgNullOrEmpty(id, nameof(id));
            GFTools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            if (!Tools.IsValidId(id))
            {
                throw new ArgumentException
                    ($"{nameof(StatDefinitionAsset)} {nameof(StatDefinitionAsset.id)} can only be alphanumeric with optional dashes or underscores.");
            }

            var statDefinition = CreateInstance<StatDefinitionAsset>();
            statDefinition.m_Id = id;
            statDefinition.m_DisplayName = displayName;
            statDefinition.m_StatValueType = statValueType;

            statDefinition.name = statDefinition.Editor_AssetName;

            return statDefinition;
        }

        /// <summary>
        /// Returns the name to assign to the asset file of this
        /// <see cref="StatDefinitionAsset"/> instance.
        /// </summary>
        internal string Editor_AssetName => $"Stat_{id}";

        /// <summary>
        /// Sets the friendly name of the stat definition.
        /// </summary>
        /// <param name="this">The <see cref="StatDefinitionAsset"/> to define
        /// the display name</param>
        /// <param name="displayName">The new display name.</param>
        internal void Editor_SetDiplayName(string displayName)
        {
            GFTools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            m_DisplayName = displayName;

            EditorUtility.SetDirty(this);
        }

        void OnDestroy()
        {
            if (catalog is null) return;
            //Debug.Log($"Removing {displayName} ({GetType().Name}) from {catalog}");
            catalog.Editor_RemoveStatDefinition(this);
        }
    }
}

#endif
