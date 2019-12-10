using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class InventoryItemDefinitionEditor : CollectionEditorBase<InventoryItemDefinition>
    {
        private string m_CurrentItemId = null;
        private string m_LastSelectedReferenceDefinitionId = null;
        private string m_LastSelectedReferenceDefinitionName = null;
        private static GameItemDefinition s_CreatingReferenceDefinition = null;

        private CategoryPickerEditor m_CategoryPicker;

        public InventoryItemDefinitionEditor(string name, InventoryEditorWindow window) : base(name, window)
        {
            m_CategoryPicker = new CategoryPickerEditor();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            m_CategoryPicker.RefreshCategories();

            m_CategoryFilterEditor.RefreshSidebarCategoryFilterList(m_CategoryPicker.categoryDefinitions);

            if (GameFoundationSettings.database != null
                && GameFoundationSettings.database.inventoryCatalog != null)
            {
                GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            SelectFilteredItem(0); // Select the first Item
            GameFoundationAnalytics.SendOpenTabEvent(GameFoundationAnalytics.TabName.InventoryItems);
        }

        protected override List<InventoryItemDefinition> GetFilteredItems()
        {
            return m_CategoryFilterEditor.GetFilteredItems(GetItems(), m_CategoryPicker.categoryDefinitions);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
            s_CreatingReferenceDefinition = null;
        }

        protected override void DrawCreateInputFields()
        {
            ReferenceDefinitionPickerEditor.DrawReferenceDefinitionPicker(
                CreateReferenceDefinitionMenu(createNewMode: true, s_CreatingReferenceDefinition, new List<GameItemDefinition>(GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions())),
                s_CreatingReferenceDefinition,
                new GUIContent("Reference Definition", "Optionally start creating a new Inventory Item Definition from a reference to a Game Item Definition. This will link the item definition being created with the chosen GameItemDefinition. This will allow it to inherit details from the GameItemDefinition.")
            );
            m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_NewItemId, ref m_NewItemDisplayName);
        }

        protected override void CreateNewItemFinalize()
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Could not create new item because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                Debug.LogError("Could not create new item because the inventory catalog is null.");
                return;
            }

            InventoryItemDefinition itemDefinition = InventoryItemDefinition.Create(m_NewItemId, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(itemDefinition, GameFoundationSettings.database.inventoryCatalog);

            // If filter is currently set to a category, add that category to the category list of the item currently being created
            CategoryDefinition currentFilteredCategory = m_CategoryFilterEditor.GetCurrentFilteredCategory(m_CategoryPicker.categoryDefinitions);
            if (currentFilteredCategory != null)
            {
                List<CategoryDefinition> existingItemCategories = new List<CategoryDefinition>();
                itemDefinition.GetCategories(existingItemCategories);
                if (existingItemCategories.All(category => category.hash != currentFilteredCategory.hash))
                {
                    itemDefinition.AddCategory(currentFilteredCategory);
                }
            }

            if (s_CreatingReferenceDefinition != null)
            {
                itemDefinition.referenceDefinition = s_CreatingReferenceDefinition;
            }

            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            AddItem(itemDefinition);
            SelectItem(itemDefinition);
            m_CurrentItemId = m_NewItemId;
            RefreshItems();
            DrawGeneralDetail(itemDefinition);
        }

        protected override void AddItem(InventoryItemDefinition inventoryItemDefinition)
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Inventory Item Definition " + inventoryItemDefinition.displayName + " could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                Debug.LogError("Inventory Item Definition " + inventoryItemDefinition.displayName + " could not be added because the inventory catalog is null");
            }
            else
            {
                GameFoundationSettings.database.inventoryCatalog.AddItemDefinition(inventoryItemDefinition);
                EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
            }
        }

        protected override void DrawDetail(InventoryItemDefinition inventoryItemDefinition, int index, int count)
        {
            DrawGeneralDetail(inventoryItemDefinition);

            EditorGUILayout.Space();

            m_CategoryPicker.DrawCategoryPicker(inventoryItemDefinition);

            EditorGUILayout.Space();

            DetailEditorGUI.DrawDetailView(inventoryItemDefinition);

            // make sure this is the last to draw
            m_CategoryPicker.DrawCategoryPickerPopup(inventoryItemDefinition);
        }

        private void DrawGeneralDetail(InventoryItemDefinition inventoryItemDefinition)
        {
            if (GameFoundationSettings.database == null
                || GameFoundationSettings.database.inventoryCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = inventoryItemDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);
                if (inventoryItemDefinition.displayName != displayName)
                {
                    inventoryItemDefinition.displayName = displayName;
                    EditorUtility.SetDirty(inventoryItemDefinition);
                }

                ReferenceDefinitionPickerEditor.DrawReferenceDefinitionPicker(
                    CreateReferenceDefinitionMenu(createNewMode: false, inventoryItemDefinition.referenceDefinition, new List<GameItemDefinition>(GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions()), inventoryItemDefinition),
                    inventoryItemDefinition.referenceDefinition,
                    new GUIContent("Reference Definition", "Attaching a Reference Definition links the displayed item definition with the chosen GameItemDefinition. This will allow it to inherit details from the GameItemDefinition.")
                );
            }
        }

        protected override void DrawSidebarList()
        {
            EditorGUILayout.Space();

            bool categoryChanged;
            m_CategoryFilterEditor.DrawCategoryFilter(out categoryChanged);

            if (categoryChanged)
            {
                if (m_SelectedItem == null || !m_SelectedItem.HasCategoryDefinition(m_CategoryFilterEditor.GetCurrentFilteredCategory(m_CategoryPicker.categoryDefinitions)))
                {
                    SelectFilteredItem(0);
                }
            }

            base.DrawSidebarList();
        }

        protected override void DrawSidebarListItem(InventoryItemDefinition item, int index)
        {
            BeginSidebarItem(item, index, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(item);

            EndSidebarItem(item, index);
        }

        protected override void SelectItem(InventoryItemDefinition item)
        {
            m_CategoryPicker.ResetCategorySearch();

            if (item != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentItemId = item.id;
            }

            base.SelectItem(item);
        }

        protected override void OnRemoveItem(InventoryItemDefinition inventoryItemDefinition)
        {
            if (inventoryItemDefinition != null)
            {
                if (GameFoundationSettings.database == null)
                {
                    Debug.LogError("Inventory Item Definition " + inventoryItemDefinition.displayName + " could not be removed because the Game Foundation database is null");
                }
                else if (GameFoundationSettings.database.inventoryCatalog == null)
                {
                    Debug.LogError("Inventory Item Definition " + inventoryItemDefinition.displayName + " could not be removed because the inventory catalog is null");
                }
                else
                {
                    if (GameFoundationSettings.database.inventoryCatalog.RemoveItemDefinition(inventoryItemDefinition))
                    {
                        CollectionEditorTools.AssetDatabaseRemoveObject(inventoryItemDefinition);
                        EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                    }
                    else
                    {
                        Debug.LogError("Inventory Item Definition " + inventoryItemDefinition.displayName + " was not removed from the inventory catalog.");
                    }
                }
            }
        }

        private GenericMenu CreateReferenceDefinitionMenu(bool createNewMode, GameItemDefinition referenceDefinition, List<GameItemDefinition> siblingGameItemDefinitions, InventoryItemDefinition inventoryItemDefinition = null)
        {
            // NOTE: Reason for using GenericMenu:
            // NOTE: EditorGUILayout.ObjectField is too broad, it doesn't show what asset an object is nested in
            // NOTE: EditorGUIUtility.ShowObjectPicker with a search filter is also not going to work since you can't filter by specific asset file

            if (GameFoundationSettings.database == null)
            {
                throw new System.NullReferenceException("There is no Game Foundation database!");
            }

            if (GameFoundationSettings.database.gameItemCatalog == null)
            {
                throw new System.NullReferenceException("There is no GameItemDefinition catalog!");
            }

            if (siblingGameItemDefinitions == null)
            {
                throw new System.NullReferenceException("Cannot pass a null value for siblingGameItemDefinitions into DrawReferenceDefinitionPicker!");
            }

            GenericMenu menu = new GenericMenu();

            var gameItemDefinitions = GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinitions();

            List<GameItemDefinition> defsAlreadyReferenced = new List<GameItemDefinition>();

            foreach (GameItemDefinition rootGameItemDefinition in siblingGameItemDefinitions)
            {
                if (rootGameItemDefinition.referenceDefinition != null)
                {
                    defsAlreadyReferenced.Add(rootGameItemDefinition.referenceDefinition);
                }
            }

            if (createNewMode)
            {
                AddMenuItemNoneCreateNewMode(menu);
            }
            else if (inventoryItemDefinition != null)
            {
                AddMenuItemNoneEditExistingMode(menu, inventoryItemDefinition);
            }

            foreach (GameItemDefinition rootGameItemDefinition in gameItemDefinitions)
            {
                if (rootGameItemDefinition == null)
                {
                    Debug.LogWarning("There is a null entry in the GameItemDefinition collection!");
                    continue;
                }

                if (referenceDefinition != rootGameItemDefinition
                    && defsAlreadyReferenced.Contains(rootGameItemDefinition))
                {
                    menu.AddDisabledItem(new GUIContent(rootGameItemDefinition.displayName));
                }
                else
                {
                    bool selected = referenceDefinition ? referenceDefinition.id == rootGameItemDefinition.id : false;

                    if (createNewMode)
                    {
                        AddMenuItemCreateNewMode(menu, rootGameItemDefinition, selected);
                    }
                    else if (inventoryItemDefinition != null)
                    {
                        AddMenuItemEditExistingMode(menu, rootGameItemDefinition, inventoryItemDefinition, selected);
                    }
                }
            }

            return menu;
        }

        private void AddMenuItemCreateNewMode(GenericMenu menu, GameItemDefinition rootGameItemDefinition, bool selected)
        {
            menu.AddItem(
                new GUIContent(rootGameItemDefinition.displayName),
                selected,
                () =>
                {
                    bool changeId = false;
                    bool changeName = false;

                    if (m_NewItemId == string.Empty)
                    {
                        changeId = true;
                    }

                    if (m_NewItemDisplayName == string.Empty)
                    {
                        changeName = true;
                    }

                    if (m_NewItemDisplayName == m_LastSelectedReferenceDefinitionName && m_NewItemId == m_LastSelectedReferenceDefinitionId)
                    {
                        changeName = true;
                        changeId = true;
                    }

                    if (changeId)
                    {
                        m_NewItemId = rootGameItemDefinition.id;
                    }
                    if (changeName)
                    {
                        m_NewItemDisplayName = rootGameItemDefinition.displayName;
                    }

                    s_CreatingReferenceDefinition = rootGameItemDefinition;
                    m_LastSelectedReferenceDefinitionId = rootGameItemDefinition.id;
                    m_LastSelectedReferenceDefinitionName = rootGameItemDefinition.displayName;
                }
            );
        }

        private void AddMenuItemNoneCreateNewMode(GenericMenu menu)
        {
            menu.AddItem(
                new GUIContent("None"),
                s_CreatingReferenceDefinition == null,
                () => { s_CreatingReferenceDefinition = null; }
            );
        }

        private void AddMenuItemEditExistingMode(GenericMenu menu, GameItemDefinition rootGameItemDefinition, InventoryItemDefinition inventoryItemDefinition, bool selected)
        {
            menu.AddItem(
                new GUIContent(rootGameItemDefinition.displayName),
                selected,
                () =>
                {
                    inventoryItemDefinition.referenceDefinition = rootGameItemDefinition;
                    EditorUtility.SetDirty(inventoryItemDefinition);
                }
            );
        }

        private void AddMenuItemNoneEditExistingMode(GenericMenu menu, InventoryItemDefinition inventoryItemDefinition)
        {
            menu.AddItem(
                new GUIContent("None"),
                inventoryItemDefinition.referenceDefinition == null,
                () =>
                {
                    inventoryItemDefinition.referenceDefinition = null;
                    EditorUtility.SetDirty(inventoryItemDefinition);
                }
            );
        }
    }
}
