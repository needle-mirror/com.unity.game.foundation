namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// AnalyticsDetailDefinition. Attach to a game item to have it automatically get tracked with analytics.
    /// </summary>
    /// <inheritdoc/>
    public class AnalyticsDetailDefinition : BaseDetailDefinition
    {
        /// <summary>
        /// Constructor to build an AnalyticsDetailDefinition object.
        /// </summary>
        /// <param name="owner">The GameItemDefinition that is attached to this DetailDefinition.</param>
        internal AnalyticsDetailDefinition(GameItemDefinition owner = null) : base(owner)
        {
        }
    }
}
