using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(SpriteAssetsDetailDefinition))]
    internal class SpriteAssetsDetailDefinitionEditor : BaseDetailDefinitionEditor
    {
        private SerializedProperty m_Keys_SerializedProperty;
        private SerializedProperty m_Values_SerializedProperty;

        private SpriteAssetsDetailDefinition m_TargetDefinition;
        private List<AssetsDetailListItem> m_ListItems = new List<AssetsDetailListItem>();

        private void OnEnable()
        {
            // NOTE: this is a workaround to avoid a problem with Unity asset importer
            // - sometimes targets[0] is null when it shouldn't be
            // - the first two conditions are just a precaution
            if (targets.IsNullOrEmpty())
            {
                return;
            }

            m_TargetDefinition = targets[0] as SpriteAssetsDetailDefinition;

            if (m_TargetDefinition == null)
            {
                return;
            }

            m_Keys_SerializedProperty = serializedObject.FindProperty("m_Keys");
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

            if (m_Keys_SerializedProperty.arraySize != m_Values_SerializedProperty.arraySize)
            {
                return;
            }

            for (int i = 0; i < m_Keys_SerializedProperty.arraySize; i++)
            {
                m_ListItems.Add(
                    new AssetsDetailListItem(
                        i,
                        m_Keys_SerializedProperty.GetArrayElementAtIndex(i),
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
                    GUILayout.Label("Key", GameFoundationEditorStyles.tableViewToolbarTextStyle);
                    GUILayout.Label("Sprite", GameFoundationEditorStyles.tableViewToolbarTextStyle);
                    GUILayout.Space(40f); // "x" column
                }

                if (m_ListItems.Count > 0)
                {
                    int indexToDelete = -1;

                    for (int i = m_ListItems.Count - 1; i >= 0; i--)
                    {
                        // draw row

                        using (new GUILayout.HorizontalScope())
                        {
                            // delayed text field will make for less annoying validation
                            m_ListItems[i].keyProperty.stringValue =
                                EditorGUILayout.DelayedTextField(m_ListItems[i].keyProperty.stringValue);

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
                        m_Keys_SerializedProperty.DeleteArrayElementAtIndex(indexToDelete);

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
                    GUILayout.Label("no sprites selected", GameFoundationEditorStyles.centeredGrayLabel);
                    EditorGUILayout.Space();
                }

                if (GUILayout.Button("+"))
                {
                    m_Keys_SerializedProperty.InsertArrayElementAtIndex(0);
                    SerializedProperty newKeySerializedProperty = m_Keys_SerializedProperty.GetArrayElementAtIndex(0);
                    newKeySerializedProperty.stringValue = m_TargetDefinition.GetNextValidKey();

                    m_Values_SerializedProperty.InsertArrayElementAtIndex(0);
                    SerializedProperty newValueSerializedProperty = m_Values_SerializedProperty.GetArrayElementAtIndex(0);
                    newValueSerializedProperty.objectReferenceValue = null;
                }

                if (changeCheck.changed)
                {
                    // validate all keys here, and cancel the changes if there is a duplicate key
                    // this can't be done by the target object,
                    // because the target object doesn't know about the new changes yet

                    bool detectedEmptyKey = false;
                    bool detectedDuplicateKey = false;
                    List<string> validatedKeys = new List<string>();

                    for (int i = 0; i < m_Keys_SerializedProperty.arraySize; i++)
                    {
                        string key = m_Keys_SerializedProperty.GetArrayElementAtIndex(i).stringValue;

                        if (string.IsNullOrEmpty(key))
                        {
                            detectedEmptyKey = true;
                            break;
                        }

                        if (validatedKeys.Contains(key))
                        {
                            detectedDuplicateKey = true;
                            break;
                        }

                        validatedKeys.Add(key);
                    }

                    if (detectedEmptyKey)
                    {
                        EditorUtility.DisplayDialog("Empty Key", "Empty keys are not allowed. Please enter a different key.", "OK");
                    }
                    else if (detectedDuplicateKey)
                    {
                        EditorUtility.DisplayDialog("Non-Unique Key", "The key you entered is already in use in this list. Please enter a different key.", "OK");
                    }
                    else
                    {
                        serializedObject.ApplyModifiedProperties();
                    }

                    RefreshCache();
                }
            }

            EditorGUILayout.Space();
        }
    }
}
