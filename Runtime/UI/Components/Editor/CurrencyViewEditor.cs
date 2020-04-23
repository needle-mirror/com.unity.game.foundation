using UnityEngine;
using UnityEngine.UI;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.UI;

#if UNITY_EDITOR
namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(CurrencyView))]
    public class CurrencyViewEditor : Editor
    {
        private CurrencyView m_CurrencyView;
        
        private string[] m_CurrencyNames = null;
        private string[] m_CurrencyIds = null;
        
        private int m_SelectedCurrencyIndex = 0;
        
        private UISpriteNameSelector m_ItemSpriteNameSelector;
        
        private SerializedProperty m_CurrencyId_SerializedProperty = null;
        private SerializedProperty m_IconImageField_SerializedProperty = null;
        private SerializedProperty m_QuantityText_SerializedProperty = null;
        private SerializedProperty m_IconSpriteName_SerializedProperty = null;

        private readonly string[] kExcludedFields = {"m_Script", "m_CurrencyId", "m_IconImageField", "m_QuantityTextField", "m_IconSpriteName"};

        private void OnEnable()
        {
            m_CurrencyView = target as CurrencyView;
            
            m_CurrencyId_SerializedProperty = serializedObject.FindProperty("m_CurrencyId");
            m_IconImageField_SerializedProperty = serializedObject.FindProperty("m_IconImageField");
            m_QuantityText_SerializedProperty = serializedObject.FindProperty("m_QuantityTextField");
            m_IconSpriteName_SerializedProperty = serializedObject.FindProperty("m_IconSpriteName");

            m_ItemSpriteNameSelector = new UISpriteNameSelector();

            UpdateCurrencies();
        }

        private void UpdateCurrencies()
        {
            string selectedItemId = m_CurrencyId_SerializedProperty.stringValue;

            var itemDefCount = 0;
            var itemDefs = GameFoundationDatabaseSettings.database.currencyCatalog.GetItems();
            if (itemDefs != null)
            {
                itemDefCount = itemDefs.Length;
            }

            if (m_CurrencyNames == null || m_CurrencyNames.Length != itemDefCount)
            {
                m_CurrencyNames = new string[itemDefCount];
            }

            if (m_CurrencyIds == null || m_CurrencyIds.Length != itemDefCount)
            {
                m_CurrencyIds = new string[itemDefCount];
            }

            if (itemDefs != null)
            {
                for (int i = 0; i < itemDefCount; i++)
                {
                    var def = itemDefs[i];
                    m_CurrencyNames[i] = def.displayName;
                    m_CurrencyIds[i] = def.id;

                    if (selectedItemId == def.id)
                    {
                        m_SelectedCurrencyIndex = i;
                    }
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            var itemDisplayContent = new GUIContent("Currency", "The Currency to display.");
            var selectedItemIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedCurrencyIndex, m_CurrencyNames);
            ChangeSelectedCurrency(selectedItemIndex);
            
            EditorGUILayout.Space();
            
            var oldItemSpriteName = m_CurrencyView.iconSpriteName;
            var newItemSpriteName = m_ItemSpriteNameSelector.Draw(oldItemSpriteName, "Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the Currency icon.");
            if (newItemSpriteName != oldItemSpriteName)
            {
                m_CurrencyView.SetIconSpriteName(newItemSpriteName);
                m_IconSpriteName_SerializedProperty.stringValue = newItemSpriteName;
            }

            EditorGUILayout.Space();
            
            var imageDisplayContent = new GUIContent("Item Image Field", "The Image component in witch to display Currency icon sprite.");
            var oldImage = m_CurrencyView.iconImageField;
            var newImage = (Image)EditorGUILayout.ObjectField(imageDisplayContent, oldImage, typeof(Image), true);
            
            if (oldImage != newImage)
            {
                m_CurrencyView.SetIconImageField(newImage);
                if (m_IconImageField_SerializedProperty != null)
                {
                    m_IconImageField_SerializedProperty.objectReferenceValue = newImage;
                }
            }
            
            var textDisplayContent = new GUIContent("Item Quantity Text Field", "Text component in which to display Currency quantity.");
            var oldText = m_CurrencyView.quantityTextField;
            var newText = (Text)EditorGUILayout.ObjectField(textDisplayContent, oldText, typeof(Text), true);
            
            if (oldText != newText)
            {
                m_CurrencyView.SetQuantityTextField(newText);
                if (m_QuantityText_SerializedProperty != null)
                {
                    m_QuantityText_SerializedProperty.objectReferenceValue = newText;
                }
            }

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
        
        private void ChangeSelectedCurrency(int index)
        {
            if (m_SelectedCurrencyIndex == index || index < 0 || index >= m_CurrencyIds.Length)
            {
                return;
            }

            m_SelectedCurrencyIndex = index;

            // Update the serialized value
            m_CurrencyId_SerializedProperty.stringValue = m_CurrencyIds[index];
            
            // Update Component's state
            m_CurrencyView.SetCurrencyId(m_CurrencyIds[index]);
        }
    }
}
#endif