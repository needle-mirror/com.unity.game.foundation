using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    internal class InventoryItemDefinitionAssetEditor : CollectionEditorBase<InventoryItemDefinitionAsset>
    {
        private string m_CurrentItemId;

        private readonly CategoryPickerEditor m_CategoryPicker;

        public InventoryItemDefinitionAssetEditor(string name) : base(name)
        {
            m_CategoryPicker = new CategoryPickerEditor
                (GameFoundationDatabaseSettings.database.m_InventoryCatalog);
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            m_CategoryPicker.RefreshCategories();

            m_CategoryFilterEditor.RefreshSidebarCategoryFilterList(m_CategoryPicker.categories);

            if (GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.inventoryCatalog != null)
            {
                GameFoundationDatabaseSettings.database.inventoryCatalog.GetItems(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            SelectFilteredItem(0); // Select the first Item
            GameFoundationAnalytics.SendOpenTabEvent(GameFoundationAnalytics.TabName.InventoryItems);
        }

        protected override List<InventoryItemDefinitionAsset> GetFilteredItems()
        {
            if (m_CategoryPicker == null) return null;
            return m_CategoryFilterEditor.GetFilteredItems(GetItems(), m_CategoryPicker.categories);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
        }

        protected override void DrawCreateInputFields()
        {
            m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_NewItemId, ref m_NewItemDisplayName);

            if (m_ClickedCreateButton)
            {
                EditorGUI.FocusTextInControl("displayName");
            }
        }

        protected override void CreateNewItemFinalize()
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("Could not create new item because the Game Foundation database is null.");
                return;
            }

            var catalog = GameFoundationDatabaseSettings.database.inventoryCatalog;

            if (catalog == null)
            {
                Debug.LogError("Could not create new item because the inventory catalog is null.");
                return;
            }

            var itemDefinition = ScriptableObject.CreateInstance<InventoryItemDefinitionAsset>();
            itemDefinition.Editor_Initialize
                (catalog, m_NewItemId, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(itemDefinition, catalog);

            // If filter is currently set to a category, add that category to the category list of the item currently being created
            var currentFilteredCategory = m_CategoryFilterEditor.GetCurrentFilteredCategory(m_CategoryPicker.categories);

            if (currentFilteredCategory != null)
            {
                var existingItemCategories = new List<CategoryAsset>();
                itemDefinition.GetCategories(existingItemCategories);

                if (existingItemCategories.All(category => category.id != currentFilteredCategory.id))
                {
                    itemDefinition.Editor_AddCategory(currentFilteredCategory);
                }
            }

            EditorUtility.SetDirty(catalog);
            AddItem(itemDefinition);
            SelectItem(itemDefinition);
            m_CurrentItemId = m_NewItemId;
            RefreshItems();
            DrawGeneralDetail(itemDefinition);
        }

        protected override void AddItem(InventoryItemDefinitionAsset inventoryItemDefinition)
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"{nameof(InventoryItemDefinitionAsset)} {inventoryItemDefinition.displayName} could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationDatabaseSettings.database.inventoryCatalog == null)
            {
                Debug.LogError($"{nameof(InventoryItemDefinitionAsset)} {inventoryItemDefinition.displayName} could not be added because the inventory catalog is null");
            }
            else
            {
                GameFoundationDatabaseSettings.database.inventoryCatalog.Editor_AddItem(inventoryItemDefinition);
                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.inventoryCatalog);
            }
        }

        protected override void DrawDetail(InventoryItemDefinitionAsset inventoryItemDefinition, int index, int count)
        {
            DrawGeneralDetail(inventoryItemDefinition);

            EditorGUILayout.Space();

            m_CategoryPicker.DrawCategoryPicker(inventoryItemDefinition);

            EditorGUILayout.Space();

            DetailEditorGUI.DrawDetailView(inventoryItemDefinition);

            // make sure this is the last to draw
            m_CategoryPicker.DrawCategoryPickerPopup(inventoryItemDefinition);
        }

        private void DrawGeneralDetail(InventoryItemDefinitionAsset inventoryItemDefinition)
        {
            if (GameFoundationDatabaseSettings.database == null
                || GameFoundationDatabaseSettings.database.inventoryCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var displayName = inventoryItemDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);

                if (inventoryItemDefinition.displayName != displayName)
                {
                    inventoryItemDefinition.Editor_SetDisplayName(displayName);
                    EditorUtility.SetDirty(inventoryItemDefinition);
                }
            }
        }

        protected override void DrawSidebarList()
        {
            EditorGUILayout.Space();

            m_CategoryFilterEditor.DrawCategoryFilter(out var categoryChanged);

            if (categoryChanged)
            {
                var selectedCategory = m_CategoryFilterEditor.GetCurrentFilteredCategory(m_CategoryPicker.categories);
                if (m_SelectedItem == null || selectedCategory == null || !m_SelectedItem.HasCategory(selectedCategory))
                {
                    SelectFilteredItem(0);
                }
            }

            base.DrawSidebarList();
        }

        protected override void DrawSidebarListItem(InventoryItemDefinitionAsset item)
        {
            BeginSidebarItem(item, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(item);

            EndSidebarItem();
        }

        protected override void SelectItem(InventoryItemDefinitionAsset item)
        {
            m_CategoryPicker.ResetCategorySearch();

            if (item != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentItemId = item.id;
            }

            base.SelectItem(item);
        }

        protected override void OnRemoveItem(InventoryItemDefinitionAsset inventoryItemDefinitionAsset)
        {
            CollectionEditorTools.RemoveObjectFromCatalogAsset(inventoryItemDefinitionAsset);
            Object.DestroyImmediate(inventoryItemDefinitionAsset, true);
        }
    }
}
