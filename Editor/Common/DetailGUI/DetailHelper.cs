using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// This class contains methods that help with working with details and detail definitions in general.
    /// </summary>
    internal static class DetailHelper
    {
        private static Dictionary<string, Type> m_DefaultDetailDefinitionInfo;
        private static Dictionary<string, Type> m_CustomDetailDefinitionInfo;

        /// <summary>
        /// Used mostly as part of a workaround in custom editors where targets might not yet be populated due to a bug in Unity
        /// </summary>
        public static bool IsNullOrEmpty(this UnityEngine.Object[] targets)
        {
            return (targets == null || targets.Length <= 0 || targets[0] == null);
        }

        /// <summary>
        /// A list of all classes that inherit from BaseDetailDefinition, that come with Game Foundation. Call RefreshTypeDict() to make sure it's up to date. 
        /// </summary>
        public static Dictionary<string, Type> defaultDetailDefinitionInfo
        {
            get { return m_DefaultDetailDefinitionInfo; }
        }

        /// <summary>
        /// A list of all classes that inherit from BaseDetailDefinition, that were made by the user. Call RefreshTypeDict() to make sure it's up to date. 
        /// </summary>
        public static Dictionary<string, Type> customDetailDefinitionInfo
        {
            get { return m_CustomDetailDefinitionInfo; }
        }

        /// <summary>
        /// Refreshes (or creates) a static list of all classes that inherit from BaseDetailDefinition.
        /// </summary>
        public static void RefreshTypeDict()
        {
            if (m_DefaultDetailDefinitionInfo == null)
            {
                m_DefaultDetailDefinitionInfo = new Dictionary<string, Type>();
            }
            else
            {
                m_DefaultDetailDefinitionInfo.Clear();
            }

            if (m_CustomDetailDefinitionInfo == null)
            {
                m_CustomDetailDefinitionInfo = new Dictionary<string, Type>();
            }
            else
            {
                m_CustomDetailDefinitionInfo.Clear();
            }
            
            var baseType = typeof(BaseDetailDefinition);
            var baseAssembly = baseType.Assembly;
            
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly userAssembly = null;
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().ToString().Contains("Assembly-CSharp,"))
                {
                    userAssembly = assembly;
                    break;
                }
            }
            
            List<Type> defaultTypes = new List<Type>();
            List<Type> customTypes = new List<Type>();
            
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
            
            foreach (Type t in defaultTypes)
            {
                BaseDetailDefinition inst = (BaseDetailDefinition)ScriptableObject.CreateInstance(t.ToString());
                if (inst != null)
                {
                    m_DefaultDetailDefinitionInfo.Add(inst.DisplayName(), t);
                }
            }
            
            foreach (Type t in customTypes)
            {
                BaseDetailDefinition inst = (BaseDetailDefinition)ScriptableObject.CreateInstance(t.ToString());
                if (inst != null)
                {
                    m_CustomDetailDefinitionInfo.Add(inst.DisplayName(), t);
                }
            }
        }
    }
}
