using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class DebugEditorWindow : EditorWindow
    {
        private InventoryTree m_TreeView;
        private SearchField m_SearchField;

        private static int m_AddInventoryOptionsIndex = 0;
        private static int m_AddItemOptionsIndex = 0;
        private static int m_AddStatOptionsIndex = 0;

        private string[] m_AddInventoryOptions = {"Select"};
        private string[] m_AddItemOptions = {"Select"};
        private string[] m_AddStatOptions = {"Select"};

        private int m_LastSelectedTreeViewId = -1;

        private InventoryDefinition[] m_CollectionDefinitions;
        private StatDefinition[] m_StatDefinitions;
        private InventoryItemDefinition[] m_InventoryItemDefinitions;

        private const int m_TreeViewHeightClippingSize = 100000;
        public static void ShowWindow()
        {
            GetWindow<DebugEditorWindow>(false, "Game Foundation Debug", true);
        }

        private void OnEnable()
        {
            MultiColumnHeader state = new MultiColumnHeader(InventoryTree.CreateDefaultMultiColumnHeaderState(position.width));
            m_TreeView = new InventoryTree(null, state);

            m_SearchField = new SearchField();

            m_CollectionDefinitions = GameFoundationSettings.database.inventoryCatalog.GetCollectionDefinitions();
            m_StatDefinitions = GameFoundationSettings.database.statCatalog.GetStatDefinitions();
            m_InventoryItemDefinitions = GameFoundationSettings.database.inventoryCatalog.GetItemDefinitions();
        }

        //TODO: Change to event driven updates. Don't fetch inventory info every frame.
        private void OnGUI()
        {
            if (EditorApplication.isPlaying)
            {
                if (UnityEngine.GameFoundation.GameFoundation.IsInitialized)
                {
                    DrawInventoryCount();
                    DrawSearchBar();
                    DrawTree();
                    DrawAddItem();
                }
                else
                {
                    EditorGUILayout.HelpBox("No Runtime data available! Ensure Game Foundation is Initialized via GameFoundation.Initialize()",
                        MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to start Debugging.", MessageType.Info);
            }
        }

        private void DrawSearchBar()
        {
            string newSearchString = m_SearchField.OnGUI(m_TreeView.SearchString);
            if (newSearchString != m_TreeView.SearchString)
            {
                m_TreeView.SetSelection(new List<int>());
            }
            m_TreeView.SearchString = newSearchString;
        }

        private void DrawInventoryCount()
        {
            EditorGUILayout.LabelField($"{m_TreeView.Inventories.Count} Inventories",
                new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter});
        }

        private void DrawTree()
        {
            Rect rect = GUILayoutUtility.GetRect(0, m_TreeViewHeightClippingSize, 0, m_TreeViewHeightClippingSize);
            m_TreeView.Update();
            m_TreeView.OnGUI(rect);
        }

        private void DrawAddItem()
        {
            int selectedDepth = -1;
            bool walletSelected = false;
            GameItem selectedItem = null;
            IList<int> selected = m_TreeView.GetSelection();
            
            if (selected.Count == 1)
            {
                if (m_TreeView.FindItem(selected[0]) == null)
                {
                    m_TreeView.SetSelection(new List<int>());
                }
                else
                {
                    selectedDepth = m_TreeView.FindItem(selected[0]).depth;
                    walletSelected = m_TreeView.GetGameItem((InventoryTreeItem) m_TreeView.FindItem(selected[0])).id == "wallet";
                    selectedItem = m_TreeView.GetGameItem((InventoryTreeItem)m_TreeView.FindItem(selected[0]));
                }
            }

            //Filter Dropdowns with correct definition ids to add.
            if ( (selected.Count == 1 && m_LastSelectedTreeViewId != selected[0]) ||
                m_LastSelectedTreeViewId == -1)
            {
                //Cannot add main or wallet inventories.
                m_AddInventoryOptions =
                    m_CollectionDefinitions?.Select(definition => definition.id)
                        .Where(id => !id.Equals("main") && !id.Equals("wallet")).Prepend("Select").ToArray() ??
                    new[] {"Select"};
                
                //If wallet inventory is selected make sure the definition has a Currency Detail Definition
                m_AddItemOptions =
                    m_InventoryItemDefinitions
                        ?.Where(inventoryItemDefinition => !walletSelected || inventoryItemDefinition.GetDetailDefinition<CurrencyDetailDefinition>() != null)
                        .Select(inventoryItemDefinition => inventoryItemDefinition.id).Prepend("Select").ToArray() ?? new[] {"Select"};

                //Ensure selected Item doesn't have Stat we are adding.
                m_AddStatOptions = m_StatDefinitions?.Select(definition => definition.id)
                                                     .Where(id => selectedItem == null || (!StatManager.HasIntValue(selectedItem,id) && !StatManager.HasFloatValue(selectedItem,id))).Prepend("Select").ToArray() ??
                                   new[] {"Select"};

                //Only update dropdowns when different Items are selected
                if (m_TreeView.GetSelection().Count > 0)
                {
                    m_LastSelectedTreeViewId = m_TreeView.GetSelection()[0];
                    m_AddInventoryOptionsIndex = m_AddItemOptionsIndex = m_AddStatOptionsIndex = 0;
                }
            }

            string idToAdd = "";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            m_AddInventoryOptionsIndex = EditorGUILayout.Popup(m_AddInventoryOptionsIndex, m_AddInventoryOptions);
            //Can't add unselected definition "Select"
            EditorGUI.BeginDisabledGroup(m_AddInventoryOptionsIndex == 0);
            if (GUILayout.Button("Add Inventory", new GUIStyle(GUI.skin.button) {fixedWidth = position.width / 3 - 5}))
            {
                idToAdd = m_AddInventoryOptions[m_AddInventoryOptionsIndex];
                InventoryManager.CreateInventory(idToAdd, InventoryManager.GetNewInventoryId());
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            
            //Can't add Item if Inventory not selected
            EditorGUI.BeginDisabledGroup(selectedDepth != 0);
            EditorGUILayout.BeginVertical();
            m_AddItemOptionsIndex = EditorGUILayout.Popup(m_AddItemOptionsIndex, m_AddItemOptions);
            
            //Can't add unselected definition "Select"
            EditorGUI.BeginDisabledGroup(m_AddItemOptionsIndex == 0);
            
            if (GUILayout.Button("Add Item", new GUIStyle(GUI.skin.button) {fixedWidth = position.width / 3 - 5}))
            {
                idToAdd = m_AddItemOptions[m_AddItemOptionsIndex];
                InventoryManager
                .GetInventory(((InventoryTreeItem) m_TreeView.FindItem(selected[0])).itemHash)
                .AddItem(idToAdd, 1);

                //Expand inventory to see change.
                m_TreeView.SetExpanded(selected[0],true);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            
            //Can't add Stat if Item is not selected
            EditorGUI.BeginDisabledGroup(selectedDepth != 1);
            EditorGUILayout.BeginVertical();
            m_AddStatOptionsIndex = EditorGUILayout.Popup(m_AddStatOptionsIndex, m_AddStatOptions);

            //Can't add unselected definition "Select"
            EditorGUI.BeginDisabledGroup(m_AddStatOptionsIndex == 0);
            if (GUILayout.Button("Add Stat", new GUIStyle(GUI.skin.button) {fixedWidth = position.width / 3 - 5}))
            {
                idToAdd = m_AddStatOptions[m_AddStatOptionsIndex];
                
                StatDefinition statDefinition =
                    GameFoundationSettings.database.statCatalog.GetStatDefinition(idToAdd);
                InventoryItem inventoryItem =
                    m_TreeView.GetGameItem((InventoryTreeItem) m_TreeView.FindItem(selected[0])) as
                        InventoryItem;
                if (statDefinition.statValueType == StatDefinition.StatValueType.Int)
                {
                    if (!StatManager.HasIntValue(inventoryItem, idToAdd))
                    {
                        StatManager.SetIntValue(inventoryItem, idToAdd, 0);
                    }
                    else
                    {
                        StatManager.SetIntValue(inventoryItem, idToAdd,
                            StatManager.GetIntValue(inventoryItem, idToAdd) + 1);
                    }
                }
                else
                {
                    if (!StatManager.HasFloatValue(inventoryItem, idToAdd))
                    {
                        StatManager.SetFloatValue(inventoryItem, idToAdd, 0);
                    }
                    else
                    {
                        StatManager.SetFloatValue(inventoryItem, idToAdd,
                            StatManager.GetFloatValue(inventoryItem, idToAdd) + 1);
                    }
                }
                //Expand inventory to see item and stat.
                m_TreeView.SetExpandedRecursive(m_TreeView.FindItem(selected[0]).parent.id,true);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        internal static void ClearIndexes()
        {
            m_AddItemOptionsIndex = 0;
            m_AddInventoryOptionsIndex = 0;
            m_AddStatOptionsIndex = 0;
        }
    }
}
