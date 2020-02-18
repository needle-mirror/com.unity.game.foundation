using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Detail definition to establish that item uses certain stats, and also to set default values for those stats.
    /// </summary>
    /// <inheritdoc/>
    public class StatDetailDefinition : BaseDetailDefinition
    {
        Dictionary<int, StatUnion> m_StatDefaultValues;
        internal Dictionary<int, StatUnion> statDefaultValues => m_StatDefaultValues;

        /// <summary>
        /// Constructor to build a StatDetailDefinition object.
        /// </summary>
        /// <param name="statDefaultValues">The dictionary of default stat values. If null value is passed in an empty dictionary will be created.</param>
        /// <param name="owner">The GameItemDefinition that is attached to this DetailDefinition.</param>
        internal StatDetailDefinition(Dictionary<int, StatUnion> statDefaultValues, GameItemDefinition owner = null)
            : base(owner)
        {
            m_StatDefaultValues = statDefaultValues ?? new Dictionary<int, StatUnion>();
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionId">The StatDefinition ID to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public int GetStatInt(string statDefinitionId)
        {
            return GetStatInt(Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public int GetStatInt(int statDefinitionHash)
        {
            if (statDefaultValues != null
                && statDefaultValues.TryGetValue(statDefinitionHash, out var value))
            {
                return value.intValue;
            }

            throw new KeyNotFoundException("Attempted to get stat default int value for stat that does not exist.");
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionId">The StatDefinition ID to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public float GetStatFloat(string statDefinitionId)
        {
            return GetStatFloat(Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public float GetStatFloat(int statDefinitionHash)
        {
            if (statDefaultValues != null
                && statDefaultValues.TryGetValue(statDefinitionHash, out var value))
            {
                return value.floatValue;
            }

            throw new KeyNotFoundException("Attempted to get stat default float value for stat that does not exist.");
        }
    }
}
