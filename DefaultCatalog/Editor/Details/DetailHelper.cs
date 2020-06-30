using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using Object = UnityEngine.Object;

namespace UnityEditor.GameFoundation.DefaultCatalog.Details
{
    /// <summary>
    /// This class contains methods that help with working with details and detail definitions in general.
    /// </summary>
    static class DetailHelper
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
        public static Dictionary<string, Type> defaultDetailInfo { get; private set; }

        /// <summary>
        /// A list of all classes that inherit from BaseDetailDefinition, that were made by the user. Call RefreshTypeDict() to make sure it's up to date. 
        /// </summary>
        public static Dictionary<string, Type> customDetailDefinitionInfo { get; private set; }

        /// <summary>
        /// Refreshes (or creates) a static list of all classes that inherit from BaseDetailDefinition.
        /// </summary>
        public static void RefreshTypeDict()
        {
            if (defaultDetailInfo == null)
            {
                defaultDetailInfo = new Dictionary<string, Type>();
            }
            else
            {
                defaultDetailInfo.Clear();
            }

            if (customDetailDefinitionInfo == null)
            {
                customDetailDefinitionInfo = new Dictionary<string, Type>();
            }
            else
            {
                customDetailDefinitionInfo.Clear();
            }

            var baseType = typeof(BaseDetailAsset);
            var baseAssembly = baseType.Assembly;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
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

            var defaultTypes = new List<Type>();
            var customTypes = new List<Type>();

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
                var inst = (BaseDetailAsset)ScriptableObject.CreateInstance(t.ToString());

                if (inst != null)
                {
                    defaultDetailInfo.Add(inst.DisplayName(), t);
                }
            }

            foreach (var t in customTypes)
            {
                var inst = (BaseDetailAsset)ScriptableObject.CreateInstance(t.ToString());

                if (inst != null)
                {
                    customDetailDefinitionInfo.Add(inst.DisplayName(), t);
                }
            }
        }
    }
}
