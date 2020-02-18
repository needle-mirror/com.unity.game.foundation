using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    internal class InventoryDefinitionEditor : CollectionEditorBase<InventoryDefinition>
    {
        private string m_InventoryId;
        private DefaultItem[] m_DefaultItems;
        private DefaultItem m_DefaultItemToMoveDown;
        private DefaultItem m_DefaultItemToMoveUp;
        private DefaultItem m_DefaultItemToRemove;
        private DefaultCollectionDefinition m_DefaultInventoryDefinition;
        private bool m_CreateDefaultInventory;

        public InventoryDefinitionEditor(string name) : base(name)
        {
        }

        protected override List<InventoryDefinition> GetFilteredItems()
        {
            return GetItems();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            if (GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.inventoryCatalog != null)
            {
                GameFoundationDatabaseSettings.database.inventoryCatalog.GetCollectionDefinitions(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            SelectFilteredItem(0); // Select the first Item
            GameFoundationAnalytics.SendOpenTabEvent(GameFoundationAnalytics.TabName.Inventories);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
        }

        protected override void CreateNewItemFinalize()
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("Could not create new inventory definition because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationDatabaseSettings.database.inventoryCatalog == null)
            {
                Debug.LogError("Could not create new inventory definition because the inventory catalog is null.");
                return;
            }

            InventoryDefinition inventory = InventoryDefinition.Create(m_NewItemId, m_NewItemDisplayName);

            if (inventory != null)
            {
                AddItem(inventory);
                CollectionEditorTools.AssetDatabaseAddObject(inventory, GameFoundationDatabaseSettings.database.inventoryCatalog);
                SelectItem(inventory);
                m_InventoryId = m_NewItemId;
                RefreshItems();
                DrawGeneralDetail(inventory);
            }
            else
            {
                Debug.LogError("Sorry, there was an error creating new inventory. Please try again.");
            }
        }

        protected override void AddItem(InventoryDefinition inventoryDefinition)
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"Inventory Definition {inventoryDefinition.displayName} could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationDatabaseSettings.database.inventoryCatalog == null)
            {
                Debug.LogError($"Inventory Definition {inventoryDefinition.displayName} could not be added because the inventory catalog is null");
            }
            else
            {
                GameFoundationDatabaseSettings.database.inventoryCatalog.AddCollectionDefinition(inventoryDefinition);
                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.inventoryCatalog);
            }
        }

        protected override void DrawDetail(InventoryDefinition inventory, int index, int count)
        {
            DrawGeneralDetail(inventory);

            EditorGUILayout.Space();

            DrawInventoryDetail(inventory, index, count);
        }

        private void DrawGeneralDetail(InventoryDefinition inventoryDefinition)
        {
            if (GameFoundationDatabaseSettings.database == null
                || GameFoundationDatabaseSettings.database.inventoryCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var displayName = inventoryDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_InventoryId, ref displayName);

                if (inventoryDefinition.displayName != displayName)
                {
                    inventoryDefinition.displayName = displayName;
                    EditorUtility.SetDirty(inventoryDefinition);
                }

                if (IsIdReserved(inventoryDefinition.id))
                {
                    GUI.enabled = false;
                }

                var autoCreateInventoryLabel =
                    new GUIContent(
                        "Auto Create Instance",
                        "When checked, an instance of this InventoryDefinition will automatically get instantiated at runtime when Game Foundation initializes.");

                var newAutoCreateInventorySelection = EditorGUILayout.Toggle(autoCreateInventoryLabel, m_CreateDefaultInventory);
                if (newAutoCreateInventorySelection != m_CreateDefaultInventory)
                {
                    if (newAutoCreateInventorySelection)
                    {
                        m_DefaultInventoryDefinition = new DefaultCollectionDefinition(inventoryDefinition.id, inventoryDefinition.displayName, inventoryDefinition.hash);
                        GameFoundationDatabaseSettings.database.inventoryCatalog.AddDefaultCollectionDefinition(m_DefaultInventoryDefinition);
                    }
                    else
                    {
                        GameFoundationDatabaseSettings.database.inventoryCatalog.RemoveDefaultCollectionDefinition(m_DefaultInventoryDefinition);
                        m_DefaultInventoryDefinition = null;
                    }
                    EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.inventoryCatalog);
                    m_CreateDefaultInventory = newAutoCreateInventorySelection;
                }

                if (IsIdReserved(inventoryDefinition.id))
                {
                    CollectionEditorTools.SetGUIEnabledAtEditorTime(true);
                }
            }
        }

        private void DrawInventoryDetail(InventoryDefinition inventoryDefinition, int index, int count)
        {
            m_DefaultItems = inventoryDefinition.GetDefaultItems();

            DrawItemsInInventory(inventoryDefinition);

            EditorGUILayout.Space();

            DrawItemsNotInInventory(inventoryDefinition);
        }

        private void DrawItemsInInventory(InventoryDefinition inventoryDefinition)
        {
            m_DefaultItemToMoveUp = null;
            m_DefaultItemToMoveDown = null;
            m_DefaultItemToRemove = null;

            var inventoryCatalogAllItemDefinitions = GameFoundationDatabaseSettings.database.inventoryCatalog.GetItemDefinitions();

            var defaultItemsLabel =
                new GUIContent(
                    "Default Items",
                    "When this inventory is instantiated at runtime, these items will be created and added to it automatically.");

            EditorGUILayout.LabelField(defaultItemsLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (new EditorGUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    EditorGUILayout.LabelField("Inventory Item", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField("Quantity", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(80));

                    GUILayout.Space(64);
                }

                if (m_DefaultItems != null && m_DefaultItems.Length > 0)
                {
                    for (var i = 0; i < m_DefaultItems.Length; i++)
                    {
                        var defaultInventoryItem = m_DefaultItems[i];

                        var inventoryItemDefinition = inventoryCatalogAllItemDefinitions.FirstOrDefault(item => item.hash == defaultInventoryItem.definitionHash);

                        if (inventoryItemDefinition == null)
                        {
                            continue;
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(5);

                            EditorGUILayout.LabelField(inventoryItemDefinition.displayName, GUILayout.Width(150));

                            GUILayout.FlexibleSpace();

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(inventoryItemDefinition != null);
                            var quantity = defaultInventoryItem.quantity;
                            quantity = EditorGUILayout.IntField(quantity, GUILayout.Width(80));

                            if (quantity != defaultInventoryItem.quantity)
                            {
                                inventoryDefinition.SetDefaultItemQuantity(defaultInventoryItem, quantity);
                                EditorUtility.SetDirty(inventoryDefinition);
                            }

                            GUILayout.Space(5);

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i < m_DefaultItems.Length - 1);

                            if (GUILayout.Button("\u25BC", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_DefaultItemToMoveDown = defaultInventoryItem;
                                m_DefaultItemToMoveUp = m_DefaultItems[i + 1];
                            }

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i > 0);

                            if (GUILayout.Button("\u25B2", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_DefaultItemToMoveUp = defaultInventoryItem;
                                m_DefaultItemToMoveDown = m_DefaultItems[i - 1];
                            }

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(true);

                            GUILayout.Space(5);

                            if (GUILayout.Button("X", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(18)))
                            {
                                m_DefaultItemToRemove = defaultInventoryItem;
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
                        GUILayout.Label("no default items");
                        GUILayout.FlexibleSpace();
                    }

                    EditorGUILayout.Space();
                }
            }

            if (m_DefaultItemToMoveUp != null && m_DefaultItemToMoveDown != null)
            {
                SwapInventoryItems(inventoryDefinition, m_DefaultItemToMoveUp, m_DefaultItemToMoveDown);
            }

            if (m_DefaultItemToRemove != null)
            {
                if (EditorUtility.DisplayDialog ("Confirm Delete", "Are you sure you want to delete the selected item?", "Yes", "Cancel"))
                {
                    inventoryDefinition.RemoveDefaultItem(m_DefaultItemToRemove);
                    EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.inventoryCatalog);
                }
            }

            if (m_DefaultItemToMoveUp != null || m_DefaultItemToMoveDown != null || m_DefaultItemToRemove != null)
            {
                GUI.FocusControl(null);
            }
        }

        private void DrawItemsNotInInventory(InventoryDefinition inventoryDefinition)
        {
            var inventoryCatalogAllItemDefinitions = GameFoundationDatabaseSettings.database.inventoryCatalog.GetItemDefinitions();

            var otherAvailableItemsLabel =
                new GUIContent(
                    "Other Available Items",
                    "Items that are eligible to be added to this inventory as default items.");

            var otherAvailableWalletItemsLabel =
                new GUIContent(
                    "Other Available Items",
                    "Items that are eligible to be added to this inventory as default items. Only inventory items with the currency detail attached to either themselves or their reference definition are eligible to be added to the Wallet inventory.");

            var otherItemsLabel = (inventoryDefinition.id == InventoryCatalog.k_WalletInventoryDefinitionId)
                ? otherAvailableWalletItemsLabel
                : otherAvailableItemsLabel;

            EditorGUILayout.LabelField(otherItemsLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (new EditorGUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
                {
                    EditorGUILayout.LabelField("Inventory Item", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));
                    GUILayout.FlexibleSpace();
                }

                var validItemCount = 0;

                foreach (var inventoryItemDefinition in inventoryCatalogAllItemDefinitions)
                {
                    // wallets can only have currencies as auto-add items
                    if (inventoryDefinition.id == InventoryCatalog.k_WalletInventoryDefinitionId &&
                        inventoryItemDefinition.GetDetailDefinition<CurrencyDetailDefinition>() == null)
                    {
                        continue;
                    }

                    if (m_DefaultItems.Length > 0 && m_DefaultItems.Any(defaultItem => defaultItem.definitionHash == inventoryItemDefinition.hash))
                    {
                        continue;
                    }

                    validItemCount++;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(5);

                        EditorGUILayout.LabelField(inventoryItemDefinition.displayName);

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Add To Default Items", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(150)))
                        {
                            inventoryDefinition.AddDefaultItem(inventoryItemDefinition);
                            EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.inventoryCatalog);
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
                    GUILayout.Label("no items available");
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();
            }
        }

        private static void SwapInventoryItems(InventoryDefinition inventoryDefinition, DefaultItem defaultItem1, DefaultItem defaultItem2)
        {
            inventoryDefinition.SwapDefaultItemsListOrder(defaultItem1, defaultItem2);
            EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.inventoryCatalog);
        }

        protected override void DrawSidebarListItem(InventoryDefinition item)
        {
            BeginSidebarItem(item, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            if (!IsIdReserved(item.id))
            {
                DrawSidebarItemRemoveButton(item);
            }

            EndSidebarItem();
        }

        protected override void SelectItem(InventoryDefinition inventoryDefinition)
        {
            if (inventoryDefinition != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_InventoryId = inventoryDefinition.id;
                m_DefaultInventoryDefinition = GameFoundationDatabaseSettings.database.inventoryCatalog.GetDefaultCollectionDefinition(inventoryDefinition.id);
                m_CreateDefaultInventory = IsIdReserved(m_InventoryId) || m_DefaultInventoryDefinition != null;
            }

            base.SelectItem(inventoryDefinition);
        }

        protected override void OnRemoveItem(InventoryDefinition inventoryDefinition)
        {
            if (inventoryDefinition == null)
            {
                return;
            }

            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"Inventory Definition {inventoryDefinition.displayName} could not be removed because the Game Foundation database is null");
            }
            else if (GameFoundationDatabaseSettings.database.inventoryCatalog == null)
            {
                Debug.LogError($"Inventory Definition {inventoryDefinition.displayName} could not be removed because the inventory catalog is null");
            }
            else
            {
                if (GameFoundationDatabaseSettings.database.inventoryCatalog.RemoveCollectionDefinition(inventoryDefinition))
                {
                    CollectionEditorTools.AssetDatabaseRemoveObject(inventoryDefinition);
                    EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.inventoryCatalog);
                }
                else
                {
                    Debug.LogError($"Inventory Definition {inventoryDefinition.displayName} was not removed from the inventory catalog.");
                }
            }
        }

        private static bool IsIdReserved(string id)
        {
            return id == InventoryCatalog.k_MainInventoryDefinitionId || id == InventoryCatalog.k_WalletInventoryDefinitionId;
        }
    }
}
