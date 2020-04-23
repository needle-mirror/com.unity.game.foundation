using UnityEngine;
using UnityEngine.UI;
using UnityEngine.GameFoundation.UI;
using UnityEngine.GameFoundation.CatalogManagement;

#if UNITY_EDITOR
namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(InventoryItemView))]
    public class InventoryItemViewEditor : Editor
    {
        private InventoryItemView m_InventoryItemView;
        
        private string[] m_InventoryItemNames = null;
        private string[] m_InventoryItemIds = null;

        private int m_SelectedInventoryItemIndex = 0;

        private UISpriteNameSelector m_ItemSpriteNameSelector;

        private SerializedProperty m_ItemDefinitionId_SerializedProperty = null;
        private SerializedProperty m_IconImageField_SerializedProperty = null;
        private SerializedProperty m_QuantityText_SerializedProperty = null;
        private SerializedProperty m_IconSpriteName_SerializedProperty = null;

        private readonly string[] kExcludedFields = {"m_Script", "m_ItemDefinitionId", "m_IconImageField", "m_QuantityTextField", "m_IconSpriteName"};
        
        private void OnEnable()
        {
            m_InventoryItemView = target as InventoryItemView;
            
            m_ItemDefinitionId_SerializedProperty = serializedObject.FindProperty("m_ItemDefinitionId");
            m_IconImageField_SerializedProperty = serializedObject.FindProperty("m_ItemImage");
            m_QuantityText_SerializedProperty = serializedObject.FindProperty("m_QuantityTextField");
            m_IconSpriteName_SerializedProperty = serializedObject.FindProperty("m_IconSpriteName");

            m_ItemSpriteNameSelector = new UISpriteNameSelector();

            UpdateInventoryItems();
        }

        private void UpdateInventoryItems()
        {
            string selectedItemId = m_ItemDefinitionId_SerializedProperty.stringValue;
            
            var itemDefs = GameFoundationDatabaseSettings.database.inventoryCatalog.GetItems();
            var itemDefCount = itemDefs?.Length ?? 0;

            if (m_InventoryItemNames == null || m_InventoryItemNames.Length != itemDefCount)
            {
                m_InventoryItemNames = new string[itemDefCount];
            }

            if (m_InventoryItemIds == null || m_InventoryItemIds.Length != itemDefCount)
            {
                m_InventoryItemIds = new string[itemDefCount];
            }

            if (itemDefs != null)
            {
                for (int i = 0; i < itemDefCount; i++)
                {
                    var def = itemDefs[i];
                    m_InventoryItemNames[i] = def.displayName;
                    m_InventoryItemIds[i] = def.id;

                    if (selectedItemId == def.id)
                    {
                        m_SelectedInventoryItemIndex = i;
                    }
                }
            }
        }
        
        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();
            
            if (m_InventoryItemNames != null && m_InventoryItemNames.Length > 0)
            {
                var itemDisplayContent = new GUIContent("Item", "The Inventory Item to display.");
                var selectedItemIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedInventoryItemIndex, m_InventoryItemNames);
                ChangeSelectedInventoryItem(selectedItemIndex);
            }
            else
            {
                EditorGUILayout.Popup("Items", m_SelectedInventoryItemIndex, new string[]{"None"});
            }
            
            EditorGUILayout.Space();
            
            var oldItemSpriteName = m_InventoryItemView.iconSpriteName;
            var newItemSpriteName = m_ItemSpriteNameSelector.Draw(oldItemSpriteName, "Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the Inventory item icon.");
            if (newItemSpriteName != oldItemSpriteName)
            {
                m_IconSpriteName_SerializedProperty.stringValue = newItemSpriteName;
                m_InventoryItemView.SetIconSpriteName(newItemSpriteName);
            }

            EditorGUILayout.Space();
            
            var imageDisplayContent = new GUIContent("Item Image Field", "The Image component in witch to display Inventory item icon sprite.");
            var oldImage = m_InventoryItemView.iconImageField;
            var newImage = (Image)EditorGUILayout.ObjectField(imageDisplayContent, oldImage, typeof(Image), true);
            
            if (oldImage != newImage)
            {
                m_InventoryItemView.SetIconImageField(newImage);
                if (m_IconImageField_SerializedProperty != null)
                {
                    m_IconImageField_SerializedProperty.objectReferenceValue = newImage;
                }
            }
            
            var textDisplayContent = new GUIContent("Item Quantity Text Field", "Text component in which to display Inventory item quantity.");
            var oldText = m_InventoryItemView.quantityTextField;
            var newText = (Text)EditorGUILayout.ObjectField(textDisplayContent, oldText, typeof(Text), true);
            
            if (oldText != newText)
            {
                m_InventoryItemView.SetQuantityTextField(newText);
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

        private void ChangeSelectedInventoryItem(int index)
        {
            if (m_SelectedInventoryItemIndex == index || index < 0 || index >= m_InventoryItemIds.Length)
            {
                return;
            }

            m_SelectedInventoryItemIndex = index;

            // Update the serialized value
            m_ItemDefinitionId_SerializedProperty.stringValue = m_InventoryItemIds[index];
            
            // Update Component's state
            m_InventoryItemView.SetItemDefinitionId(m_InventoryItemIds[index]);
        }
    }
}
#endif
