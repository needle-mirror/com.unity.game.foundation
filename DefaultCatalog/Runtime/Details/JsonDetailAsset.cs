using System;
using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Configs.Details;
using UnityEngine.Serialization;

namespace UnityEngine.GameFoundation.DefaultCatalog.Details
{
    [Obsolete]
    public sealed partial class JsonDetailAsset : BaseDetailAsset
    {
        [SerializeField, FormerlySerializedAs("jsonData")]
        internal string m_JsonData;

        public override string DisplayName() => "Json Detail";

        public string jsonData => m_JsonData;

        internal override BaseDetailConfig CreateConfig()
        {
            var config = new JsonDetailConfig();
            config.json = m_JsonData;
            return config;
        }
    }
}
