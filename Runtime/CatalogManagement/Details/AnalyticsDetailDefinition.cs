namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// AnalyticsDetailDefinition. Attach to a game item to have it automatically get tracked with analytics.
    /// </summary>
    /// <inheritdoc/>
    public class AnalyticsDetailDefinition : BaseDetailDefinition
    {
        /// <summary>
        /// Returns 'friendly' display name for this AnalyticsDetailDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this AnalyticsDetailDefinition.</returns>
        public override string DisplayName() { return "Analytics Detail"; }

        /// <summary>
        /// Returns string message which explains the purpose of this AnalyticsDetailDefinition, for the purpose of displaying as a tooltip in editor.
        /// </summary>
        /// <returns>The string tooltip message of this AnalyticsDetailDefinition.</returns>
        public override string TooltipMessage() { return "Enables automatic analytics tracking of the objects created with the attached definition. For items: created, destroyed, and modified actions. For inventories: created and destroyed actions."; }

        /// <summary>
        /// Creates a runtime AnalyticsDetailDefinition based on this editor-time AnalyticsDetailDefinition.
        /// </summary>
        /// <returns>Runtime version of AnalyticsDetailDefinition.</returns>
        public override UnityEngine.GameFoundation.BaseDetailDefinition CreateRuntimeDefinition()
        {
            return new UnityEngine.GameFoundation.AnalyticsDetailDefinition();
        }
    }
}
