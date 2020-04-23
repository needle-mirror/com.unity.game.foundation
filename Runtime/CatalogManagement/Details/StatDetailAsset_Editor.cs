#if UNITY_EDITOR

using UnityEditor;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Detail definition to establish that item uses certain stats, and also to set default values for those stats.
    /// </summary>
    public partial class StatDetailAsset
    {
        /// <inheritdoc/>
        internal override string Editor_Detail_Name => "Stats";

        /// <summary>
        /// Sets the default value of the <paramref name="statDefinition"/>.
        /// </summary>
        /// <param name="statDefinition">The <see cref="StatDefinitionAsset"/>
        /// to add.</param>
        /// <param name="value">The default value for the stat.</param>
        internal void Editor_SetStat
            (StatDefinitionAsset statDefinition, StatValue value)
        {
            GFTools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            m_Stats[statDefinition] = value;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Remove the <paramref name="statDefinition"/>.
        /// </summary>
        /// <param name="statDefinition">The <see cref="StatDefinitionAsset"/>
        /// to remove.</param>
        /// <returns>True if the specified default stat was removed.</returns>
        internal bool Editor_RemoveStat(StatDefinitionAsset statDefinition)
        {
            GFTools.ThrowIfArgNull(statDefinition, nameof(statDefinition));

            var removed = m_Stats.Remove(statDefinition);

            if (removed)
            {
                EditorUtility.SetDirty(this);
            }

            return removed;
        }
    }
}

#endif
