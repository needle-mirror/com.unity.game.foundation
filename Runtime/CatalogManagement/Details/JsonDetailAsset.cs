using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Configs.Details;
using UnityEngine.Serialization;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public sealed partial class JsonDetailAsset : BaseDetailAsset
    {
        [SerializeField, FormerlySerializedAs("jsonData")]
        internal string m_JsonData;

        public override string DisplayName() => "Json Detail";

        public string jsonData => m_JsonData;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal sealed override BaseDetailConfig CreateConfig()
        {
            var config = new JsonDetailConfig();
            config.json = m_JsonData;
            return config;
        }
    }
}
