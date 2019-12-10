namespace UnityEngine.GameFoundation
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
        public override string TooltipMessage() { return "This enables automatic analytics tracking of the objects created using the definition it is attached to. For items it will track created, destroyed, and modified actions, for inventories it will track created and destroyed actions."; }
    }
}
