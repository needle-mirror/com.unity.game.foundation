using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(AudioClipAssetsDetailDefinition))]
    internal class AudioClipAssetsDetailDefinitionEditor : BaseDetailDefinitionEditor
    {
        private SerializedProperty m_Names_SerializedProperty;
        private SerializedProperty m_Values_SerializedProperty;

        private AudioClipAssetsDetailDefinition m_TargetDefinition;
        private readonly List<AssetsDetailListItem> m_ListItems = new List<AssetsDetailListItem>();

        private void OnEnable()
        {
            // NOTE: this is a workaround to avoid a problem with Unity asset importer
            // - sometimes targets[0] is null when it shouldn't be
            // - the first two conditions are just a precaution
            if (targets.IsNullOrEmpty())
            {
                return;
            }

            m_TargetDefinition = targets[0] as AudioClipAssetsDetailDefinition;

            if (m_TargetDefinition == null)
            {
                return;
            }

            m_Names_SerializedProperty = serializedObject.FindProperty("m_Names");
            m_Values_SerializedProperty = serializedObject.FindProperty("m_Values");

            RefreshCache();
        }

        private void RefreshCache()
        {
            m_ListItems.Clear();

            if (m_TargetDefinition == null)
            {
                return;
            }

            // NOTE: this is a workaround to avoid a problem with Unity asset importer
            // - sometimes targets[0] is null when it shouldn't be
            // - the first two conditions are just a precaution
            if (targets.IsNullOrEmpty())
            {
                return;
            }

            if (m_Names_SerializedProperty.arraySize != m_Values_SerializedProperty.arraySize)
            {
                return;
            }

            for (var i = 0; i < m_Names_SerializedProperty.arraySize; i++)
            {
                m_ListItems.Add(
                    new AssetsDetailListItem(
                        i,
                        m_Names_SerializedProperty.GetArrayElementAtIndex(i),
                        m_Values_SerializedProperty.GetArrayElementAtIndex(i)));
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

            serializedObject.Update();

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                using (new GUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    GUILayout.Label("Name", GameFoundationEditorStyles.tableViewToolbarTextStyle);
                    GUILayout.Label("Audio Clip", GameFoundationEditorStyles.tableViewToolbarTextStyle);
                    GUILayout.Space(40f); // "x" column
                }

                if (m_ListItems.Count > 0)
                {
                    var indexToDelete = -1;

                    for (var i = m_ListItems.Count - 1; i >= 0; i--)
                    {
                        // draw row

                        using (new GUILayout.HorizontalScope())
                        {
                            // delayed text field will make for less annoying validation
                            m_ListItems[i].nameProperty.stringValue =
                                EditorGUILayout.DelayedTextField(m_ListItems[i].nameProperty.stringValue);

                            EditorGUILayout.PropertyField(m_ListItems[i].valueProperty, GUIContent.none);

                            if (
                                GUILayout.Button(
                                    "X",
                                    GameFoundationEditorStyles.tableViewButtonStyle,
                                    GUILayout.Width(40f)))
                            {
                                indexToDelete = m_ListItems[i].indexInOriginalList;
                            }
                        }
                    }

                    // do any actual deletion outside the rendering loop to prevent sync issues
                    if (indexToDelete >= 0)
                    {
                        m_Names_SerializedProperty.DeleteArrayElementAtIndex(indexToDelete);

                        // when array elements are object references,
                        // if it's not null, you need an extra delete call to make it null first,
                        // then the second delete actually removes the array element
                        if (m_Values_SerializedProperty.GetArrayElementAtIndex(indexToDelete).objectReferenceValue != null)
                        {
                            m_Values_SerializedProperty.DeleteArrayElementAtIndex(indexToDelete);
                        }
                        m_Values_SerializedProperty.DeleteArrayElementAtIndex(indexToDelete);
                    }                    
                }
                else
                {
                    EditorGUILayout.Space();
                    GUILayout.Label("no audio clips selected", GameFoundationEditorStyles.centeredGrayLabel);
                    EditorGUILayout.Space();
                }

                if (GUILayout.Button("+"))
                {
                    m_Names_SerializedProperty.InsertArrayElementAtIndex(0);
                    var newNameSerializedProperty = m_Names_SerializedProperty.GetArrayElementAtIndex(0);
                    newNameSerializedProperty.stringValue = AudioClipAssetsDetailDefinition.k_NewAudioClipName;

                    m_Values_SerializedProperty.InsertArrayElementAtIndex(0);
                    SerializedProperty newValueSerializedProperty = m_Values_SerializedProperty.GetArrayElementAtIndex(0);
                    newValueSerializedProperty.objectReferenceValue = null;
                }

                if (changeCheck.changed)
                {
                    for (var i = 0; i < m_Names_SerializedProperty.arraySize; i++)
                    {
                        if (string.IsNullOrEmpty(m_Names_SerializedProperty.GetArrayElementAtIndex(i).stringValue))
                        {
                            m_Names_SerializedProperty.GetArrayElementAtIndex(i).stringValue
                                = AudioClipAssetsDetailDefinition.k_NewAudioClipName;
                        }
                    }

                    serializedObject.ApplyModifiedProperties();

                    RefreshCache();
                }
            }

            EditorGUILayout.Space();
        }
    }
}
