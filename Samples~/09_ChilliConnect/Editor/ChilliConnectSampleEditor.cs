using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Sample;
using UnityEngine;

namespace UnityEditor.GameFoundation.Sample
{
    [CustomEditor(typeof(ChilliConnectSample))]
    public class ChilliConnectSampleEditor : Editor
    {
        const string k_ChilliConnectDefine = "CHILLICONNECT_ENABLED";

        Action m_Action;

        bool m_AreChilliConnectPackagesImported;

        bool m_IsSymbolDefined;

        void OnEnable()
        {
            m_AreChilliConnectPackagesImported = AreChilliConnectPackagesImported();
            m_IsSymbolDefined = IsSymbolDefinedFor(k_ChilliConnectDefine, EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        public override void OnInspectorGUI()
        {
            var clicked = GUILayout.Button("Get the ChilliConnect SDK Package");
            if (clicked)
            {
                m_Action = () => Application.OpenURL("https://docs.chilliconnect.com/guide/sdks/#unity");
            }

            if (m_AreChilliConnectPackagesImported)
            {
                if (m_IsSymbolDefined)
                {
                    clicked = GUILayout.Button("Use default data access layer");
                    if (clicked)
                    {
                        m_Action = () => UndefineSymbolFor(k_ChilliConnectDefine, EditorUserBuildSettings.selectedBuildTargetGroup);
                    }
                }
                else
                {
                    clicked = GUILayout.Button("Use ChilliConnect data access layer");
                    if (clicked)
                    {
                        m_Action = () => DefineSymbolFor(k_ChilliConnectDefine, EditorUserBuildSettings.selectedBuildTargetGroup);
                    }
                }
            }

            base.OnInspectorGUI();

            PerformAction();
        }

        void PerformAction()
        {
            if (m_Action is null) return;

            try
            {
                m_Action();
            }
            finally
            {
                m_Action = null;
            }
        }

        bool IsSymbolDefinedFor(string symbol, BuildTargetGroup targetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            return defines.Contains(symbol);
        }

        static bool AreChilliConnectPackagesImported()
        {
            var chilliConnectSdkGuids = AssetDatabase.FindAssets("ChilliConnectSdk t:Script");
            if (chilliConnectSdkGuids.Length > 0)
            {
                var chilliConnectAdapterGuids = AssetDatabase.FindAssets("ChilliConnectCloudSync t:Script");
                return chilliConnectAdapterGuids.Length > 0;
            }

            return false;
        }

        static void DefineSymbolFor(string symbol, BuildTargetGroup targetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            var oldDefinedSymbols = defines.Split(
                new[] { ';' },
                StringSplitOptions.RemoveEmptyEntries);
            var newDefinedSymbols = new List<string>(oldDefinedSymbols);
            newDefinedSymbols.Add(symbol);

            defines = string.Join(";", newDefinedSymbols.ToArray());

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        }

        static void UndefineSymbolFor(string symbol, BuildTargetGroup targetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            var oldDefinedSymbols = defines.Split(
                new[] { ';' },
                StringSplitOptions.RemoveEmptyEntries);
            var newDefinedSymbols = new List<string>(oldDefinedSymbols);
            newDefinedSymbols.Remove(symbol);

            defines = string.Join(";", newDefinedSymbols.ToArray());

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        }
    }
}
