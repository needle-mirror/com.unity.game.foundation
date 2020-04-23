using System;

namespace UnityEngine.GameFoundation.Configs
{
    /// <summary>
    /// Configurator for a <see cref="Category"/> instance.
    /// </summary>
    public sealed class CategoryConfig
    {
        /// <summary>
        /// The identifier of the category.
        /// </summary>
        public string id { get; internal set; }

        /// <summary>
        /// The friendly name of the category.
        /// </summary>
        public string displayName;

        /// <summary>
        /// The category built by this configurator.
        /// </summary>
        internal Category runtimeCategory;

        /// <summary>
        /// Checks the configuration and builds the <see cref="Category"/>
        /// instance.
        /// </summary>
        internal void Compile()
        {
            Tools.ThrowIfArgNullOrEmpty(id, nameof(id));

            if (!Tools.IsValidId(id))
            {
                throw new Exception($"Id {id} is not valid");
            }

            Tools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            runtimeCategory = new Category(id, displayName);
        }
    }
}