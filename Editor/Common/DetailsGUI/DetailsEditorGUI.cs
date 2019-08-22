using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// A GUI control for adding/removing/editing details on anything that inherits GameItemDefinition.
    /// </summary>
    internal static class DetailsEditorGUI
    {
        static BaseDetailsDefinition m_detailsDefinitionToRemove;

        /// <summary>
        /// Uses Unity GUI and GUILayout calls to draw a details manager in a custom editor window.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition for which the Details definitions are to be managed.</param>
        public static void DrawDetailsDetail(GameItemDefinition gameItemDefinition)
        {
            EditorGUILayout.LabelField("Details Definitions", GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var detailsDefinitions = gameItemDefinition.detailsDefinitions;

                if (detailsDefinitions != null)
                {
                    // loop through details and render their property drawers
                    int detailsCount = gameItemDefinition.detailsDefinitionCount;
                    if (detailsCount <= 0)
                    {
                        GUILayout.Label("no details attached", GameFoundationEditorStyles.centeredGrayLabel);
                    }
                    else
                    {
                        int i = 0;

                        foreach (var detailsDefinition in detailsDefinitions)
                        {
                            string detailsDisplayName = detailsDefinition.DisplayName();

                            // if the reference entity has this same details type attached, then this is overriding that
                            if (gameItemDefinition.referenceDefinition != null)
                            {
                                foreach (BaseDetailsDefinition compareDetailsDefinition in gameItemDefinition.referenceDefinition.detailsDefinitions)
                                {
                                    if (compareDetailsDefinition.GetType() == detailsDefinition.GetType())
                                    {
                                        detailsDisplayName += " (overriding)";
                                        break;
                                    }
                                }
                            }

                            GUILayout.Label(detailsDisplayName, EditorStyles.boldLabel);

                            Rect removeButtonRect = GUILayoutUtility.GetLastRect();
                            removeButtonRect.x = removeButtonRect.x + removeButtonRect.width - 20f;
                            removeButtonRect.width = 20f;
                            if (GUI.Button(removeButtonRect, "<b>X</b>", GameFoundationEditorStyles.deleteButtonStyle))
                            {
                                if (EditorUtility.DisplayDialog("Are you sure?", "Do you want to delete " + detailsDefinition.DisplayName() + "?", "Yes", "No"))
                                {
                                    m_detailsDefinitionToRemove = detailsDefinition;
                                }
                            }

                            Editor detailsDefinitionEditor = Editor.CreateEditor(detailsDefinition);

                            if (detailsDefinitionEditor != null)
                            {
                                detailsDefinitionEditor.OnInspectorGUI();
                            }

                            i++;

                            if (i < detailsCount)
                            {
                                DrawDetailsSeparator();
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label("Error: The Details Definitions list is null!");
                }
            }

            if (m_detailsDefinitionToRemove != null)
            {
                gameItemDefinition.RemoveDetailsDefinition(m_detailsDefinitionToRemove);
                m_detailsDefinitionToRemove = null;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Details", GUILayout.Width(200f)))
                {
                    DetailsHelper.RefreshTypeDict();

                    // keep a list of the names of all details types already added to this GameItemDefinition
                    List<BaseDetailsDefinition> alreadyAddedDetails =
                        new List<BaseDetailsDefinition>(gameItemDefinition.detailsDefinitions);

                    List<string> alreadyAddedDetailsTypeNames = new List<string>();

                    foreach (BaseDetailsDefinition detailsDefinition in alreadyAddedDetails)
                    {
                        alreadyAddedDetailsTypeNames.Add(detailsDefinition.DisplayName());
                    }

                    bool noNewDetailsTypesAvailable = true;

                    GenericMenu detailsChoicesMenu = new GenericMenu();

                    foreach (var detailsDefinitionInfo in DetailsHelper.detailsDefinitionInfo)
                    {
                        if (!alreadyAddedDetailsTypeNames.Contains(detailsDefinitionInfo.Key))
                        {
                            noNewDetailsTypesAvailable = false;

                            detailsChoicesMenu.AddItem(
                                new GUIContent(detailsDefinitionInfo.Key),
                                false,
                                () =>
                                {
                                    // create new details definition
                                    var newDetailsDefinition = ScriptableObject.CreateInstance(detailsDefinitionInfo.Value) as BaseDetailsDefinition;

                                    // add to GameItemDefinition
                                    gameItemDefinition.AddDetailsDefinition(newDetailsDefinition);
                                });
                        }
                    }

                    if (noNewDetailsTypesAvailable)
                    {
                        detailsChoicesMenu.AddDisabledItem(new GUIContent("all details already added"));
                    }

                    detailsChoicesMenu.ShowAsContext();
                }

                GUILayout.FlexibleSpace();
            }

            // only show this section if inheritance is possible (not possible with GameItemDefinition, for example)
            if (gameItemDefinition.GetType() != typeof(GameItemDefinition))
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Inherited Detail Definitions", GameFoundationEditorStyles.titleStyle);

                using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                {
                    if (gameItemDefinition.referenceDefinition != null)
                    {
                        var detailsDefinitions = gameItemDefinition.referenceDefinition.detailsDefinitions;

                        if (detailsDefinitions != null)
                        {
                            // loop through components and render their property drawers
                            int componentCount = gameItemDefinition.referenceDefinition.detailsDefinitionCount;
                            if (componentCount <= 0)
                            {
                                GUILayout.Label("no details inherited", GameFoundationEditorStyles.centeredGrayLabel);
                            }
                            else
                            {
                                int i = 0;

                                foreach (var detailsDefinition in detailsDefinitions)
                                {
                                    // if this DetailsDef type is also attached to the gameItemDefinition, then this has been overriden
                                    bool isOverridden = false;
                                    foreach (BaseDetailsDefinition compareDetailsDefinition in gameItemDefinition.detailsDefinitions)
                                    {
                                        if (compareDetailsDefinition.GetType() == detailsDefinition.GetType())
                                        {
                                            isOverridden = true;
                                            break;
                                        }
                                    }

                                    // this should always be disabled at editor time and runtime
                                    using (new EditorGUI.DisabledScope(true))
                                    {
                                        SerializedObject componentDefinitionSerializedObject = new SerializedObject(detailsDefinition);

                                        string detailsDisplayName = detailsDefinition.DisplayName();

                                        if (isOverridden)
                                        {
                                            detailsDisplayName += " (overridden)";
                                        }

                                        GUILayout.Label(detailsDisplayName, EditorStyles.boldLabel);

                                        if (!isOverridden)
                                        {
                                            Editor detailsDefinitionEditor = Editor.CreateEditor(detailsDefinition);

                                            if (detailsDefinitionEditor != null)
                                            {
                                                detailsDefinitionEditor.OnInspectorGUI();
                                            }
                                        }

                                        componentDefinitionSerializedObject.ApplyModifiedProperties();
                                    }

                                    i++;

                                    if (i < componentCount)
                                    {
                                        DrawDetailsSeparator();
                                    }
                                }
                            }
                        }
                        else
                        {
                            GUILayout.Label("Error: The component definitions list is null!");
                        }
                    }
                    else
                    {
                        GUILayout.Label("no reference definition selected", GameFoundationEditorStyles.centeredGrayLabel);
                    }
                }
            }
        }

        static void DrawDetailsSeparator()
        {
            GUILayout.Space(5f);
            Rect lineRect1 = EditorGUILayout.GetControlRect(false, 1);
            lineRect1.xMin -= 10;
            lineRect1.xMax += 10;
            EditorGUI.DrawRect(lineRect1, EditorGUIUtility.isProSkin ? Color.black : Color.gray);
        }
    }
}
