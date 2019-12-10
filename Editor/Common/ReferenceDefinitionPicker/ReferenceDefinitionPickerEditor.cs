using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Module for reference definition selection UI.
    /// </summary>
    internal static class ReferenceDefinitionPickerEditor
    {
        internal static void DrawReferenceDefinitionPicker(GenericMenu menu, GameItemDefinition referenceDefinition, GUIContent referenceDefinitionLabel)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(referenceDefinitionLabel);

                if (referenceDefinition == null)
                {
                    EditorGUILayout.LabelField(
                        $"<color=\"{GameFoundationEditorStyles.noValueLabelColor}\">none selected</color>",
                        GameFoundationEditorStyles.richTextLabelStyle);
                }
                else
                {
                    string referenceDefinitionDisplayName = referenceDefinition.id;

                    if (!string.IsNullOrEmpty(referenceDefinition.displayName))
                    {
                        referenceDefinitionDisplayName = referenceDefinition.displayName;
                    }

                    EditorGUILayout.SelectableLabel(referenceDefinitionDisplayName, GUILayout.Height(15f));
                }

                GUI.SetNextControlName("ReferenceDefinitionChooseButton");
                if (GUILayout.Button("Choose", EditorStyles.miniButton, GUILayout.Width(50f)))
                {
                    GUI.FocusControl("ReferenceDefinitionChooseButton");
                    menu.ShowAsContext();
                }
            }
        }
    }
}
