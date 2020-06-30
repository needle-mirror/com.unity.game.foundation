#if UNITY_EDITOR

namespace UnityEngine.GameFoundation.DefaultCatalog.Details
{
    public partial class JsonDetailAsset
    {
        /// <inheritdoc/>
        internal override string Editor_Detail_Name => "Json";

        /// <summary>
        /// Set the JSON data of the detail.
        /// </summary>
        /// <param name="jsonData">The serialized data</param>
        internal void Editor_SetJsonData(string jsonData)
        {
            m_JsonData = jsonData;
        }
    }
}

#endif
