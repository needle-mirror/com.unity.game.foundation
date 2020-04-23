using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    internal class CategoryAssetEditor : CollectionEditorBase<CategoryAsset>
    {
        private string m_CurrentCategoryId;

        public CategoryAssetEditor(string name) : base(name)
        {
        }
        
        protected override List<CategoryAsset> GetFilteredItems()
        {
            return GetItems();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            GameFoundationDatabaseSettings.database.inventoryCatalog.GetCategories(GetItems());
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            SelectFilteredItem(0); // Select the first Item
            GameFoundationAnalytics.SendOpenTabEvent(GameFoundationAnalytics.TabName.Categories);
        }

        protected override void SelectItem(CategoryAsset category)
        {
            if (category != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentCategoryId = category.id;
            }

            base.SelectItem(category);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
        }

        protected override void AddItem(CategoryAsset category)
        {
            var catalog = GameFoundationDatabaseSettings.database.inventoryCatalog;
            catalog.Editor_AddCategory(category);
            CollectionEditorTools.AssetDatabaseAddObject(category, catalog);
        }

        protected override void CreateNewItemFinalize()
        {
            var category = CategoryAsset.Editor_Create(m_NewItemId, m_NewItemDisplayName);
            if (category is null) return;

            AddItem(category);

            SelectItem(category);
            m_CurrentCategoryId = m_NewItemId;
            RefreshItems();
            var categories = GetItems();
            DrawDetail(category, categories.FindIndex(x => x.Equals(m_SelectedItem)), categories.Count);
        }

        protected override void DrawDetail(CategoryAsset category, int index, int count)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = category.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentCategoryId, ref displayName);

                if (category.displayName != displayName)
                {
                    category.Editor_SetDisplayName(displayName);
                }
            }
        }

        protected override void DrawSidebarListItem(CategoryAsset category)
        {
            BeginSidebarItem(category, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(category.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(category);

            EndSidebarItem();
        }

        protected override void OnRemoveItem(CategoryAsset categoryAsset)
        {
            CollectionEditorTools.RemoveObjectFromCatalogAsset(categoryAsset);
            Object.DestroyImmediate(categoryAsset, true);
            m_CategoryFilterEditor.ResetCategoryFilter();
        }
    }
}
