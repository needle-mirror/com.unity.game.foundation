using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.UI;

#if UNITY_EDITOR
namespace UnityEditor.GameFoundation.UI
{
    [CustomEditor(typeof(StoreView))]
    public class StoreViewEditor : Editor
    {
        private StoreView m_StoreView;
        
        private string[] m_StoreNames;
        private string[] m_CategoryNames;

        private string[] m_StoreIds;
        private string[] m_CategoryIds;

        private int m_SelectedStoreIndex = -1;
        private int m_SelectedCategoryIndex = -1;

        private UISpriteNameSelector m_ItemSpriteNameSelector;
        private UISpriteNameSelector m_PriceSpriteNameSelector;

        private SerializedProperty m_StoreId_SerializedProperty;
        private SerializedProperty m_CategoryId_SerializedProperty;
        private SerializedProperty m_ItemIconSpriteName_SerializedProperty;
        private SerializedProperty m_PriceIconSpriteName_SerializedProperty;
        private SerializedProperty m_Interactable_SerializedProperty;

        private readonly string[] kExcludedFields = {"m_Script", "m_StoreId", "m_CategoryId", "m_ItemIconSpriteName", "m_PriceIconSpriteName", "m_OnlyVisibleItems", "m_Interactable"};


        private void OnEnable()
        {
            m_StoreView = target as StoreView;
            
            m_StoreId_SerializedProperty = serializedObject.FindProperty("m_StoreId");
            m_CategoryId_SerializedProperty = serializedObject.FindProperty("m_CategoryId");
            m_ItemIconSpriteName_SerializedProperty = serializedObject.FindProperty("m_ItemIconSpriteName");
            m_PriceIconSpriteName_SerializedProperty = serializedObject.FindProperty("m_PriceIconSpriteName");
            m_Interactable_SerializedProperty = serializedObject.FindProperty("m_Interactable");

            m_PriceSpriteNameSelector = new UISpriteNameSelector();
            m_ItemSpriteNameSelector = new UISpriteNameSelector();

            UpdateNamesAndIds();
        }

        private void UpdateNamesAndIds()
        {
            UpdateStores();
            UpdateCategories();
        }

        private void UpdateStores()
        {
            string selectedStoreId = m_StoreId_SerializedProperty.stringValue;

            var storeDefCount = 0;
            var storeDefs = GameFoundationDatabaseSettings.database.storeCatalog.GetItems();
            if (storeDefs != null)
            {
                storeDefCount = storeDefs.Length;
            }

            if (m_StoreNames == null || m_CategoryNames.Length != storeDefCount)
            {
                m_StoreNames = new string[storeDefCount];   
            }

            if (m_StoreIds == null || m_CategoryNames.Length != storeDefCount)
            {
                m_StoreIds = new string[storeDefCount];
            }

            if (storeDefs != null)
            {
                for (int i = 0; i < storeDefCount; i++)
                {
                    var def = storeDefs[i];
                    m_StoreNames[i] = def.displayName;
                    m_StoreIds[i] = def.id;

                    if (!string.IsNullOrEmpty(selectedStoreId) && selectedStoreId == def.id)
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

        private void UpdateCategories()
        {
            string selectedCategoryId = m_CategoryId_SerializedProperty.stringValue;

            var categoryDefinitions = GameFoundationDatabaseSettings.database.transactionCatalog.GetCategories();
            int catCount = categoryDefinitions?.Length ?? 0;

            m_CategoryNames = new string[catCount + 1];
            m_CategoryNames[0] = "<All>";

            m_CategoryIds = new string[catCount + 1];

            for (int i = 0; i < catCount; i++)
            {
                var def = categoryDefinitions[i];
                m_CategoryNames[i + 1] = def.displayName;
                m_CategoryIds[i + 1] = def.id;

                if (selectedCategoryId == def.id)
                {
                    m_SelectedCategoryIndex = i + 1;
                }
            }

            if (m_SelectedCategoryIndex == -1)
            {
                ChangeSelectedCategory(0);
            }
        }

        public override void OnInspectorGUI()
        {
            // Pull the information from the target into the serializedObject.
            serializedObject.Update();

            var storeDisplayContent = new GUIContent("Store", "The Store to display in this view.");
            var selectedStoreIndex = EditorGUILayout.Popup(storeDisplayContent, m_SelectedStoreIndex, m_StoreNames);
            ChangeSelectedStore(selectedStoreIndex);

            var categoryDisplayContent = new GUIContent("Category", "If a specific category is selected, list of Transaction Items rendered will be filtered to only ones in that category.");
            var selectedCategoryIndex = EditorGUILayout.Popup(categoryDisplayContent, m_SelectedCategoryIndex, m_CategoryNames);
            ChangeSelectedCategory(selectedCategoryIndex);

            EditorGUILayout.Space();
            
            var oldItemSpriteName = m_StoreView.itemIconSpriteName;
            var newItemSpriteName = m_ItemSpriteNameSelector.Draw(oldItemSpriteName, "Item Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the item icon.");
            if (newItemSpriteName != oldItemSpriteName)
            {
                m_ItemIconSpriteName_SerializedProperty.stringValue = newItemSpriteName;
                m_StoreView.SetItemIconSpriteName(newItemSpriteName);
            }
            
            var oldPriceIconName = m_StoreView.priceIconSpriteName;
            var newPriceIconName = m_PriceSpriteNameSelector.Draw(oldPriceIconName, "Price Icon Sprite Name", "Sprite name that is defined on Assets Detail to display the price icon.");
            if (newPriceIconName != oldPriceIconName)
            { 
                m_PriceIconSpriteName_SerializedProperty.stringValue = newPriceIconName;
                m_StoreView.SetPriceIconSpriteName(newPriceIconName);
            }
            
            EditorGUILayout.Space();
            
            var interactableContent = new GUIContent("Interactable", "Sets the Purchase Button's interactable state.");
            bool oldInteractable = m_StoreView.interactable;
            var newInteractable = EditorGUILayout.Toggle(interactableContent, oldInteractable);
            if (oldInteractable != newInteractable)
            {
                m_StoreView.interactable = m_Interactable_SerializedProperty.boolValue = newInteractable;
            }

            EditorGUILayout.Space();
            
            // Use the default object field GUI for these properties.
            DrawPropertiesExcluding(serializedObject, kExcludedFields);

            // Push all changes made on the serializedObject back to the target.
            serializedObject.ApplyModifiedProperties();
        }

        private void ChangeSelectedStore(int index)
        {
            if (m_SelectedStoreIndex == index || m_StoreIds == null || m_StoreIds.Length <= index || index < -1)
            {
                return;
            }

            m_SelectedStoreIndex = index;

            // Update the serialized value
            m_StoreId_SerializedProperty.stringValue = m_StoreIds[index];
            m_StoreView.SetStoreId(m_StoreIds[index]);
        }

        private void ChangeSelectedCategory(int index)
        {
            if (m_SelectedCategoryIndex == index || m_CategoryIds == null || m_CategoryIds.Length <= index || index < -1)
            {
                return;
            }

            m_SelectedCategoryIndex = index;

            // Update the serialized value
            m_CategoryId_SerializedProperty.stringValue = m_CategoryIds[index];
            m_StoreView.SetCategoryId(m_CategoryIds[index]);
        }
    }
}
#endif
