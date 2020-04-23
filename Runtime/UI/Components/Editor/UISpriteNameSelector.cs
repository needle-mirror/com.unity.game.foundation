using UnityEngine;

#if UNITY_EDITOR
namespace UnityEditor.GameFoundation.UI
{
    public class UISpriteNameSelector
    {
        private bool m_IsChanging = false;
        private string m_SpriteName = null;

        public string Draw(string currentSpriteName, string fieldName, string fieldTooltip = null)
        {
            var spriteNameDisplayContent = new GUIContent(fieldName, fieldTooltip);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (!m_IsChanging)
                {
                    GUIContent currentSpriteNameContent = new GUIContent(currentSpriteName);
                    EditorGUILayout.LabelField(spriteNameDisplayContent, currentSpriteNameContent);

                    if (GUILayout.Button("Change", EditorStyles.miniButton, GUILayout.Width(50f)))
                    {
                        m_SpriteName = currentSpriteName;
                        m_IsChanging = true;
                    }
                }
                else
                {
                    m_SpriteName = EditorGUILayout.TextField(spriteNameDisplayContent, m_SpriteName);

                    if (GUILayout.Button("Set", EditorStyles.miniButton, GUILayout.Width(50f)))
                    {
                        m_IsChanging = false;
                        return m_SpriteName;
                    }
                }
                
                return currentSpriteName;
            }
        }
    }
}
#endif
