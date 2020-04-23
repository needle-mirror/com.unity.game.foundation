#if UNITY_EDITOR

using UnityEditor;
using System.Collections.Generic;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    internal delegate void Editor_StatAssetRemovedEventHandler
        (StatCatalogAsset catalog, StatDefinitionAsset definition);

    public partial class StatCatalogAsset
    {
        internal event Editor_StatAssetRemovedEventHandler editor_StatRemoved;

        /// <summary>
        /// Adds the given <paramref name="statDefinition"/> to this catalog.
        /// </summary>
        /// <param name="statDefinition">The StatDefinition to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        internal bool Editor_AddStatDefinition(StatDefinitionAsset statDefinition)
        {
            GFTools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            if (m_StatDefinitions.Contains(statDefinition))
            {
                Debug.LogWarning
                    ($"The object is already registered within this Stat Catalog. (id: {statDefinition.id})");
                return false;
            }

            m_StatDefinitions.Add(statDefinition);

            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        /// Removes the given <paramref name="statDefinition"/> from this catalog.
        /// </summary>
        /// <param name="statDefinition">The <see cref="StatDefinitionAsset"/>
        /// to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        internal bool Editor_RemoveStatDefinition(StatDefinitionAsset statDefinition)
        {
            GFTools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            var removed = m_StatDefinitions.Remove(statDefinition);
            if (removed)
            {
                EditorUtility.SetDirty(this);
                Editor_DispatchStatRemoved(statDefinition);
            }

            return removed;
        }

        /// <summary>
        /// Triggers the stat removed event.
        /// </summary>
        /// <param name="stat">The <see cref="StatDefinitionAsset"/> removed.</param>
        internal void Editor_DispatchStatRemoved(StatDefinitionAsset stat)
        {
            editor_StatRemoved?.Invoke(this, stat);
        }

        /// <summary>
        /// Gets all the subassets of this catalog.
        /// </summary>
        /// <param name="target">The target collection to where the subassets
        /// will be added</param>
        internal void Editor_GetSubAssets(ICollection<Object> target)
        {
            if (m_StatDefinitions is null) return;

            foreach (var statDefinition in m_StatDefinitions)
            {
                if (statDefinition is null) continue;
                target.Add(statDefinition);
            }
        }
    }
}

#endif
