using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// This class contains methods that help with working with details and detail definitions in general.
    /// </summary>
    internal static class DetailHelper
    {
        /// <summary>
        /// Used mostly as part of a workaround in custom editors where targets might not yet be populated due to a bug in Unity
        /// </summary>
        public static bool IsNullOrEmpty(this Object[] targets)
        {
            return (targets == null || targets.Length <= 0 || targets[0] == null);
        }

        /// <summary>
        /// A list of all classes that inherit from BaseDetailDefinition, that come with Game Foundation. Call RefreshTypeDict() to make sure it's up to date. 
        /// </summary>
        public static Dictionary<string, System.Type> defaultDetailDefinitionInfo { get; private set; }

        /// <summary>
        /// A list of all classes that inherit from BaseDetailDefinition, that were made by the user. Call RefreshTypeDict() to make sure it's up to date. 
        /// </summary>
        public static Dictionary<string, System.Type> customDetailDefinitionInfo { get; private set; }

        /// <summary>
        /// Refreshes (or creates) a static list of all classes that inherit from BaseDetailDefinition.
        /// </summary>
        public static void RefreshTypeDict()
        {
            if (defaultDetailDefinitionInfo == null)
            {
                defaultDetailDefinitionInfo = new Dictionary<string, System.Type>();
            }
            else
            {
                defaultDetailDefinitionInfo.Clear();
            }

            if (customDetailDefinitionInfo == null)
            {
                customDetailDefinitionInfo = new Dictionary<string, System.Type>();
            }
            else
            {
                customDetailDefinitionInfo.Clear();
            }

            var baseType = typeof(BaseDetailDefinition);
            var baseAssembly = baseType.Assembly;
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            Assembly userAssembly = null;

            foreach (var assembly in assemblies)
            {
                if (!assembly.GetName().ToString().Contains("Assembly-CSharp,"))
                {
                    continue;
                }
                userAssembly = assembly;
                break;
            }

            var defaultTypes = new List<System.Type>();
            var customTypes = new List<System.Type>();

            defaultTypes.AddRange(baseAssembly
                .GetTypes()
                .Where(t => t.IsClass
                            && !t.IsAbstract
                            && baseType.IsAssignableFrom(t)
                ));

            if (userAssembly != null)
            {
                customTypes.AddRange(userAssembly
                    .GetTypes()
                    .Where(t => t.IsClass
                                && !t.IsAbstract
                                && baseType.IsAssignableFrom(t)
                    ));
            }

            foreach (var t in defaultTypes)
            {
                var inst = (BaseDetailDefinition)ScriptableObject.CreateInstance(t.ToString());

                if (inst != null)
                {
                    defaultDetailDefinitionInfo.Add(inst.DisplayName(), t);
                }
            }

            foreach (var t in customTypes)
            {
                var inst = (BaseDetailDefinition)ScriptableObject.CreateInstance(t.ToString());

                if (inst != null)
                {
                    customDetailDefinitionInfo.Add(inst.DisplayName(), t);
                }
            }
        }
    }
}
