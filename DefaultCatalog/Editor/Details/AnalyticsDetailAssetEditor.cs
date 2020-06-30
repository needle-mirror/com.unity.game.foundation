using UnityEngine.GameFoundation.DefaultCatalog.Details;

namespace UnityEditor.GameFoundation.DefaultCatalog.Details
{
    [CustomEditor(typeof(AnalyticsDetailAsset))]
    class AnalyticsDetailAssetEditor : BaseDetailAssetEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("This detail automatically tracks Analytics data for any GameItem it is attached to.");
        }
    }
}
