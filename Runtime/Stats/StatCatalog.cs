using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Definitions for StatDefinitions.
    /// The Stat Catalog serves as a way to find references to Definitions, as needed.
    /// </summary>
    [CreateAssetMenu(fileName = "StatCatalog.asset", menuName = "Game Foundation/Catalog/Stat Catalog")]
    public class StatCatalog : ScriptableObject
    {
        [SerializeField]
        internal List<StatDefinition> m_StatDefinitions = new List<StatDefinition>();

        /// <summary>
        /// This is an enumerator for iterating through all StatDefinitions in the StatCatalog.
        /// </summary>
        /// <returns>An enumerable for iterating through all StatDefinitions.</returns>
        public IEnumerable<StatDefinition> allStatDefinitions
        {
            get { return m_StatDefinitions; }
        }

        internal StatCatalog()
        {
        }

        /// <summary>
        /// Creates a new StatCatalog.
        /// </summary>
        /// <returns>Reference to the newly made StatCatalog.</returns>
        public static StatCatalog Create()
        {
            Tools.ThrowIfPlayMode("Cannot create a StatCatalog while in play mode.");

            var statCatalog = ScriptableObject.CreateInstance<StatCatalog>();

            return statCatalog;
        }

        /// <summary>
        /// Find and return stat definition.
        /// </summary>
        /// <returns>StatDefinition for specified stat id</returns>
        public StatDefinition GetStatDefinition(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
            {
                return null;
            }
            return GetStatDefinition(Tools.StringToHash(definitionId));
        }

        /// <summary>
        /// Find and return stat definition.
        /// </summary>
        /// <returns>StatDefinition for specified stat id hash</returns>
        public StatDefinition GetStatDefinition(int hashId)
        { 
            foreach(var definition in m_StatDefinitions)
            {
                if (definition.idHash == hashId)
                {
                    return definition;
                }
            }

            return null;
        }

        /// <summary>
        /// Determine if specified stat definition hash is unique in the stat catalog.
        /// </summary>
        /// <param name="hash">The StatDefinition ID hash to check.</param>
        /// <returns>True if stat definition hash is unique.</returns>
        public bool IsStatDefinitionHashUnique(int hash)
        {
            foreach (var definition in m_StatDefinitions)
            {
                if (definition.idHash == hash)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Adds the given StatDefintion to this Catalog.
        /// </summary>
        /// <param name="definition">The StatDefinition to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        public bool AddStatDefinition(StatDefinition definition)
        {
            Tools.ThrowIfPlayMode("Cannot add a StatDefinition to a Catalog while in play mode.");

            if (definition == null)
                return false;

            if (m_StatDefinitions.Contains(definition))
            {
                Debug.LogWarning("The object is already registered within this Stat Catalog. (id: " + definition.id + ", hash: " + definition.idHash + ")");
                return false;
            }

            if (GetStatDefinition(definition.idHash) != null)
            {
                Debug.LogWarning("The hash for this object is already registered within this Stat Catalog. (id: " + definition.id + ", hash: " + definition.idHash + ")");
                return false;
            }

            m_StatDefinitions.Add(definition);
            return true;
        }

        /// <summary>
        /// Removes the given StatDefinition from this Catalog.
        /// </summary>
        /// <param name="definition">The StatDefinition to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveStatDefinition(StatDefinition definition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a StatDefinition from a Catalog while in play mode.");

            // definition.OnRemove();

            return m_StatDefinitions.Remove(definition);
        }
    }
}
