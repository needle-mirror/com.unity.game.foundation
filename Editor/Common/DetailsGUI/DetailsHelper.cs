using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// This class contains methods that help with working with details and details definitions in general.
    /// </summary>
    internal static class DetailsHelper
    {
        private static Dictionary<string, System.Type> m_DetailsDefinitionInfo;

        /// <summary>
        /// A list of all classes that inherit from BaseDetailsDefinition. Call RefreshTypeDict() to make sure it's up to date. 
        /// </summary>
        public static Dictionary<string, System.Type> detailsDefinitionInfo
        {
            get { return m_DetailsDefinitionInfo; }
        }

        /// <summary>
        /// Refreshes (or creates) a static list of all classes that inherit from BaseDetailsDefinition.
        /// </summary>
        public static void RefreshTypeDict()
        {
            m_DetailsDefinitionInfo = new Dictionary<string, System.Type>();

            var baseType = typeof(BaseDetailsDefinition);
            var assembly = baseType.Assembly;

            var detailsDefinitionList = assembly
                .GetTypes()
                .Where(t => t.IsClass
                    && !t.IsAbstract
                    && baseType.IsAssignableFrom(t)
                    );

            foreach (System.Type t in detailsDefinitionList)
            {
                BaseDetailsDefinition inst = (BaseDetailsDefinition)ScriptableObject.CreateInstance(t.ToString());
                m_DetailsDefinitionInfo.Add(inst.DisplayName(), t);
            }
        }
    }
}
