using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class InventoryDefinitionEditor : CollectionEditorBase<InventoryDefinition>
    {
        private string m_InventoryId = null;
        private DefaultItem[] m_DefaultItems = null;
        private DefaultItem m_DefaultItemToMoveDown = null;
        private DefaultItem m_DefaultItemToMoveUp = null;
        private DefaultItem m_DefaultItemToRemove;
        private DefaultCollectionDefinition m_DefaultInventoryDefinition;
        private bool m_CreateDefaultInventory;
        private static GUIContent s_AutoCreateInventoryLabel = new GUIContent("Auto Create Instance", "When checked, an instance of this InventoryDefinition will automatically get instantiated at runtime when Game Foundation initializes.");
        private static GUIContent s_DefaultItemsLabel = new GUIContent("Default Items", "When this inventory is instantiated at runtime, these items will be created and added to it automatically.");
        private static GUIContent s_OtherAvailableItemsLabel = new GUIContent("Other Available Items", "Items that are eligible to be added to this inventory as a default item.");
        private static GUIContent s_OtherAvailableWalletItemsLabel = new GUIContent("Other Available Items", "Items that are eligible to be added to this inventory as a default item. Only inventory items with the currency detail attached to either themselves or their reference definition are eligible to be added to the Wallet inventory.");

        public InventoryDefinitionEditor(string name, InventoryEditorWindow window) : base(name, window)
        {
        }
        
        protected override List<InventoryDefinition> GetFilteredItems()
        {
            return GetItems();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            if (GameFoundationSettings.database != null
                && GameFoundationSettings.database.inventoryCatalog != null)
            {
                GameFoundationSettings.database.inventoryCatalog.GetCollectionDefinitions(GetItems());
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
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Could not create new inventory definition because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                Debug.LogError("Could not create new inventory definition because the inventory catalog is null.");
                return;
            }

            InventoryDefinition inventory = InventoryDefinition.Create(m_NewItemId, m_NewItemDisplayName);

            if (inventory != null)
            {
                AddItem(inventory);
                CollectionEditorTools.AssetDatabaseAddObject(inventory, GameFoundationSettings.database.inventoryCatalog);
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
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Inventory Definition " + inventoryDefinition.displayName + " could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                Debug.LogError("Inventory Definition " + inventoryDefinition.displayName + " could not be added because the inventory catalog is null");
            }
            else
            {
                GameFoundationSettings.database.inventoryCatalog.AddCollectionDefinition(inventoryDefinition);
                EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
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
            if (GameFoundationSettings.database == null
                || GameFoundationSettings.database.inventoryCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = inventoryDefinition.displayName;
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

                bool newAutoCreateInventorySelection = EditorGUILayout.Toggle(s_AutoCreateInventoryLabel, m_CreateDefaultInventory);
                if (newAutoCreateInventorySelection != m_CreateDefaultInventory)
                {
                    if (newAutoCreateInventorySelection)
                    {
                        m_DefaultInventoryDefinition = new DefaultCollectionDefinition(inventoryDefinition.id, inventoryDefinition.displayName, inventoryDefinition.hash);
                        GameFoundationSettings.database.inventoryCatalog.AddDefaultCollectionDefinition(m_DefaultInventoryDefinition);
                    }
                    else
                    {
                        GameFoundationSettings.database.inventoryCatalog.RemoveDefaultCollectionDefinition(m_DefaultInventoryDefinition);
                        m_DefaultInventoryDefinition = null;
                    }
                    EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
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

            var inventoryCatalogAllItemDefinitions = GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions();

            EditorGUILayout.LabelField(s_DefaultItemsLabel, GameFoundationEditorStyles.titleStyle);

            EditorGUILayout.BeginVertical(GameFoundationEditorStyles.boxStyle);

            GUILayout.BeginHorizontal(GameFoundationEditorStyles.tableViewToolbarStyle);
            EditorGUILayout.LabelField("Inventory Item", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Quantity", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(80));
            GUILayout.Space(64);
            GUILayout.EndHorizontal();

            if (m_DefaultItems != null && m_DefaultItems.Count() > 0)
            {
                for (int i = 0; i < m_DefaultItems.Count(); i++)
                {
                    DefaultItem defaultInventoryItem = m_DefaultItems[i];
                    var inventoryItemDefinition = inventoryCatalogAllItemDefinitions.FirstOrDefault(item => item.hash == defaultInventoryItem.definitionHash);
                    if (inventoryItemDefinition != null)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(5);

                            EditorGUILayout.LabelField(inventoryItemDefinition.displayName, GUILayout.Width(150));

                            GUILayout.FlexibleSpace();

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(inventoryItemDefinition != null);
                            int quantity = defaultInventoryItem.quantity;
                            quantity = EditorGUILayout.IntField(quantity, GUILayout.Width(80));

                            if (quantity != defaultInventoryItem.quantity)
                            {
                                inventoryDefinition.SetDefaultItemQuantity(defaultInventoryItem, quantity);
                                EditorUtility.SetDirty(inventoryDefinition);
                            }

                            GUILayout.Space(5);

                            CollectionEditorTools.SetGUIEnabledAtEditorTime(i < m_DefaultItems.Count() - 1);
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

            EditorGUILayout.EndVertical();

            if (m_DefaultItemToMoveUp != null && m_DefaultItemToMoveDown != null)
            {
                SwapInventoryItems(inventoryDefinition, m_DefaultItemToMoveUp, m_DefaultItemToMoveDown);
            }

            if (m_DefaultItemToRemove != null)
            {
                if (EditorUtility.DisplayDialog ("Confirm Delete", "Are you sure you want to delete the selected item?", "Yes", "Cancel"))
                {
                    inventoryDefinition.RemoveDefaultItem(m_DefaultItemToRemove);
                    EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                }
            }

            if (m_DefaultItemToMoveUp != null || m_DefaultItemToMoveDown != null || m_DefaultItemToRemove != null)
            {
                ClearFocus();
            }
        }

        private void DrawItemsNotInInventory(InventoryDefinition inventoryDefinition)
        {
            var inventoryCatalogAllItemDefinitions = GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions();

            GUIContent otherItemsLabel = (inventoryDefinition.id == InventoryCatalog.k_WalletInventoryDefinitionId)
                ? s_OtherAvailableWalletItemsLabel
                : s_OtherAvailableItemsLabel;

            EditorGUILayout.LabelField(otherItemsLabel, GameFoundationEditorStyles.titleStyle);

            EditorGUILayout.BeginVertical(GameFoundationEditorStyles.boxStyle);
            GUILayout.BeginHorizontal(GameFoundationEditorStyles.tableViewToolbarStyle);
            EditorGUILayout.LabelField("Inventory Item", GameFoundationEditorStyles.tableViewToolbarTextStyle, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            int validItemCount = 0;

            foreach (InventoryItemDefinition inventoryItemDefinition in inventoryCatalogAllItemDefinitions)
            {
                // wallets can only have currencies as auto-add items
                if (inventoryDefinition.id == InventoryCatalog.k_WalletInventoryDefinitionId &&
                    inventoryItemDefinition.GetDetailDefinition<CurrencyDetailDefinition>() == null)
                {
                    continue;
                }

                if (m_DefaultItems.Count() > 0 && m_DefaultItems.Any(defaultItem => defaultItem.definitionHash == inventoryItemDefinition.hash))
                {
                    continue;
                }

                validItemCount++;

                GUILayout.BeginHorizontal();

                GUILayout.Space(5);

                EditorGUILayout.LabelField(inventoryItemDefinition.displayName);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add To Default Items", GameFoundationEditorStyles.tableViewButtonStyle, GUILayout.Width(150)))
                {
                    inventoryDefinition.AddDefaultItem(inventoryItemDefinition);
                    EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                }

                GUILayout.EndHorizontal();
            }

            if (validItemCount <= 0)
            {
                EditorGUILayout.Space();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("no items available");
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndVertical();
        }

        private void SwapInventoryItems(InventoryDefinition inventoryDefinition, DefaultItem defaultItem1, DefaultItem defaultItem2)
        {
            inventoryDefinition.SwapDefaultItemsListOrder(defaultItem1, defaultItem2);
            EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
        }

        protected override void DrawSidebarListItem(InventoryDefinition item, int index)
        {
            BeginSidebarItem(item, index, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            if (!IsIdReserved(item.id))
            {
                DrawSidebarItemRemoveButton(item);
            }

            EndSidebarItem(item, index);
        }

        protected override void SelectItem(InventoryDefinition inventoryDefinition)
        {
            if (inventoryDefinition != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_InventoryId = inventoryDefinition.id;
                m_DefaultInventoryDefinition = GameFoundationSettings.database.inventoryCatalog.GetDefaultCollectionDefinition(inventoryDefinition.id);
                m_CreateDefaultInventory = IsIdReserved(m_InventoryId) || m_DefaultInventoryDefinition != null;
            }

            base.SelectItem(inventoryDefinition);
        }

        protected override void OnRemoveItem(InventoryDefinition inventoryDefinition)
        {
            if (inventoryDefinition != null)
            {
                if (GameFoundationSettings.database == null)
                {
                    Debug.LogError("Inventory Definition " + inventoryDefinition.displayName + " could not be removed because the Game Foundation database is null");
                }
                else if (GameFoundationSettings.database.inventoryCatalog == null)
                {
                    Debug.LogError("Inventory Definition " + inventoryDefinition.displayName + " could not be removed because the inventory catalog is null");
                }
                else
                {
                    if (GameFoundationSettings.database.inventoryCatalog.RemoveCollectionDefinition(inventoryDefinition))
                    {
                        CollectionEditorTools.AssetDatabaseRemoveObject(inventoryDefinition);
                        EditorUtility.SetDirty(GameFoundationSettings.database.inventoryCatalog);
                    }
                    else
                    {
                        Debug.LogError("Inventory Definition " + inventoryDefinition.displayName + " was not removed from the inventory catalog.");
                    }
                }
            }
        }

        protected static bool IsIdReserved(string id)
        {
            return id == InventoryCatalog.k_MainInventoryDefinitionId || id == InventoryCatalog.k_WalletInventoryDefinitionId;
        }
    }
}
