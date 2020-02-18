using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Definitions for StatDefinitions.
    /// The Stat Catalog serves as a way to find references to Definitions, as needed.
    /// </summary>
    public class StatCatalog
    {
        internal readonly List<StatDefinition> m_StatDefinitions;
        
        /// <summary>
        /// Constructor to build a StatCatalog object.
        /// </summary>
        /// <param name="statDefinitions">The list of StatDefinitions that will be available in this catalog for runtime instance instantiation. If null value is passed in an empty list will be created.</param>
        internal StatCatalog(List<StatDefinition> statDefinitions)
        {
            m_StatDefinitions = statDefinitions ?? new List<StatDefinition>();
        }

        /// <summary>
        /// Find and return a StatDefinition by its Id.
        /// </summary>
        /// <param name="statDefinitionId">Id of Stat Definition we're looking for.</param>
        /// <returns>StatDefinition for specified Stat Id</returns>
        public StatDefinition GetStatDefinition(string statDefinitionId)
        {
            if (string.IsNullOrEmpty(statDefinitionId))
            {
                return null;
            }
            return GetStatDefinition(Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Find and return Stat definition by its Hash.
        /// </summary>
        /// <param name="statDefinitionHash"> Hash of Stat Definition we're looking for.</param>
        /// <returns>StatDefinition for specified Stat Hash </returns>
        public StatDefinition GetStatDefinition(int statDefinitionHash)
        {
            foreach(var definition in m_StatDefinitions)
            {
                if (definition.idHash == statDefinitionHash)
                {
                    return definition;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an array of all stat definitions in this catalog.
        /// </summary>
        /// <returns>An array of all stat definitions in this catalog.</returns>
        public StatDefinition[] GetStatDefinitions()
        {
            return m_StatDefinitions?.ToArray();
        }

        /// <summary>
        /// Fills the given list with all stat definitions in this catalog.
        /// Note: this returns the current state of all stat definitions.  To 
        /// ensure that there are no invalid or duplicate entries, the list will 
        /// always be cleared and 'recycled' (i.e. updated) with current data.
        /// </summary>
        /// <param name="statDefinitions">The list to clear and fill with updated data.</param>
        public void GetStatDefinitions(List<StatDefinition> statDefinitions)
        {
            if (statDefinitions == null)
            {
                return;
            }

            statDefinitions.Clear();

            if (m_StatDefinitions == null)
            {
                return;
            }
            
            statDefinitions.AddRange(m_StatDefinitions);
        }

        /// <summary>
        /// Determine if specified Stat definition Hash is unique in the Stat catalog.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to check.</param>
        /// <returns>True if Stat definition Hash is unique.</returns>
        public bool IsStatDefinitionHashUnique(int statDefinitionHash)
        {
            foreach (var definition in m_StatDefinitions)
            {
                if (definition.idHash == statDefinitionHash)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
