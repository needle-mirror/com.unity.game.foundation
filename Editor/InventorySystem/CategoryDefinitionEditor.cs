using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class CategoryDefinitionEditor : CollectionEditorBase<CategoryDefinition>
    {
        private string m_CurrentCategoryDefinitionId = null;

        public CategoryDefinitionEditor(string name, InventoryEditorWindow window) : base(name, window)
        {
        }
        
        protected override List<CategoryDefinition> GetFilteredItems()
        {
            return GetItems();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            if (GameFoundationSettings.database != null
                && GameFoundationSettings.database.inventoryCatalog != null)
            {
                GameFoundationSettings.database.inventoryCatalog.GetCategories(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            SelectFilteredItem(0); // Select the first Item
            GameFoundationAnalytics.SendOpenTabEvent(GameFoundationAnalytics.TabName.Categories);
        }

        protected override void SelectItem(CategoryDefinition categoryDefinition)
        {
            if (categoryDefinition != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentCategoryDefinitionId = categoryDefinition.id;
            }

            base.SelectItem(categoryDefinition);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
        }

        protected override void AddItem(CategoryDefinition categoryDefinition)
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Category " + categoryDefinition.displayName + " could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                Debug.LogError("Category " + categoryDefinition.displayName + " could not be added because the inventory catalog is null");
            }
            else
            {
                GameFoundationSettings.database.inventoryCatalog.AddCategory(categoryDefinition);
                EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            }
        }

        protected override void CreateNewItemFinalize()
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Could not create new category definition because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                Debug.LogError("Could not create new category definition because the inventory catalog is null.");
                return;
            }

            CategoryDefinition categoryDefinition = new CategoryDefinition(m_NewItemId, m_NewItemDisplayName);

            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            AddItem(categoryDefinition);
            SelectItem(categoryDefinition);
            m_CurrentCategoryDefinitionId = m_NewItemId;
            RefreshItems();
            List<CategoryDefinition> categoryDefinitions = GetItems();
            DrawDetail(categoryDefinition, categoryDefinitions.FindIndex(x => x.Equals(m_SelectedItem)), categoryDefinitions.Count);
        }

        protected override void DrawDetail(CategoryDefinition categoryDefinition, int index, int count)
        {
            if (GameFoundationSettings.database == null
                || GameFoundationSettings.database.inventoryCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = categoryDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentCategoryDefinitionId, ref displayName);

                if (categoryDefinition.displayName != displayName)
                {
                    categoryDefinition.displayName = displayName;
                    EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                }
            }
        }

        protected override void DrawSidebarListItem(CategoryDefinition item, int index)
        {
            BeginSidebarItem(item, index, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(item);

            EndSidebarItem(item, index);
        }

        protected override void OnRemoveItem(CategoryDefinition categoryDefinition)
        {
            if (categoryDefinition != null)
            {
                // loop through all inventory items and attempt to remove category in case they are referring to it.
                foreach (InventoryItemDefinition inventoryItemDefinition in GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions())
                {
                    inventoryItemDefinition.RemoveCategory(categoryDefinition);
                }

                m_CategoryFilterEditor.ResetCategoryFilter();

                if (GameFoundationSettings.database.inventoryCatalog != null)
                {
                    bool successfullyRemoved = GameFoundationSettings.database.inventoryCatalog.RemoveCategory(categoryDefinition);
                    if (!successfullyRemoved)
                    {
                        Debug.LogError("Category " + categoryDefinition.displayName + " was unable to be removed from inventory catalog list.");
                    }
                }
                else
                {
                    Debug.LogError("Category " + categoryDefinition.displayName + " could not be removed from inventory catalog because catalog is null");
                }
            }
        }
    }
}
