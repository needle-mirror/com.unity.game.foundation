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

        private static int m_AddItemOptionsIndex;

        private string[] m_AddItemOptions = {"Select"};

        private int m_LastSelectedTreeViewId = -1;

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

            GetCatalogDefinitions();
        }

        //TODO: Change to event driven updates. Don't fetch inventory info every frame.
        private void OnGUI()
        {
            if (EditorApplication.isPlaying)
            {
                if (UnityEngine.GameFoundation.GameFoundation.IsInitialized)
                {
                    if (m_StatDefinitions == null || m_InventoryItemDefinitions == null)
                    {
                        GetCatalogDefinitions();
                    }

                    DrawInventoryItemCount();
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

        private void GetCatalogDefinitions()
        {
            var catalogs = UnityEngine.GameFoundation.GameFoundation.catalogs;
            m_StatDefinitions = catalogs?.statCatalog?.GetStatDefinitions();
            m_InventoryItemDefinitions = catalogs?.inventoryCatalog?.GetItems();
        }

        private void DrawSearchBar()
        {
            string newSearchString = m_SearchField.OnGUI(m_TreeView.SearchString);
            if (newSearchString != m_TreeView.SearchString)
            {
                m_TreeView.SearchString = newSearchString;
                m_TreeView.SetSelection(new List<int>());
            }
        }

        private void DrawInventoryItemCount()
        {
            EditorGUILayout.LabelField($"{m_TreeView.itemCount} Items",
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
            IList<int> selected = m_TreeView.GetSelection();
            
            if (selected.Count == 1)
            {
                if (m_TreeView.FindItem(selected[0]) == null)
                {
                    m_TreeView.SetSelection(new List<int>());
                }
            }

            //If wallet inventory is selected make sure the definition has a Currency Detail Definition
            m_AddItemOptions =
                m_InventoryItemDefinitions
                    ?
                    .Select(definition => definition.id)
                    .Prepend("Select")
                    .ToArray() ?? new[] { "Select" };

            //Filter Dropdowns with correct definition ids to add.
            if ( (selected.Count == 1 && m_LastSelectedTreeViewId != selected[0]) ||
                m_LastSelectedTreeViewId == -1)
            {
                //Only update dropdowns when different Items are selected
                if (m_TreeView.GetSelection().Count > 0)
                {
                    m_LastSelectedTreeViewId = m_TreeView.GetSelection()[0];
                }
            }

            string idToAdd = "";
            EditorGUILayout.BeginHorizontal();
            
            //Can't add Item if Inventory not selected
            EditorGUILayout.BeginVertical();
            m_AddItemOptionsIndex = EditorGUILayout.Popup(m_AddItemOptionsIndex, m_AddItemOptions);
            
            //Can't add unselected definition "Select"
            EditorGUI.BeginDisabledGroup(m_AddItemOptionsIndex == 0);
            
            if (GUILayout.Button("Add Item", new GUIStyle(GUI.skin.button) {fixedWidth = position.width / 3 - 5}))
            {
                idToAdd = m_AddItemOptions[m_AddItemOptionsIndex];
                InventoryManager.CreateItem(idToAdd);
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        internal static void ClearIndexes()
        {
            m_AddItemOptionsIndex = 0;
        }
    }
}
