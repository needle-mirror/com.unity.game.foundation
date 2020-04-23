using System.Collections.Generic;
using UnityEngine.GameFoundation.Exceptions;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Detail definition to establish that item uses certain stats, and also to set default values for those stats.
    /// </summary>
    /// <inheritdoc/>
    public class StatDetail : BaseDetail
    {
        /// <summary>
        /// 
        /// </summary>
        internal readonly Dictionary<string, StatValue> m_DefaultValues =
            new Dictionary<string, StatValue>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statDefinition"></param>
        /// <returns></returns>
        public bool HasStat(StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            return m_DefaultValues.ContainsKey(statDefinition.id);
        }

        public bool HasStat(string statDefinitionId)
        {
            Tools.ThrowIfArgNull(statDefinitionId, nameof(statDefinitionId));
            return m_DefaultValues.ContainsKey(statDefinitionId);
        }

        /// <summary>
        /// Gets default value for specified stat.
        /// </summary>
        /// <param name="definition">The StatDefinition to address.</param>
        /// <returns>Default value for specified stat.</returns>
        StatValue GetDefaultValueInternal(StatDefinition definition)
        {
            var found =
                m_DefaultValues.TryGetValue(definition.id, out var value);

            if (!found)
            {
                throw new StatDefinitionNotFoundException(owner.id, definition.id);
            }

            return value;
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinition">The StatDefinition to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public StatValue GetDefaultValue(StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            return GetDefaultValueInternal(statDefinition);
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionId">The StatDefinition ID to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public StatValue GetDefaultValue(string statDefinitionId)
        {
            var statDefinition = Tools.GetStatDefinitionOrDie
                (statDefinitionId, nameof(statDefinitionId));

            return GetDefaultValueInternal(statDefinition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statDefinition"></param>
        /// <returns></returns>
        public bool HasDefinition(StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            return m_DefaultValues.ContainsKey(statDefinition.id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statDefinitionId"></param>
        /// <returns></returns>
        public bool HasDefinition(string statDefinitionId)
        {
            Tools.GetStatDefinitionOrDie
                (statDefinitionId, nameof(statDefinitionId));

            return m_DefaultValues.ContainsKey(statDefinitionId);
        }
    }
}
