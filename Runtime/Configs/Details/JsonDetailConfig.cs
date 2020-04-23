namespace UnityEngine.GameFoundation.Configs.Details
{
    /// <summary>
    /// Configurator for an <see cref="JsonDetail"/> instance.
    /// </summary>
    public class JsonDetailConfig : BaseDetailConfig<JsonDetail>
    {
        /// <summary>
        /// The JSON content as text.
        /// </summary>
        public string json;

        /// <inheritdoc/>
        protected override JsonDetail CompileDetail()
        {
            var detail = new JsonDetail(json);

            return detail;
        }
    }
}
