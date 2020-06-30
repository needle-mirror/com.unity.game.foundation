using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.UI;

namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(StoreView))]
    public class StoreViewEditor : Editor
    {
        StoreView m_StoreView;

        string[] m_StoreNames;
        string[] m_StoreKeys;
        string[] m_TagNames;

        int m_SelectedStoreIndex = -1;
        int m_SelectedTagIndex = -1;

        UISpriteNameSelector m_ItemSpriteNameSelector;
        UISpriteNameSelector m_PriceSpriteNameSelector;

        SerializedProperty m_StoreKey_SerializedProperty;
        SerializedProperty m_TagKey_SerializedProperty;
        SerializedProperty m_ItemIconSpriteName_SerializedProperty;
        SerializedProperty m_PriceIconSpriteName_SerializedProperty;
        SerializedProperty m_NoPriceString_SerializedProperty;
        SerializedProperty m_Interactable_SerializedProperty;

        readonly string[] kExcludedFields =
        {
            "m_Script",
            nameof(StoreView.m_StoreKey),
            nameof(StoreView.m_TagKey),
            nameof(StoreView.m_ItemIconSpriteName),
            nameof(StoreView.m_PriceIconSpriteName),
            nameof(StoreView.m_NoPriceString),
            nameof(StoreView.m_Interactable)
        };

        void OnEnable()
        {
            m_StoreView = target as StoreView;

            m_StoreKey_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_StoreKey));
            m_TagKey_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_TagKey));
            m_ItemIconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_ItemIconSpriteName));
            m_PriceIconSpriteName_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_PriceIconSpriteName));
            m_NoPriceString_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_NoPriceString));
            m_Interactable_SerializedProperty = serializedObject.FindProperty(nameof(m_StoreView.m_Interactable));

            m_PriceSpriteNameSelector = new UISpriteNameSelector();
            m_ItemSpriteNameSelector = new UISpriteNameSelector();

            UpdateNamesAndKeys();
        }

        void UpdateNamesAndKeys()
        {
            UpdateStores();
            UpdateTags();
        }

        void UpdateStores()
        {
            m_SelectedStoreIndex = -1;
            
            string selectedStoreKey = m_StoreKey_SerializedProperty.stringValue;

            var storeCount = 0;
            var storeAssets = GameFoundationDatabaseSettings.database.storeCatalog.GetItems();
            if (storeAssets != null)
            {
                storeCount = storeAssets.Length;
            }

            if (m_StoreNames == null || m_StoreNames.Length != storeCount)
            {
                m_StoreNames = new string[storeCount];
            }

            if (m_StoreKeys == null || m_StoreKeys.Length != storeCount)
            {
                m_StoreKeys = new string[storeCount];
            }

            if (storeAssets != null)
            {
                for (int i = 0; i < storeCount; i++)
                {
                    var storeAsset = storeAssets[i];
                    m_StoreNames[i] = storeAsset.displayName;
                    m_StoreKeys[i] = storeAsset.key;

                    if (!string.IsNullOrEmpty(selectedStoreKey) && selectedStoreKey == storeAsset.key)
                    {
                        m_SelectedStoreIndex = i;
                    }
                }
            }

            if (m_SelectedStoreIndex == -1)
            {
                ChangeSelectedStore(0);
            }
        }

        void UpdateTags()
        {
            m_SelectedTagIndex = -1;
            
            string selectedTagKey = m_TagKey_SerializedProperty.stringValue;

            var tagDefinitions = GameFoundationDatabaseSettings.database.tagCatalog.GetTags();
            int catCount = tagDefinitions?.Length ?? 0;

            m_TagNames = new string[catCount + 1];
            m_TagNames[0] = "<All>";

            for (int i = 0; i < catCount; i++)
            {
                var def = tagDefinitions[i];
                m_TagNames[i + 1] = def.key;

                if (selectedTagKey == def.key)
                {
                    m_SelectedTagIndex = i + 1;
                }
            }

            if (m_SelectedTagIndex == -1)
            {
                ChangeSelectedTag(0);
            }
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();
            
            var storeDisplayContent = new GUIContent("Store", "The Store to display in this view.");
            if (m_StoreNames != null && m_StoreNames.Length > 0)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var storeIndex =  EditorGUILayout.Popup(storeDisplayContent, m_SelectedStoreIndex, m_StoreNames);
                    if (check.changed)
                    {
                        ChangeSelectedStore(storeIndex);
                    }
                }
            }
            else
            {
                EditorGUILayout.Popup(storeDisplayContent, 0, new[] {"None"});
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var tagDisplayContent = new GUIContent("Tag", "If a specific tag is selected, list of Transaction Items rendered will be filtered to only ones in that tag.");
                var selectedTagIndex = EditorGUILayout.Popup(tagDisplayContent, m_SelectedTagIndex, m_TagNames);
                if (check.changed)
                {
                    ChangeSelectedTag(selectedTagIndex);
                }
            }
            
            EditorGUILayout.Space();
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var noPriceStringContent = new GUIContent("No Price String", "String to display on Purchase Button when there is no cost defined in the Transaction Item.");
                var noPriceString = EditorGUILayout.TextField(noPriceStringContent, m_StoreView.noPriceString);
                if (check.changed)
                {
                    m_StoreView.SetNoPriceString(noPriceString);
                    m_NoPriceString_SerializedProperty.stringValue = noPriceString;
                }
            }
            
            EditorGUILayout.Space();
            
            var itemIconName = m_ItemSpriteNameSelector.Draw(m_StoreView.itemIconSpriteName,"Item Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the item icon.");
            if (itemIconName != m_StoreView.itemIconSpriteName)
            {
                m_StoreView.SetItemIconSpriteName(itemIconName);
                m_ItemIconSpriteName_SerializedProperty.stringValue = itemIconName;
            }
            var priceIconName = m_PriceSpriteNameSelector.Draw(m_StoreView.priceIconSpriteName, "Price Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the price icon.");
            if (priceIconName != m_StoreView.priceIconSpriteName)
            {
                m_StoreView.SetNoPriceString(priceIconName);
                m_PriceIconSpriteName_SerializedProperty.stringValue = priceIconName;
            }

            EditorGUILayout.Space();
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var interactableContent = new GUIContent("Interactable", "Sets the Store View's interactable state.");
                var interactable = EditorGUILayout.Toggle(interactableContent, m_StoreView.interactable);
                
                if (check.changed)
                {
                    m_StoreView.interactable = m_Interactable_SerializedProperty.boolValue = interactable;    
                }
            }

            EditorGUILayout.Space();

            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        string GetStoreKey(int index)
        {
            if (m_StoreKeys == null || m_StoreKeys.Length <= index || index < -1)
            {
                return null;
            }

            return m_StoreKeys[index];
        }
        
        string GetTagKey(int index)
        {
            if (m_TagNames == null || m_TagNames.Length <= index || index < -1)
            {
                return null;
            }
            
            return index == 0 ? null : m_TagNames[index];
        }

        void ChangeSelectedStore(int index)
        {
            var key = GetStoreKey(index);
            if (key == null)
            {
                return;
            }
            
            m_SelectedStoreIndex = index;

            // Update the serialized value
            m_StoreKey_SerializedProperty.stringValue = key;
            m_StoreView.SetStoreKey(key);
            
            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        void ChangeSelectedTag(int index)
        {
            var key = GetTagKey(index);

            m_SelectedTagIndex = key == null ? 0 : index;
            
            // Update the serialized value
            m_TagKey_SerializedProperty.stringValue = key;
            m_StoreView.SetTagKey(key);
            
            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
