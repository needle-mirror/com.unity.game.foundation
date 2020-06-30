using UnityEditor;
using UnityEngine.UI;

namespace UnityEngine.GameFoundation.UI
{
    /// <summary>
    /// Component that manages displaying an icon and text.
    /// </summary>
    public class ImageInfoView : MonoBehaviour
    {
        /// <summary>
        /// The Image component to show an icon.
        /// </summary>
        public Image imageField => m_ImageField;

        [SerializeField]
        internal Image m_ImageField;
        
        /// <summary>
        /// The Text component to show a text.
        /// </summary>
        public Text textField => m_TextField;

        [SerializeField]
        internal Text m_TextField;

        /// <summary>
        /// Sets icon and text that are displayed in this view.
        /// </summary>
        /// <param name="icon">Icon sprite to display</param>
        /// <param name="text">Text to display</param>
        public void SetView(Sprite icon, string text)
        {
            SetIcon(icon);
            SetText(text);
        }

        
        /// <summary>
        /// Sets icon that is displayed in this view.
        /// </summary>
        /// <param name="icon">Icon sprite to display</param>
        public void SetIcon(Sprite icon)
        {
            if (icon == null)
            {
                Debug.LogWarning($"{nameof(ImageInfoView)} - Icon sprite shouldn't be null.");
                return;
            }

            m_ImageField.sprite = icon;
            m_ImageField.SetNativeSize();
#if UNITY_EDITOR
            EditorUtility.SetDirty(m_ImageField);
#endif
        }

        /// <summary>
        /// Set text that is displayed in this view.
        /// </summary>
        /// <param name="text">Text to display</param>
        public void SetText(string text)
        {
            m_TextField.gameObject.SetActive(!string.IsNullOrEmpty(text));

            if (m_TextField.text != text)
            {
                m_TextField.text = text;
#if UNITY_EDITOR
                EditorUtility.SetDirty(m_TextField);
#endif
            }
        }
    }
}
