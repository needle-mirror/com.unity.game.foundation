using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.UI;
using UnityEngine.UI;

namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(InventoryItemHudView))]
    public class InventoryItemHudViewEditor : Editor
    {
        InventoryItemHudView m_InventoryItemHudView;

        string[] m_InventoryItemNames;
        string[] m_InventoryItemKeys;

        int m_SelectedInventoryItemIndex = -1;

        UISpriteNameSelector m_ItemSpriteNameSelector;

        SerializedProperty m_ItemDefinitionKey_SerializedProperty;
        SerializedProperty m_IconImageField_SerializedProperty;
        SerializedProperty m_QuantityText_SerializedProperty;
        SerializedProperty m_IconSpriteName_SerializedProperty;
        SerializedProperty m_ShowGameObjectEditorFields_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(InventoryItemHudView.m_ItemDefinitionKey),
            nameof(InventoryItemHudView.m_IconImageField),
            nameof(InventoryItemHudView.m_QuantityTextField),
            nameof(InventoryItemHudView.m_IconSpriteName),
            nameof(InventoryItemHudView.showGameObjectEditorFields)
        };

        void OnEnable()
        {
            m_InventoryItemHudView = target as InventoryItemHudView;

            m_ItemDefinitionKey_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.m_ItemDefinitionKey));
            m_IconImageField_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.m_IconImageField));
            m_QuantityText_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.m_QuantityTextField));
            m_IconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.m_IconSpriteName));
            m_ShowGameObjectEditorFields_SerializedProperty = serializedObject.FindProperty(nameof(m_InventoryItemHudView.showGameObjectEditorFields));

            m_ItemSpriteNameSelector = new UISpriteNameSelector();

            UpdateInventoryItems();
            
            // To update the content when the GameObject is selected
            if (!Application.isPlaying)
            {
                m_InventoryItemHudView.UpdateContent();
            }
        }

        void UpdateInventoryItems()
        {
            m_SelectedInventoryItemIndex = -1;
             
            string selectedItemKey = m_ItemDefinitionKey_SerializedProperty.stringValue;

            var itemDefinitionAssets = GameFoundationDatabaseSettings.database.inventoryCatalog.GetItems();
            var itemCounts = itemDefinitionAssets?.Length ?? 0;

            if (m_InventoryItemNames == null || m_InventoryItemNames.Length != itemCounts)
            {
                m_InventoryItemNames = new string[itemCounts];
            }

            if (m_InventoryItemKeys == null || m_InventoryItemKeys.Length != itemCounts)
            {
                m_InventoryItemKeys = new string[itemCounts];
            }

            if (itemDefinitionAssets != null)
            {
                for (int i = 0; i < itemCounts; i++)
                {
                    var definitionAsset = itemDefinitionAssets[i];
                    m_InventoryItemNames[i] = definitionAsset.displayName;
                    m_InventoryItemKeys[i] = definitionAsset.key;

                    if (selectedItemKey == definitionAsset.key)
                    {
                        m_SelectedInventoryItemIndex = i;
                    }
                }
            }
            
            if (m_SelectedInventoryItemIndex == -1)
            {
                ChangeSelectedInventoryItem(0);
            }
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            var itemDisplayContent = new GUIContent("Item", "The Inventory Item to display.");
            if (m_InventoryItemNames != null && m_InventoryItemNames.Length > 0)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var inventoryItemIndex = EditorGUILayout.Popup(itemDisplayContent, m_SelectedInventoryItemIndex, m_InventoryItemNames);
                    if (check.changed)
                    {
                        ChangeSelectedInventoryItem(inventoryItemIndex);
                    }
                }
            }
            else
            {
                EditorGUILayout.Popup(itemDisplayContent, 0, new[] {"None"});
            }

            EditorGUILayout.Space();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var itemIconName = m_ItemSpriteNameSelector.Draw(m_InventoryItemHudView.iconSpriteName, "Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the Inventory item icon.");
                
                if (check.changed)
                {
                    m_InventoryItemHudView.SetIconSpriteName(itemIconName);
                    m_IconSpriteName_SerializedProperty.stringValue = itemIconName;
                }
            }
            
            EditorGUILayout.Space();
            
            m_ShowGameObjectEditorFields_SerializedProperty.boolValue = EditorGUILayout.Foldout(m_ShowGameObjectEditorFields_SerializedProperty.boolValue, "GameObject Fields");
            if (m_ShowGameObjectEditorFields_SerializedProperty.boolValue)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var itemImageFieldContent = new GUIContent("Item Image",
                        "The Image component in witch to display Inventory item icon sprite.");
                    var itemImageField = (Image) EditorGUILayout.ObjectField(itemImageFieldContent,
                        m_InventoryItemHudView.iconImageField, typeof(Image), true);

                    if (check.changed)
                    {
                        m_InventoryItemHudView.SetIconImageField(itemImageField);
                        if (m_IconImageField_SerializedProperty != null)
                        {
                            m_IconImageField_SerializedProperty.objectReferenceValue = itemImageField;
                        }
                    }
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var itemQuantityFieldContent = new GUIContent("Item Quantity Text",
                        "Text component in which to display Inventory item quantity.");
                    var itemQuantityField = (Text) EditorGUILayout.ObjectField(itemQuantityFieldContent,
                        m_InventoryItemHudView.quantityTextField, typeof(Text), true);

                    if (check.changed)
                    {
                        m_InventoryItemHudView.SetQuantityTextField(itemQuantityField);
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
        
        string GetInventoryItemKey(int index)
        {
            if (m_InventoryItemKeys == null || index < 0 || index >= m_InventoryItemKeys.Length)
            {
                return null;
            }

            return m_InventoryItemKeys[index];
        }

        void ChangeSelectedInventoryItem(int index)
        {
            var key = GetInventoryItemKey(index);
            if (key == null)
            {
                return;
            }

            m_SelectedInventoryItemIndex = index;

            // Update the serialized value
            m_ItemDefinitionKey_SerializedProperty.stringValue = key;

            // Update Component's state
            m_InventoryItemHudView.SetItemDefinitionKey(key);
            
            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
