using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// A GUI control for adding/removing/editing details on anything that inherits GameItemDefinition.
    /// </summary>
    internal static class DetailEditorGUI
    {
        private static readonly GUIContent s_DetailDefinitionsLabel = new GUIContent("Detail Definitions", "Detail definitions extend the information that is attached to a given definition.");
        private static readonly GUIContent s_InheritedDetailDefinitionsLabel = new GUIContent("Inherited Detail Definitions", "Detail definitions that are attached to the Reference Definition are inherited by the item. They extend the information that is attached to the current item, but can be overridden by attaching the same type of detail to the current item.");

        static BaseDetailAsset m_detailDefinitionToRemove;

        /// <summary>
        /// Uses Unity GUI and GUILayout calls to draw a detail manager in a custom editor window.
        /// </summary>
        /// <param name="catalogItem">The GameItemDefinition for which the Detail definitions are to be managed.</param>
        public static void DrawDetailView(CatalogItemAsset catalogItem)
        {
            EditorGUILayout.LabelField(s_DetailDefinitionsLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var details = catalogItem.GetDetails();

                if (details != null)
                {
                    // loop through details and render their property drawers
                    var detailCount = details.Length;
                    if (detailCount <= 0)
                    {
                        GUILayout.Label("no details attached", GameFoundationEditorStyles.centeredGrayLabel);
                    }
                    else
                    {
                        var i = 0;

                        foreach (var detail in details)
                        {
                            var detailDisplayName = detail.DisplayName();

                            var detailLabel = new GUIContent(detailDisplayName, detail.TooltipMessage());
                            GUILayout.Label(detailLabel, EditorStyles.boldLabel);

                            var removeButtonRect = GUILayoutUtility.GetLastRect();
                            removeButtonRect.x = removeButtonRect.x + removeButtonRect.width - 20f;
                            removeButtonRect.width = 20f;
                            if (GUI.Button(removeButtonRect, "<b>X</b>", GameFoundationEditorStyles.deleteButtonStyle))
                            {
                                if (EditorUtility.DisplayDialog("Are you sure?", "Do you want to delete " + detail.DisplayName() + "?", "Yes", "No"))
                                {
                                    m_detailDefinitionToRemove = detail;
                                }
                            }

                            var detailDefinitionEditor = Editor.CreateEditor(detail);

                            if (detailDefinitionEditor != null)
                            {
                                if (System.Attribute.GetCustomAttribute(
                                    detail.GetType(),
                                    typeof(System.ObsoleteAttribute))
                                    is System.ObsoleteAttribute obsoleteAttribute)
                                {
                                    EditorGUILayout.HelpBox($"Warning: {detail.GetType().Name} is obsolete. {obsoleteAttribute.Message}", MessageType.Warning);
                                }

                                detailDefinitionEditor.OnInspectorGUI();
                            }

                            i++;

                            if (i < detailCount)
                            {
                                DrawDetailSeparator();
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Label("Error: The Detail Definitions list is null!");
                }
            }

            if (m_detailDefinitionToRemove != null)
            {
                catalogItem.Editor_RemoveDetail(m_detailDefinitionToRemove);
                if (EditorUtility.IsPersistent(m_detailDefinitionToRemove))
                {
                    AssetDatabase.RemoveObjectFromAsset(m_detailDefinitionToRemove);
                    Object.DestroyImmediate(m_detailDefinitionToRemove, true);
                }
                m_detailDefinitionToRemove = null;
            }

            EditorGUILayout.Space();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add Detail", GUILayout.Width(200f)))
                {
                    DetailHelper.RefreshTypeDict();

                    // keep a list of the names of all detail types already added to this GameItemDefinition
                    var alreadyAddedDetails =
                        new List<BaseDetailAsset>(catalogItem.GetDetails());

                    var alreadyAddedDetailTypesNames = new List<string>();

                    foreach(var detail in alreadyAddedDetails)
                    {
                        alreadyAddedDetailTypesNames.Add(detail.DisplayName());
                    }

                    var newDetailTypesAvailable = false;
                    var customDetailTypesAvailable = false;

                    var detailChoicesMenu = new GenericMenu();

                    foreach (var detailInfo in DetailHelper.defaultDetailInfo)
                    {
                        if (alreadyAddedDetailTypesNames.Contains(detailInfo.Key))
                        {
                            continue;
                        }

                        newDetailTypesAvailable = true;

                        detailChoicesMenu.AddItem(
                            new GUIContent(detailInfo.Key),
                            false,
                            () =>
                            {
                                // create new detail definition
                                var newDetailDefinition = ScriptableObject.CreateInstance(detailInfo.Value) as BaseDetailAsset;

                                // add to GameItemDefinition
                                catalogItem.Editor_AddDetail(newDetailDefinition);

                                AssetDatabase.AddObjectToAsset(newDetailDefinition, catalogItem);
                            });
                    }

                    // Need to do a check first to determine if we should display the separator.
                    foreach (var detailDefinitionInfo in DetailHelper.customDetailDefinitionInfo)
                    {
                        if (alreadyAddedDetailTypesNames.Contains(detailDefinitionInfo.Key))
                        {
                            continue;
                        }
                        customDetailTypesAvailable = true;
                        break;
                    }

                    if (customDetailTypesAvailable)
                    {
                        detailChoicesMenu.AddSeparator("");

                        foreach (var detailInfo in DetailHelper.customDetailDefinitionInfo)
                        {
                            if (!alreadyAddedDetailTypesNames.Contains(detailInfo.Key))
                            {
                                detailChoicesMenu.AddItem(
                                    new GUIContent(detailInfo.Key),
                                    false,
                                    () =>
                                    {
                                        // create new detail definition
                                        var newDetail = ScriptableObject.CreateInstance(detailInfo.Value) as BaseDetailAsset;

                                        // add to GameItemDefinition
                                        catalogItem.Editor_AddDetail(newDetail);
                                    });
                            }
                        }
                    }

                    if (!newDetailTypesAvailable && !customDetailTypesAvailable)
                    {
                        detailChoicesMenu.AddDisabledItem(new GUIContent("all details already added"));
                    }

                    detailChoicesMenu.ShowAsContext();
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space();
        }

        private static void DrawDetailSeparator()
        {
            GUILayout.Space(5f);
            var lineRect1 = EditorGUILayout.GetControlRect(false, 1);
            lineRect1.xMin -= 10;
            lineRect1.xMax += 10;
            EditorGUI.DrawRect(lineRect1, EditorGUIUtility.isProSkin ? Color.black : Color.gray);
        }
    }
}
