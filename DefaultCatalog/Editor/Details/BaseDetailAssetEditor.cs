namespace UnityEditor.GameFoundation.DefaultCatalog.Details
{
    /// <summary>
    /// General use editor for details.
    /// </summary>
    public class BaseDetailAssetEditor : Editor
    {
        string[] m_ExcludedFields => new[] { "m_Script", "m_Owner" };

        /// <summary>
        /// This will draw each property on this detail except those that were excluded.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                DrawPropertiesExcluding(serializedObject, m_ExcludedFields);

                if (changeCheck.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
