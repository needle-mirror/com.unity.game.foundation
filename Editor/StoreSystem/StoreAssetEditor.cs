using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    internal class StoreAssetEditor : CollectionEditorBase<StoreAsset>
    {
        private string m_StoreId;
        private StoreItemObject[] m_StoreItems;
        private StoreItemObject m_StoreItemToMoveDown;
        private StoreItemObject m_StoreItemToMoveUp;
        private StoreItemObject m_StoreItemToRemove;

        private readonly CategoryPickerEditor m_CategoryPicker;

        public StoreAssetEditor(string name) : base(name)
        {
            m_CategoryPicker = new CategoryPickerEditor
                (GameFoundationDatabaseSettings.database.m_StoreCatalog);
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            m_CategoryPicker.RefreshCategories();

            m_CategoryFilterEditor.RefreshSidebarCategoryFilterList(m_CategoryPicker.categories);

            if (GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.storeCatalog != null)
            {
                GameFoundationDatabaseSettings.database.storeCatalog.GetItems(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            SelectFilteredItem(0); // Select the first Item
            GameFoundationAnalytics.SendOpenTabEvent(GameFoundationAnalytics.TabName.Stores);
        }

        protected override List<StoreAsset> GetFilteredItems()
        {
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
                Debug.LogError("Could not create new store definition because the Game Foundation database is null.");
                return;
            }

            var catalog = GameFoundationDatabaseSettings.database.storeCatalog;

            if (catalog == null)
            {
                Debug.LogError("Could not create new store definition because the store catalog is null.");
                return;
            }

            var store = ScriptableObject.CreateInstance<StoreAsset>();
            store.Editor_Initialize(catalog, m_NewItemId, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(store, catalog);

            // If filter is currently set to a category, add that category to the category list of the item currently being created
            var currentFilteredCategory = m_CategoryFilterEditor.GetCurrentFilteredCategory(m_CategoryPicker.categories);

            if (currentFilteredCategory != null)
            {
                var existingItemCategories = new List<CategoryAsset>();
                store.GetCategories(existingItemCategories);

                if (existingItemCategories.All(category => category.id != currentFilteredCategory.id))
                {
                    store.Editor_AddCategory(currentFilteredCategory);
                }
            }

            EditorUtility.SetDirty(catalog);
            AddItem(store);
            SelectItem(store);
            m_StoreId = m_NewItemId;
            RefreshItems();
            DrawGeneralDetail(store);
        }

        protected override void AddItem(StoreAsset store)
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"{nameof(StoreAsset)} {store.displayName} could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationDatabaseSettings.database.storeCatalog == null)
            {
                Debug.LogError($"{nameof(StoreAsset)} {store.displayName} could not be added because the store catalog is null");
            }
            else
            {
                GameFoundationDatabaseSettings.database.storeCatalog.Editor_AddItem(store);
                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.storeCatalog);
            }
        }

        protected override void DrawDetail(StoreAsset store, int index, int count)
        {
            DrawGeneralDetail(store);

            EditorGUILayout.Space();

            m_CategoryPicker.DrawCategoryPicker(store);

            EditorGUILayout.Space();

            DrawStoreDetail(store, index, count);

            // make sure this is the last to draw
            m_CategoryPicker.DrawCategoryPickerPopup(store);
        }

        private void DrawGeneralDetail(StoreAsset store)
        {
            if (GameFoundationDatabaseSettings.database == null
                || GameFoundationDatabaseSettings.database.storeCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var displayName = store.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_StoreId, ref displayName);

                if (store.displayName != displayName)
                {
                    store.Editor_SetDisplayName(displayName);
                    EditorUtility.SetDirty(store);
                }

                if (IsIdReserved(store.id))
                {
                    GUI.enabled = false;
                    CollectionEditorTools.SetGUIEnabledAtEditorTime(true);
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

        protected override void DrawSidebarListItem(StoreAsset store)
        {
            BeginSidebarItem(store, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(store.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            if (!IsIdReserved(store.id))
            {
                DrawSidebarItemRemoveButton(store);
            }

            EndSidebarItem();
        }

        protected override void SelectItem(StoreAsset store)
        {
            m_CategoryPicker.ResetCategorySearch();

            if (store != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_StoreId = store.id;
            }

            base.SelectItem(store);
        }

        protected override void OnRemoveItem(StoreAsset storeAsset)
        {
            CollectionEditorTools.RemoveObjectFromCatalogAsset(storeAsset);
            Object.DestroyImmediate(storeAsset, true);
        }

        private void DrawStoreDetail(StoreAsset store, int index, int count)
        {
            m_StoreItems = store.GetStoreItems();

            DrawItemsInStore(store);

            EditorGUILayout.Space();

            DrawItemsNotInStore(store);
        }

        private void DrawItemsInStore(StoreAsset store)
        {
            m_StoreItemToMoveUp = null;
            m_StoreItemToMoveDown = null;
            m_StoreItemToRemove = null;

            var allTransactions = GameFoundationDatabaseSettings.database.transactionCatalog.GetItems();

            var storeTransactionsLabel =
                new GUIContent(
                    "Store Transactions",
                    "At runtime, the store will only show the transactions in this list, as long as they are marked as visible.");

            EditorGUILayout.LabelField(storeTransactionsLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var anyAddedTransactionsFlag = (m_StoreItems != null && m_StoreItems.Length > 0);

                using (new EditorGUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    EditorGUILayout.LabelField("Transactions", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField(anyAddedTransactionsFlag ? "Visible" : "", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(80));

                    GUILayout.Space(64);
                }

                if (anyAddedTransactionsFlag)
                {
                    for (var i = 0; i < m_StoreItems.Length; i++)
                    {
                        var defaultStoreItem = m_StoreItems[i];

                        var transaction = allTransactions.FirstOrDefault(item => item == defaultStoreItem.m_Transaction);

                        if (transaction == null)
                        {
                            continue;
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(5);

                            EditorGUILayout.LabelField(transaction.displayName, GUILayout.Width(300));

                            GUILayout.FlexibleSpace();

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(transaction != null);

                            var newVisibleFlag = EditorGUILayout.Toggle(defaultStoreItem.enabled, GUILayout.Width(70));
                            if (newVisibleFlag != defaultStoreItem.enabled)
                            {
                                defaultStoreItem.Editor_SetEnabled(newVisibleFlag);
                                EditorUtility.SetDirty(store);
                            }

                            GUILayout.Space(5);

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i < m_StoreItems.Length - 1);

                            if (GUILayout.Button("\u25BC", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_StoreItemToMoveDown = defaultStoreItem;
                                m_StoreItemToMoveUp = m_StoreItems[i + 1];
                            }

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i > 0);

                            if (GUILayout.Button("\u25B2", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_StoreItemToMoveUp = defaultStoreItem;
                                m_StoreItemToMoveDown = m_StoreItems[i - 1];
                            }

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(true);

                            GUILayout.Space(5);

                            if (GUILayout.Button("X", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_StoreItemToRemove = defaultStoreItem;
                            }
                        }
                    }
                }
                else
                {
                    EditorGUILayout.Space();

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("no transactions in store");
                        GUILayout.FlexibleSpace();
                    }

                    EditorGUILayout.Space();
                }
            }

            if (m_StoreItemToMoveUp != null && m_StoreItemToMoveDown != null)
            {
                SwapStoreItems(store, m_StoreItemToMoveUp, m_StoreItemToMoveDown);
            }

            if (m_StoreItemToRemove != null)
            {
                if (EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete the selected item?", "Yes", "Cancel"))
                {
                    store.Editor_RemoveItem(m_StoreItemToRemove);
                    EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.storeCatalog);
                }
            }

            if (m_StoreItemToMoveUp != null || m_StoreItemToMoveDown != null || m_StoreItemToRemove != null)
            {
                GUI.FocusControl(null);
            }
        }

        private void DrawItemsNotInStore(StoreAsset store)
        {
            var allTransactions = GameFoundationDatabaseSettings.database.transactionCatalog.GetItems();

            var otherAvailableItemsLabel =
                new GUIContent(
                    "Other Available Transactions",
                    "Transactions that are eligible to be added to this store.");

            EditorGUILayout.LabelField(otherAvailableItemsLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (new EditorGUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    EditorGUILayout.LabelField("Transactions", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));
                    GUILayout.FlexibleSpace();
                }

                var validItemCount = 0;

                foreach (var transaction in allTransactions)
                {
                    if (m_StoreItems.Length > 0 && m_StoreItems.Any(storeItem => storeItem.m_Transaction == transaction))
                    {
                        continue;
                    }

                    validItemCount++;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(5);

                        EditorGUILayout.LabelField(transaction.displayName);

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Add To Store", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(150)))
                        {
                            store.Editor_AddItem(transaction);
                            EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.storeCatalog);
                        }
                    }
                }

                if (validItemCount > 0)
                {
                    return;
                }

                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("no transactions available");
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();
            }
        }

        private static void SwapStoreItems(StoreAsset store, StoreItemObject storeItem1, StoreItemObject storeItem2)
        {
            store.Editor_SwapItemsListOrder(storeItem1, storeItem2);
            EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.storeCatalog);
        }

        private static bool IsIdReserved(string id)
        {
            return id == StoreCatalogAsset.k_MainStoreDefinitionId;
        }
    }
}
