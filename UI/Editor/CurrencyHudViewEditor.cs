using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.UI;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(CurrencyHudView))]
    public class CurrencyHudViewEditor : Editor
    {
        CurrencyHudView m_CurrencyHudView;

        string[] m_CurrencyNames;
        string[] m_CurrencyKeys;

        int m_SelectedCurrencyIndex = -1;

        UISpriteNameSelector m_ItemSpriteNameSelector;

        SerializedProperty m_CurrencyKey_SerializedProperty;
        SerializedProperty m_IconImageField_SerializedProperty;
        SerializedProperty m_QuantityText_SerializedProperty;
        SerializedProperty m_IconSpriteName_SerializedProperty;
        SerializedProperty m_ShowGameObjectEditorFields_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(CurrencyHudView.m_CurrencyKey),
            nameof(CurrencyHudView.m_IconImageField),
            nameof(CurrencyHudView.m_QuantityTextField),
            nameof(CurrencyHudView.m_IconSpriteName),
            nameof(CurrencyHudView.showGameObjectEditorFields)
        };

        void OnEnable()
        {
            m_CurrencyHudView = target as CurrencyHudView;

            m_CurrencyKey_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.m_CurrencyKey));
            m_IconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.m_IconImageField));
            m_QuantityText_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.m_QuantityTextField));
            m_IconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.m_IconSpriteName));
            m_ShowGameObjectEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_CurrencyHudView.showGameObjectEditorFields));

            m_ItemSpriteNameSelector = new UISpriteNameSelector();

            UpdateCurrencies();
            
            // To update the content when the GameObject is selected
            m_CurrencyHudView.UpdateContent();
        }

        void UpdateCurrencies()
        {
            m_SelectedCurrencyIndex = -1;
            
            string selectedCurrencyKey = m_CurrencyKey_SerializedProperty.stringValue;

            var currencyCount = 0;
            var currencyAssets = GameFoundationDatabaseSettings.database.currencyCatalog.GetItems();
            if (currencyAssets != null)
            {
                currencyCount = currencyAssets.Length;
            }

            if (m_CurrencyNames == null || m_CurrencyNames.Length != currencyCount)
            {
                m_CurrencyNames = new string[currencyCount];
            }

            if (m_CurrencyKeys == null || m_CurrencyKeys.Length != currencyCount)
            {
                m_CurrencyKeys = new string[currencyCount];
            }

            if (currencyAssets != null)
            {
                for (int i = 0; i < currencyCount; i++)
                {
                    var currencyAsset = currencyAssets[i];
                    m_CurrencyNames[i] = currencyAsset.displayName;
                    m_CurrencyKeys[i] = currencyAsset.key;

                    if (selectedCurrencyKey == currencyAsset.key)
                    {
                        m_SelectedCurrencyIndex = i;
                    }
                }
            }
            
            if (m_SelectedCurrencyIndex == -1)
            {
                ChangeSelectedCurrency(0);
            }
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();
            
            var itemDisplayContent = new GUIContent("Currency", "The Currency to display.");
            if (m_CurrencyNames != null && m_CurrencyNames.Length > 0)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                { 
                    var currencyIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedCurrencyIndex, m_CurrencyNames);
                    if (check.changed)
                    {
                        ChangeSelectedCurrency(currencyIndex);
                    }
                }
            }
            else
            {
                EditorGUILayout.Popup(itemDisplayContent, 0, new[] { "None" });
            }

            EditorGUILayout.Space();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var itemIconName = m_ItemSpriteNameSelector.Draw(m_CurrencyHudView.iconSpriteName, "Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the Currency icon.");

                if (check.changed)
                {
                    m_CurrencyHudView.SetIconSpriteName(itemIconName);
                    m_IconSpriteName_SerializedProperty.stringValue = itemIconName;
                }
            }

            EditorGUILayout.Space();
            
            m_ShowGameObjectEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowGameObjectEditorFields_SerializedProperty.boolValue, "GameObject Fields");
            if (m_ShowGameObjectEditorFields_SerializedProperty.boolValue)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var itemImageFieldContent = new GUIContent("Item Image", "The Image component in witch to display Currency icon sprite.");
                    var itemImageField = (Image) EditorGUILayout.ObjectField(itemImageFieldContent, m_CurrencyHudView.iconImageField, typeof(Image), true);

                    if (check.changed)
                    {
                        m_CurrencyHudView.SetIconImageField(itemImageField);
                        if (m_IconImageField_SerializedProperty != null)
                        {
                            m_IconImageField_SerializedProperty.objectReferenceValue = itemImageField;
                        }
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var itemQuantityFieldContent = new GUIContent("Item Quantity Text", "Text component in which to display Currency quantity.");
                    var itemQuantityField = (Text) EditorGUILayout.ObjectField(itemQuantityFieldContent, m_CurrencyHudView.quantityTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_CurrencyHudView.SetQuantityTextField(itemQuantityField);
                        if (m_QuantityText_SerializedProperty != null)
                        {
                            m_QuantityText_SerializedProperty.objectReferenceValue = itemQuantityField;
                        }
                    }
                }
            }

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        string GetCurrencyKey(int index)
        {
            if (m_CurrencyKeys == null || index < 0 || index >= m_CurrencyKeys.Length)
            {
                return null;
            }

            return m_CurrencyKeys[index];
        }

        void ChangeSelectedCurrency(int index)
        {
            var key = GetCurrencyKey(index);
            if (key == null)
            {
                return;
            }

            m_SelectedCurrencyIndex = index;

            // Update the serialized value
            m_CurrencyKey_SerializedProperty.stringValue = key;

            // Update Component's state
            m_CurrencyHudView.SetCurrencyKey(key);
            
            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
