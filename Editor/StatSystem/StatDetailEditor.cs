using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(StatDetailAsset))]
    internal class StatDetailEditor : BaseDetailAssetEditor
    {
        //private SerializedProperty m_SerializedDefaultKeyList;
        //private SerializedProperty m_SerializedDefaultValueList;

        private List<StatEntry> m_StatEntries = new List<StatEntry>();

        private List<StatDefinitionAsset> m_AvailableStats =
            new List<StatDefinitionAsset>();

        private string[] m_AvailableStatDisplayNames;

        // These won't "stick" unless they're static.
        // That unfortunately means that we need to be careful with them
        // since multiple editor instances might try and access the same static field.
        private static int s_SelectedStatIndex;

        private static StatDefinitionAsset s_StatDefinitionJustAdded;

        StatDetailAsset m_TargetDetail;

        private void OnEnable()
        {
            // NOTE: this is a workaround to avoid a problem with Unity asset importer
            // - sometimes targets[0] is null when it shouldn't be
            // - the first two conditions are just a precaution
            if (targets.IsNullOrEmpty())
            {
                return;
            }

            m_TargetDetail = target as StatDetailAsset;

            //m_SerializedDefaultKeyList = serializedObject.FindProperty("m_DefaultKeyList");
            //m_SerializedDefaultValueList = serializedObject.FindProperty("m_DefaultValueList");

            RefreshCache();
        }

        /// <summary>
        /// Combine the lists and cache the merged collection in a static field.
        /// </summary>
        private void RefreshCache()
        {
            var catalog = m_TargetDetail.GetStatCatalog();

            if (catalog == null)
            {
                Debug.LogError("Catalog not found");
                return;
            }

            m_StatEntries.Clear();
            m_AvailableStats.Clear();

            catalog.GetStatDefinitions(m_AvailableStats);

            foreach (var kvp in m_TargetDetail.m_Stats)
            {
                var entry = new StatEntry
                {
                    stat = kvp.Key,
                    value = kvp.Value
                };
                m_StatEntries.Add(entry);
                m_AvailableStats.Remove(kvp.Key);
            }

            // sort the lists by display name
            // (sort the list items descending because that list is going to be iterated in reverse)

            m_AvailableStats.Sort((a, b) => a.displayName.CompareTo(b.displayName));
            m_StatEntries.Sort((a, b) => b.stat.displayName.CompareTo(a.stat.displayName));

            // list of display names (indices matching those in the original list)

            m_AvailableStatDisplayNames = new string[m_AvailableStats.Count + 1];

            m_AvailableStatDisplayNames[0] = "Select Stat";

            for (int i = 0; i < m_AvailableStats.Count; i++)
            {
                m_AvailableStatDisplayNames[i + 1] =
                    m_AvailableStats[i]?.displayName;
            }

            if (s_SelectedStatIndex > m_AvailableStatDisplayNames.Length - 1)
            {
                s_SelectedStatIndex = 0;
            }
        }

        public override void OnInspectorGUI()
        {
            // NOTE: this is a workaround to avoid a problem with Unity asset importer
            // - sometimes targets[0] is null when it shouldn't be
            // - the first two conditions are just a precaution
            if (targets.IsNullOrEmpty())
            {
                return;
            }

            var catalog = m_TargetDetail.GetStatCatalog();
            if (catalog == null)
            {
                EditorGUILayout.HelpBox("No stat catalog found. Open the Game Foundation > Stat window to create one.", MessageType.Warning);

                return;
            }

            serializedObject.Update();

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                // Draw list header
                using (new GUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    GUILayout.Label("Stat", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(130f));
                    GUILayout.Label("Type", GameFoundationEditorStyles.tableViewToolbarTextStyle);
                    GUILayout.Label("Default Value", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(130f));
                    GUILayout.Space(40f); // "x" column
                }

                if (m_StatEntries.Count > 0)
                {
                    StatDefinitionAsset statToDelete = null;

                    foreach (var entry in m_StatEntries)
                    {
                        // draw row: Stat display name | value type | default value | delete button

                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(entry.stat.displayName, GUILayout.Width(140f));

                            GUILayout.Label(entry.stat.statValueType.ToString());


                            string controlName = $"valueField{entry.GetHashCode().ToString()}";
                            GUI.SetNextControlName(controlName);

                            switch (entry.stat.statValueType)
                            {
                                case StatValueType.Int:
                                    {
                                        var newValue = EditorGUILayout.IntField((int)entry.value, GUILayout.Width(130f));
                                        if (newValue != (int)entry.value)
                                        {
                                            m_TargetDetail.Editor_SetStat(entry.stat, newValue);
                                        }
                                        break;
                                    }
                                case StatValueType.Float:
                                    {
                                        var newValue = EditorGUILayout.FloatField((float)entry.value, GUILayout.Width(130f));
                                        if (newValue != (float)entry.value)
                                        {
                                            m_TargetDetail.Editor_SetStat(entry.stat, newValue);
                                        }
                                        break;
                                    }
                            }

                            if (ReferenceEquals(s_StatDefinitionJustAdded, entry.stat))
                            {
                                EditorGUI.FocusTextInControl(controlName);
                                s_StatDefinitionJustAdded = null;
                            }

                            if (GUILayout.Button("X", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(40f)))
                            {
                                statToDelete = entry.stat;
                            }
                        }
                    }

                    // do any actual deletion outside the rendering loop to prevent sync issues
                    if (statToDelete != null)
                    {
                        m_TargetDetail.m_Stats.Remove(statToDelete);
                    }
                }
                else
                {
                    EditorGUILayout.Space();

                    // if the list has one item, that's the 'Select Stat' item and shouldn't be counted
                    if (m_AvailableStatDisplayNames.Length <= 1)
                    {
                        EditorGUILayout.HelpBox(
                            "You need to create stats in the Stat window in order to add stats here.",
                            MessageType.Info);

                        if (GUILayout.Button("Stat Window"))
                        {
                            StatEditorWindow.ShowWindow();
                        }
                    }
                    else
                    {
                        GUILayout.Label("no stats configured", GameFoundationEditorStyles.centeredGrayLabel);
                    }

                    EditorGUILayout.Space();
                }

                // UI for adding a new Stat (if there are any left unused)

                s_StatDefinitionJustAdded = null;

                // horizontal rule

                Rect lineRect1 = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(lineRect1, EditorGUIUtility.isProSkin ? Color.black : Color.gray);

                // if there is only one (or fewer) options in the list, disable this whole section
                // if the list has one item, that's the 'Select Stat' item and shouldn't be counted

                using (new EditorGUI.DisabledScope(m_AvailableStatDisplayNames.Length <= 1))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        // popup with Stat types that are not already in the dict

                        s_SelectedStatIndex = EditorGUILayout.Popup(
                            s_SelectedStatIndex,
                            m_AvailableStatDisplayNames,
                            GUILayout.Width(140f));

                        StatDefinitionAsset selectedStat = null;

                        if (s_SelectedStatIndex > 0)
                        {
                            selectedStat = m_AvailableStats[s_SelectedStatIndex - 1];
                            GUILayout.Label(selectedStat.statValueType.ToString(), GUILayout.Width(130f));
                        }
                        else
                        {
                            GUILayout.Label("", GUILayout.Width(130f));
                        }

                        // placeholder for value
                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledScope(selectedStat == null))
                        {
                            if (GUILayout.Button("Add", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(40f)))
                            {
                                if (selectedStat != null)
                                {
                                    switch (selectedStat.statValueType)
                                    {
                                        case StatValueType.Int:
                                            m_TargetDetail.Editor_SetStat(selectedStat, 0);
                                            break;
                                        case StatValueType.Float:
                                            m_TargetDetail.Editor_SetStat(selectedStat, 0f);
                                            break;
                                    }

                                    s_StatDefinitionJustAdded = selectedStat;
                                }
                                else
                                {
                                    Debug.LogError("selected stat definition is null when trying to add a stat default value");
                                }

                                s_SelectedStatIndex = 0;
                            }
                        }

                        GUI.enabled = true;
                    }
                }

                if (changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();

                    RefreshCache();
                }
            }

            EditorGUILayout.Space();
        }
    }
}
