using UnityEditor;
using UnityEngine.GameFoundation.UI;

namespace UnityEngine.GameFoundation.UI
{
    [CustomEditor(typeof(ImageInfoView))]
    public class ImageInfoViewEditor : Editor
    {
        readonly string[] kExcludedFields =
        {
            "m_Script"
        };
        
        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();
            
            EditorGUILayout.LabelField("GameObject Fields");

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}