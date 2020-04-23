namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    /// Comnfigurator for a <see cref="StatDefinition"/> instance,
    /// </summary>
    public class StatDefinitionConfig
    {
        /// <summary>
        /// The identifier of the stat definition.
        /// </summary>
        public string id { get; internal set; }

        /// <summary>
        /// The friendly name of the stat definition.
        /// </summary>
        public string displayName;

        /// <summary>
        /// The type of the stat definition.
        /// </summary>
        public StatValueType type;

        /// <summary>
        /// The <see cref="StatDefinition"/> instance built by this configurator.
        /// </summary>
        internal StatDefinition runtimeItem;

        /// <summary>
        /// Checks the configuration and builds the
        /// <see cref="StatDefinition\"/> instance.
        /// </summary>
        internal void Compile()
        {
            runtimeItem = new StatDefinition(id, displayName, type);
        }
    }
}
