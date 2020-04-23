using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(AnalyticsDetailAsset))]
    internal class AnalyticsDetailAssetEditor : BaseDetailAssetEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("This detail automatically tracks Analytics data for any GameItem it is attached to.");
        }
    }
}
