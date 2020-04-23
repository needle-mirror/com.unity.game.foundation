namespace UnityEngine.GameFoundation.Configs.Details
{
    /// <summary>
    /// Configurator for an <see cref="AnalyticsDetail"/> instance.
    /// </summary>
    public sealed class AnalyticsDetailConfig : BaseDetailConfig<AnalyticsDetail>
    {
        /// <inheritdoc/>
        protected override sealed AnalyticsDetail CompileDetail()
            => new AnalyticsDetail();
    }
}
