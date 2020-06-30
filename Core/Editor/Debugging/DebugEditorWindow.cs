using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation.Debugging
{
    class DebugEditorWindow : EditorWindow
    {
        const int k_TreeViewHeightClippingSize = 100000;

        InventoryTree m_TreeView;
        SearchField m_SearchField;

        int m_AddItemOptionsIndex;

        string[] m_AddItemOptions = { "Select" };

        int m_LastSelectedTreeViewId = -1;

        InventoryItemDefinition[] m_InventoryItemDefinitions;

        /// <summary>
        ///     Draw action of this window's current state.
        /// </summary>
        Action m_CurrentStateDraw;

        public static void ShowWindow()
        {
            GetWindow<DebugEditorWindow>(false, "Game Foundation Debug", true);
        }

        void OnEnable()
        {
            var header = new MultiColumnHeader(InventoryTree.CreateDefaultMultiColumnHeaderState(position.width));
            m_TreeView = new InventoryTree(this, null, header);

            m_SearchField = new SearchField();

            UpdateState();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            UnityEngine.GameFoundation.GameFoundation.initialized += UpdateState;
            UnityEngine.GameFoundation.GameFoundation.uninitialized += UpdateState;

            m_TreeView.AttachListeners();
        }

        void OnDisable()
        {
            m_TreeView.DetachListeners();

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEngine.GameFoundation.GameFoundation.initialized -= UpdateState;
            UnityEngine.GameFoundation.GameFoundation.uninitialized -= UpdateState;
        }

        void OnGUI()
        {
            m_CurrentStateDraw();
        }

        /// <summary>
        ///     Determine the current state of this window based on the current context.
        ///     The state determines what to draw.
        ///     Also Reload the tree view.
        /// </summary>
        void UpdateState()
        {
            if (!EditorApplication.isPlaying)
            {
                m_CurrentStateDraw = DrawEditMode;

                return;
            }

            if (!UnityEngine.GameFoundation.GameFoundation.IsInitialized)
            {
                m_CurrentStateDraw = DrawPlayModeGameFoundationNotInitialized;

                return;
            }

            var catalogs = UnityEngine.GameFoundation.GameFoundation.catalogs;
            m_InventoryItemDefinitions = catalogs.inventoryCatalog.GetItems();

            m_CurrentStateDraw = DrawPlayModeGameFoundationInitialized;

            m_TreeView.Reload();
        }

        static void DrawEditMode()
        {
            EditorGUILayout.HelpBox("Enter Play Mode to start Debugging.", MessageType.Info);
        }

        static void DrawPlayModeGameFoundationNotInitialized()
        {
            EditorGUILayout.HelpBox(
                "No Runtime data available! Ensure Game Foundation is Initialized via GameFoundation.Initialize()",
                MessageType.Error);
        }

        void DrawPlayModeGameFoundationInitialized()
        {
            DrawInventoryItemCount();
            DrawSearchBar();
            DrawTree();
            DrawAddItem();
        }

        void DrawSearchBar()
        {
            var newSearchString = m_SearchField.OnGUI(m_TreeView.SearchString);
            if (newSearchString != m_TreeView.SearchString)
            {
                m_TreeView.SearchString = newSearchString;
                m_TreeView.SetSelection(new List<int>());
            }
        }

        void DrawInventoryItemCount()
        {
            EditorGUILayout.LabelField($"{m_TreeView.itemCount} Items",
                new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
        }

        void DrawTree()
        {
            var rect = GUILayoutUtility.GetRect(0, k_TreeViewHeightClippingSize, 0, k_TreeViewHeightClippingSize);
            m_TreeView.OnGUI(rect);
        }

        void DrawAddItem()
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
            m_AddItemOptions = m_InventoryItemDefinitions
                ?
                .Select(definition => definition.key)
                .Prepend("Select")
                .ToArray() ?? new[] { "Select" };

            //Filter Dropdowns with correct definition ids to add.
            if (selected.Count == 1 && m_LastSelectedTreeViewId != selected[0] ||
                m_LastSelectedTreeViewId == -1)
            {
                //Only update dropdowns when different Items are selected
                if (m_TreeView.GetSelection().Count > 0)
                {
                    m_LastSelectedTreeViewId = m_TreeView.GetSelection()[0];
                }
            }

            //Can't add Item if Inventory not selected
            using (new EditorGUILayout.HorizontalScope())
            using (new EditorGUILayout.VerticalScope())
            {
                m_AddItemOptionsIndex = EditorGUILayout.Popup(m_AddItemOptionsIndex, m_AddItemOptions);

                //Can't add unselected definition "Select"
                using (new EditorGUI.DisabledScope(m_AddItemOptionsIndex == 0))
                {
                    if (GUILayout.Button("Add Item", new GUIStyle(GUI.skin.button) { fixedWidth = position.width / 3 - 5 }))
                    {
                        var idToAdd = m_AddItemOptions[m_AddItemOptionsIndex];
                        InventoryManager.CreateItem(idToAdd);
                    }
                }
            }
        }

        internal void ClearIndexes()
        {
            m_AddItemOptionsIndex = 0;
        }

        /// <summary>
        ///     Callback to <see cref="EditorApplication.playModeStateChanged"/>.
        ///     Update this window's state.
        /// </summary>
        void OnPlayModeStateChanged(PlayModeStateChange _)
        {
            UpdateState();
        }
    }
}
