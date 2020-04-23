#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.GameFoundation
{
    [CustomEditor(typeof(GameFoundationSettings))]
    public class GameFoundationSettingsCustomEditor : Editor 
    {
        const string k_GfIapDefine = "UNITY_PURCHASING_FOR_GAME_FOUNDATION";
        const float k_EnableFlagDelay = .25f;
        const float k_EnableFlagTimeoutDelay = 30;

        //NOTE: all targets that permit changing player settings defines without logging errors
        //TODO: consider targetting only specific platforms that actually allow IAP  -OR-  target all and ignore debug errors generated
        static readonly BuildTargetGroup[] k_BuildTargetGroups = 
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.iOS,
            BuildTargetGroup.Android,
            BuildTargetGroup.WebGL,
            BuildTargetGroup.WSA,
            BuildTargetGroup.PS4,
            BuildTargetGroup.XboxOne,
            BuildTargetGroup.tvOS,
            BuildTargetGroup.Switch,
            BuildTargetGroup.Lumin
        };

        SerializedProperty m_EnablePlayModeAnalytics;
        SerializedProperty m_EnableEditorModeAnalytics;

        // flag for new desired state (output from check box on 'Purchasing Enabled' Toggle
        static bool m_NewEnabledFlag;

        // delay before starting to change player settings to allow user to visualize requested change and avoid confusing inaction
        static float m_EnableFlagDelayTime = 0;

        // timeout to reset process if things go south (i.e. iap setting changed/unity closed/etc)
        static float m_EnableFlagTimeout = 0;
    
        // initialize custom editor
        void OnEnable()
        {
            m_EnablePlayModeAnalytics = serializedObject.FindProperty("m_EnablePlayModeAnalytics");
            m_EnableEditorModeAnalytics = serializedObject.FindProperty("m_EnableEditorModeAnalytics");

            m_NewEnabledFlag = GameFoundationSettings.purchasingEnabled;
            m_EnableFlagDelayTime = 0;
            m_EnableFlagTimeout = 0;
        }

        // update inspector (shows fields, allows updating, handles tooltips, etc)
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 50;
            EditorGUILayout.PropertyField(m_EnablePlayModeAnalytics, new GUIContent("Enable Play Mode Analytics", "Sets flag to indicate whether analytics events should be fired while in Play Mode."));
            EditorGUILayout.PropertyField(m_EnableEditorModeAnalytics, new GUIContent("Enable Editor Mode Analytics", "Sets flag to indicate whether analytic events should be fired while in Editor Mode."));

            bool canBeEnabled = CanPurchasingBeEnabled();

            // setup enable toggle title text
            var title = "Purchasing Enabled";
            bool workingFlag = m_NewEnabledFlag != GameFoundationSettings.purchasingEnabled;
            if (workingFlag)
            {
                title = m_NewEnabledFlag ? "Purchasing Enabled (enabling)" : "Purchasing Enabled (disabling)";
            }

            // setup tool tip with dynamic current status
            var tooltip = new StringBuilder();
            if (canBeEnabled)
            {
                tooltip.Append("Enables/disables Unity Purchasing within Game Foundation.");
            }
            else
            {
                tooltip.Append("Unavailable.  Important: IAP must be On and Imported in Window > General > Services for this option to be available.");
            }
#if UNITY_PURCHASING
            tooltip.Append("\n\nDEBUG info:\n  Unity Purchasing: On");
#else
            tooltip.Append("\n\nDEBUG info:\n  Unity Purchasing: Off");
#endif
            tooltip.Append(DoesPurchasingPackageExist() ? "\n  Purchasing package: Exists." : "\n  Purchasing package: Does NOT exist.");
#if UNITY_PURCHASING_FOR_GAME_FOUNDATION
            tooltip.Append("\n  Game Foundation Purchasing: On");
#else
            tooltip.Append("\n  Game Foundation Purchasing: Off");
#endif

            // show option for gf with iap
            EditorGUI.BeginDisabledGroup(workingFlag || !canBeEnabled);
            m_NewEnabledFlag = EditorGUILayout.Toggle(new GUIContent(title, tooltip.ToString()), m_NewEnabledFlag);
            EditorGUI.EndDisabledGroup();

            // if user did NOT change the setting (or previous change is complete)
            bool isFlagCorrect = m_NewEnabledFlag == GameFoundationSettings.purchasingEnabled;
            if (isFlagCorrect)
            {
                // reset everything to prep for and allow next change
                m_EnableFlagDelayTime = 0;
                m_EnableFlagTimeout = 0;
            }

            // if user DID change the option then begin the process of showing request, updating, timeout, etc.
            else
            {
                // first update: just show check so there isn't a big pause before feedback occurs
                if (m_EnableFlagDelayTime == 0)
                {
                    m_EnableFlagDelayTime = Time.time + k_EnableFlagDelay;
                }

                // if finished pausing for user to see feedback
                else if (m_EnableFlagDelayTime <= Time.time)
                {
                    if (m_EnableFlagTimeout == 0)
                    {
                        if (m_NewEnabledFlag)
                        {
                            EnableGfIap();
                        }
                        else
                        {
                            DisableGfIap();
                        }

                        // disable for 10 secs OR until option change is complete--whichever comes first
                        m_EnableFlagTimeout = Time.time + k_EnableFlagTimeoutDelay;
                    }

                    // if option WAS set and we timed out or anything else went wrong then fall back to current setting
                    else if (m_EnableFlagTimeout <= Time.time)
                    {
                        m_NewEnabledFlag = GameFoundationSettings.purchasingEnabled;
                        m_EnableFlagDelayTime = 0;
                        m_EnableFlagTimeout = 0;
                    }
                }
            }

            // save serialized object
            serializedObject.ApplyModifiedProperties();

            // process flag update until complete
            if (!isFlagCorrect)
            {
                EditorUtility.SetDirty(target);
            }
        }

        // returns true if iap for game foundation can be enabled based on UNITY_PURCHASING define and purchasing namespace
        static bool CanPurchasingBeEnabled()
        {
#if UNITY_PURCHASING
            return DoesPurchasingPackageExist();
#else
            return false;
#endif
        }

        // simply checks if Purchasing package exists
        static bool DoesPurchasingPackageExist()
        {
            return Tools.NamespaceExists("UnityEngine.Purchasing") &&
                   Tools.TypeExists("UnityEngine.Purchasing.IAppleExtensions") &&
                   Tools.TypeExists("UnityEngine.Purchasing.IGooglePlayStoreExtensions") &&
                   Tools.TypeExists("UnityEngine.Purchasing.ITransactionHistoryExtensions");
        }

        // Adds UNITY_PURCHASING_FOR_GAME_FOUNDATION to all valid project settings
        static void EnableGfIap()
        {
            foreach (var targetOn in k_BuildTargetGroups)
            {
                // get defines for this target, check if gf-iap define NOT listed
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetOn);
                if (!defines.Contains(k_GfIapDefine))
                {
                    // add gf-iap define and set in player settings
                    var splitDefines = new List<string>(defines.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
                    splitDefines.Add(k_GfIapDefine);
                    defines = string.Join(";", splitDefines.ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetOn, defines);
                }
            }
        }
        
        // Simply REMOVES UNITY_PURCHASING_FOR_GAME_FOUNDATION from all project settings, effectively turning off GF IAP
        static void DisableGfIap()
        {
            for (var targetOn = BuildTargetGroup.Unknown; targetOn <= BuildTargetGroup.Lumin; ++targetOn)
            {
                // get defines for this target, check if gf-iap define IS listed
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetOn);
                if (defines.Contains(k_GfIapDefine))
                {
                    // add gf-iap define and set in player settings
                    var splitDefines = new List<string>(defines.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
                    splitDefines.Remove(k_GfIapDefine);
                    defines = string.Join(";", splitDefines.ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetOn, defines);
                }
            }
        }
    }
}
#endif
