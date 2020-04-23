using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Catalog for <see cref="StatDefinition"/> instances.
    /// </summary>
    public class StatCatalog
    {
        /// <summary>
        /// The container of the <see cref="StatDefinition"/> instances.
        /// </summary>
        internal Dictionary<string, StatDefinition> m_StatDefinitions;

        /// <summary>
        /// Find and return a <see cref="StatDefinition"/> by its
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="StatDefinition"/>
        /// to find.</param>
        /// <returns>The <see cref="StatDefinition"/> corresponding to the
        /// <paramref name="id"/>.</returns>
        public StatDefinition FindStatDefinition(string id)
        {
            Tools.ThrowIfArgNullOrEmpty(id, nameof(id));
            m_StatDefinitions.TryGetValue(id, out var statDefinition);

            return statDefinition;
        }

        /// <summary>
        /// Returns an array of all <see cref="StatDefinition"/> instances of
        /// this catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetStatDefinitions(ICollection{StatDefinition})"/>
        /// instead.
        /// </remarks>
        /// <returns>An array of all <see cref="StatDefinition"/> instances in
        /// this catalog.</returns>
        public StatDefinition[] GetStatDefinitions() => Tools.ToArray(m_StatDefinitions.Values);

        /// <summary>
        /// Fills the given collection with all StatDefinitions in this catalog.
        /// Note: this returns the current state of all StatDefinitions.  
        /// The list will be cleared and updated with current data.
        /// </summary>
        /// <param name="statDefinitions">The list to clear and fill with all StatDefinitions.</param>
        public void GetStatDefinitions(ICollection<StatDefinition> statDefinitions)
        {
            Tools.ThrowIfArgNull(statDefinitions, nameof(statDefinitions));

            statDefinitions.Clear();
            Tools.Copy(m_StatDefinitions.Values, statDefinitions);
        }
    }
}
