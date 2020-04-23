using System;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// This is one entry in the list of possible stats an item could have.
    /// </summary>
    public sealed partial class StatDefinitionAsset : ScriptableObject
    {
        /// <summary>
        /// The identifier of the stat.
        /// </summary>
        [SerializeField]
        internal string m_Id;

        /// <summary>
        /// The display name of the stat.
        /// </summary>
        [SerializeField]
        internal string m_DisplayName;

        /// <summary>
        /// The type of the stat.
        /// </summary>
        [SerializeField]
        internal StatValueType m_StatValueType;

        /// <summary>
        /// A reference to the catalog containing this
        /// <see cref="StatDefinitionAsset"/>.
        /// </summary>
        [field: NonSerialized]
        public StatCatalogAsset catalog { get; internal set; }

        /// <summary>
        /// Prevent from using the cosntructor.
        /// </summary>
        internal StatDefinitionAsset()
        {
        }


        /// <summary>
        /// Id for this Stat definition.
        /// </summary>
        /// <returns>id for this Stat definition.</returns>
        public string id => m_Id;

        /// <summary>
        /// Custom string attached to this Stat definition.
        /// </summary>
        /// <returns>Custom string attached to this Stat definition.</returns>
        public string displayName => m_DisplayName;

        /// <summary>
        /// Stat value type for this Stat definition.
        /// </summary>
        /// <returns>Stat value type for this Stat definition.</returns>
        public StatValueType statValueType => m_StatValueType;

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with this stat
        /// definition.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        internal void Configure(CatalogBuilder builder)
        {
            var statConfig = builder.CreateStat(id);
            statConfig.displayName = displayName;
            statConfig.type = m_StatValueType;
        }
    }
}
