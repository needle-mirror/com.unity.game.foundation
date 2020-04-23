using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Configs.Details;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Attach to a game item to have it automatically get tracked with analytics.
    /// </summary>
    /// <inheritdoc/>
    public sealed partial class AnalyticsDetailAsset : BaseDetailAsset
    {
        /// <inheritdoc/>
        public override string DisplayName() { return "Analytics Detail"; }

        /// <inheritdoc/>
        public override string TooltipMessage() { return "Enables automatic analytics tracking of the objects created with the attached definition. For items: created, destroyed, and modified actions. For inventories: created and destroyed actions."; }

        /// <inheritdoc />
        internal sealed override BaseDetailConfig CreateConfig()
            => new AnalyticsDetailConfig();
    }
}
