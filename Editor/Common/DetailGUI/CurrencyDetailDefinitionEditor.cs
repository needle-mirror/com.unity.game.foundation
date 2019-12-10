using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(CurrencyDetailDefinition))]
    internal class CurrencyDetailDefinitionEditor : BaseDetailDefinitionEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                string typesTooltip = "Soft currency is typically earned through normal gameplay without having to purchase with real money. Hard currency is typically purchased with real money.";

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent("Type", typesTooltip), GUILayout.Width(40f));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CurrencyType"), GUIContent.none, GUILayout.Width(100f));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("Sub-Type", GUILayout.Width(70f));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CurrencySubType"), GUIContent.none, GUILayout.Width(100f));
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();

                if (changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
