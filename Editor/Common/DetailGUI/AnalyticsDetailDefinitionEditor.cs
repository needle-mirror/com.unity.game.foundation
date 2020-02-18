using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(AnalyticsDetailDefinition))]
    internal class AnalyticsDetailDefinitionEditor : BaseDetailDefinitionEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("This detail automatically tracks Analytics data for any GameItem it is attached to.");
        }
    }
}
