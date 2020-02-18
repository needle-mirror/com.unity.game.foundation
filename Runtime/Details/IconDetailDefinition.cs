using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// IconDetailDefinition.  Attach to a GameItemDefinition to store sprite information.
    /// </summary>
    /// <inheritdoc/>
    [Obsolete("Please use SpriteAssetsDetailDefinition instead.")]
    public class IconDetailDefinition : BaseDetailDefinition
    {
        /// <summary>
        /// Actual icon (sprite) used by the GameItemDefinition upon which this IconDetailDefinition is attached.
        /// </summary>
        /// <returns>Actual icon (sprite) used by the GameItemDefinition upon which this IconDetailDefinition is attached.</returns>
        public Sprite icon { get; }

        /// <summary>
        /// Obsolete. Constructor to build an IconDetailDefinition object.
        /// </summary>
        /// <param name="icon">The icon attached to this detail definition.</param>
        /// <param name="owner">The GameItemDefinition that is attached to this DetailDefinition.</param>
        /// <exception cref="System.ArgumentException">Throws if icon parameter is null.</exception>
        internal IconDetailDefinition(Sprite icon, GameItemDefinition owner = null) : base(owner)
        {
            if (icon == null)
            {
                throw new ArgumentException("IconDetailDefinition can not have a null sprite icon.");
            }

            this.icon = icon;
        }
    }
}
