using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation.Debugging
{
    class InventoryTree : TreeView
    {
        enum Columns
        {
            Items,
            Value,
            Action,
        }

        static readonly Texture2D k_InventoryItemIcon = EditorGUIUtility.FindTexture("Prefab Icon");

        static readonly Texture2D k_PropertyItemIcon = EditorGUIUtility.FindTexture("GameManager Icon");

        /// <summary>
        ///     Contain all tree view item holding data about GameFoundation items.
        ///     Utility nodes like "Wallet" or "Items" are not included.
        /// </summary>
        List<TreeViewItem> m_AllTreeViewItems = new List<TreeViewItem>();

        IList<int> m_ExpandedIdsBeforeSearch;

        string m_SearchString = string.Empty;

        public string SearchString
        {
            get => m_SearchString;
            set
            {
                if (m_SearchString != value)
                {
                    m_SearchString = value;
                    Reload();
                }
            }
        }

        List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        public int itemCount => m_InventoryItems.Count;

        DebugEditorWindow m_Owner;

        public InventoryTree(DebugEditorWindow owner, TreeViewState state = null, MultiColumnHeader multiColumnHeader = null)
            : base(state ?? new TreeViewState(), multiColumnHeader)
        {
            m_Owner = owner;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            //Reset AddItem PopUp Window Index when something else is selected
            m_Owner.ClearIndexes();

            base.SelectionChanged(selectedIds);
        }

        protected override void DoubleClickedItem(int id)
        {
            //Ensure only one row is selected at a time.
            state.selectedIDs.Clear();
            state.selectedIDs.Add(id);
        }

        protected override void SingleClickedItem(int id)
        {
            //Ensure only one row is selected at a time.
            state.selectedIDs.Clear();
            state.selectedIDs.Add(id);
        }

        protected override TreeViewItem BuildRoot()
        {
            var inventoryRoot = GenerateInventoryTreeRoot();

            //Filter generated root according to search and save previous expanded state.
            if (!string.IsNullOrEmpty(SearchString))
            {
                if (m_ExpandedIdsBeforeSearch == null)
                {
                    m_ExpandedIdsBeforeSearch = GetExpanded();
                }

                FilterRootOnSearch(inventoryRoot, SearchString, m_AllTreeViewItems);
            }
            else if (m_ExpandedIdsBeforeSearch != null)
            {
                SetExpanded(m_ExpandedIdsBeforeSearch);
                m_ExpandedIdsBeforeSearch = null;
            }

            if (!inventoryRoot.hasChildren)
            {
                inventoryRoot.AddChild(new TreeViewItem());
            }

            return inventoryRoot;
        }

        //Called when drawing the rows of the Tree View
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;
            if (item is null) return;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i));
            }
        }

        //This function processes a single cell based on the column its in and renders it in a different way.
        void CellGUI(Rect cellRect, TreeViewItem viewItem, Columns column)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case Columns.Items:
                {
                    ItemColumnGUI(cellRect, viewItem);
                    break;
                }
                case Columns.Value:
                {
                    ValueColumnGUI(cellRect, viewItem);
                    break;
                }
                case Columns.Action:
                {
                    ActionColumnGUI(cellRect, viewItem);
                    break;
                }
            }
        }

        void ActionColumnGUI(Rect cellRect, TreeViewItem viewItem)
        {
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 50,
                alignment = TextAnchor.MiddleCenter
            };
            var centeredButtonPosition = cellRect;
            centeredButtonPosition.x += cellRect.width / 2 - buttonStyle.fixedWidth / 2;

            switch (viewItem)
            {
                case InventoryItemView itemView:
                {
                    var clicked = GUI.Button(centeredButtonPosition, "Delete", buttonStyle);

                    if (clicked)
                    {
                        HandleSelectionRemoved(viewItem);

                        //Get Inventory from tree item parent to get InventoryItem
                        var inventoryItem = itemView.inventoryItem;
                        InventoryManager.RemoveItem(inventoryItem);
                        CorrectFoldouts(viewItem);
                    }

                    break;
                }

                case PropertyView propertyView:
                {
                    var clicked = GUI.Button(centeredButtonPosition, "Reset", buttonStyle);

                    if (clicked)
                    {
                        var inventoryItem = propertyView.inventoryItem;
                        var definition = propertyView.property;
                        inventoryItem.ResetProperty(definition.key);
                        CorrectFoldouts(viewItem);
                    }

                    break;
                }
            }
        }

        void ValueColumnGUI(Rect cellRect, TreeViewItem viewItem)
        {
            var guiStyle = new GUIStyle(IsSelected(viewItem.id) ? GUI.skin.textField : GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            //Bold Item Quantities
            if (viewItem is InventoryItemView)
            {
                guiStyle.fontStyle = FontStyle.Bold;
            }

            switch (viewItem)
            {
                case PropertyView propertyView:
                    try
                    {
                        var inventoryItem = propertyView.inventoryItem;
                        var definition = propertyView.property;

                        var property = inventoryItem.GetProperty(definition.key);
                        Property newValue = default;

                        switch (definition.value.type)
                        {
                            case PropertyType.Long:
                            {
                                newValue = EditorGUI.LongField(cellRect, property, guiStyle);

                                break;
                            }

                            case PropertyType.Double:
                            {
                                newValue = EditorGUI.DoubleField(cellRect, property, guiStyle);

                                break;
                            }

                            case PropertyType.Bool:
                            {
                                newValue = EditorGUI.Toggle(cellRect, property, guiStyle);

                                break;
                            }

                            case PropertyType.String:
                            {
                                newValue = EditorGUI.TextField(cellRect, property, guiStyle);

                                break;
                            }

                            default:
                                throw new ArgumentOutOfRangeException(
                                    $"{definition.value.type} isn't handled.");
                        }

                        if (newValue != property)
                            inventoryItem.SetProperty(definition.key, newValue);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError(e.StackTrace);
                    }

                    break;
                case CurrencyView currencyView:
                    try
                    {
                        var currency = currencyView.currency;

                        var balance = WalletManager.GetBalance(currency);

                        var newValue = EditorGUI.LongField(cellRect, balance, guiStyle);
                        if (newValue != balance)
                        {
                            var done = WalletManager.SetBalance(currency, newValue);
                            if (!done)
                            {
                                Debug.LogError("Cannot change the currency");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError(e.StackTrace);
                    }

                    break;
            }
        }

        void ItemColumnGUI(Rect cellRect, TreeViewItem viewItem)
        {
            //Make Room for Icon between Arrow and Label
            Rect tempRect = cellRect;
            tempRect.x += GetContentIndent(viewItem);
            tempRect.width = 16f;

            //Get clipping width for cell and remaining column width. 
            tempRect.width = cellRect.width - GetContentIndent(viewItem);

            var labelText = viewItem.displayName;

            GUI.Label(tempRect, labelText, DefaultStyles.label);
        }

        void FilterRootOnSearch(TreeViewItem root, string search, IEnumerable<TreeViewItem> allItems)
        {
            var foundItems = new List<TreeViewItem>();
            foreach (var item in allItems)
            {
                var searchableString = GetSearchableString(item);
                if (searchableString.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    foundItems.Add(item);
            }

            var newExpandedState = new HashSet<int>();
            foreach (var foundItem in foundItems)
            {
                newExpandedState.UnionWith(AddItemAndItsAncestorsIDs(foundItem));
            }

            state.expandedIDs = new List<int>(newExpandedState);
            state.expandedIDs.Sort();

            RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(root, foundItems);
        }

        void RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(TreeViewItem item, List<TreeViewItem> foundItems)
        {
            if (!item.hasChildren)
                return;

            for (var i = item.children.Count - 1; i >= 0; i--)
            {
                var child = item.children[i];
                if (child.hasChildren)
                {
                    if (!IsExpanded(child.id))
                        item.children.RemoveAt(i);
                    else
                    {
                        // remove collapsed items
                        RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(child, foundItems);
                    }
                }
                else if (!foundItems.Contains(child))
                {
                    // remove leaf items not matching search result
                    item.children.RemoveAt(i);
                }
            }
        }

        static IEnumerable<int> AddItemAndItsAncestorsIDs(TreeViewItem item)
        {
            var results = new List<int> { item.id };
            var cur = item;
            while (cur.parent != null)
            {
                results.Add(cur.parent.id);
                cur = cur.parent;
            }

            return results;
        }

        void HandleSelectionRemoved(TreeViewItem item)
        {
            var selectedIds = new List<int>(GetSelection());
            selectedIds.Remove(item.id);

            foreach (var id in selectedIds)
            {
                if (FindItem(id) == null)
                {
                    selectedIds.Remove(id);
                }
            }

            SetSelection(selectedIds);
        }

        /// <summary>
        ///     Correct expanded foldouts state when removing a row.
        /// </summary>
        /// <param name="deletedItem">
        ///     The removed row.
        /// </param>
        void CorrectFoldouts(TreeViewItem deletedItem)
        {
            int GetLastId(TreeViewItem item)
            {
                while (item.hasChildren)
                {
                    item = item.children[item.children.Count - 1];
                }

                return item.id;
            }

            var lastIdInChildren = GetLastId(deletedItem);
            var lastRemovedIndexInExpandedIds = deletedItem.id;
            for (var i = deletedItem.id; i <= lastIdInChildren; i++)
            {
                if (state.expandedIDs.Remove(i))
                {
                    lastRemovedIndexInExpandedIds = i;
                }
            }

            var removeOffset = lastIdInChildren - deletedItem.id + 1;
            for (var i = 0; i < state.expandedIDs.Count; i++)
            {
                if (state.expandedIDs[i] > lastRemovedIndexInExpandedIds)
                    state.expandedIDs[i] -= removeOffset;
            }
        }

        TreeViewItem GenerateInventoryTreeRoot()
        {
            var id = 0;
            m_AllTreeViewItems.Clear();
            var rootView = new TreeViewItem(id++, -1, "Root");

            var currencyCatalog = UnityEngine.GameFoundation.GameFoundation.catalogs.currencyCatalog;

            var currenciesView = new TreeViewItem(id++, 0, "Wallet");
            rootView.AddChild(currenciesView);

            foreach (var currency in currencyCatalog.GetItems())
            {
                var currencyView = new CurrencyView(id++, 1, $"{currency.displayName}", currency);

                currenciesView.AddChild(currencyView);
            }

            var itemsView = new TreeViewItem(id++, 0, "Inventory");
            rootView.AddChild(itemsView);

            InventoryManager.GetItems(m_InventoryItems);

            var itemDefinitionNodes = new Dictionary<string, TreeViewItem>(m_InventoryItems.Count);
            foreach (var item in m_InventoryItems)
            {
                if (!itemDefinitionNodes.TryGetValue(item.definition.key, out var definitionNode))
                {
                    definitionNode = new InventoryItemDefinitionView(
                        id++,
                        itemsView.depth + 1,
                        item.definition);

                    itemsView.AddChild(definitionNode);

                    itemDefinitionNodes[item.definition.key] = definitionNode;
                    m_AllTreeViewItems.Add(definitionNode);
                }

                var itemView = new InventoryItemView(
                    id++,
                    definitionNode.depth + 1,
                    item);

                definitionNode.AddChild(itemView);

                itemView.icon = k_InventoryItemIcon;
                m_AllTreeViewItems.Add(itemView);

                var properties = item.definition.defaultProperties;
                if (properties == null)
                    continue;

                foreach (var propertyEntry in properties)
                {
                    var propertyView = new PropertyView(
                        id++,
                        itemView.depth + 1,
                        propertyEntry.Key,
                        item,
                        (propertyEntry.Key, propertyEntry.Value));

                    itemView.AddChild(propertyView);
                    propertyView.icon = k_PropertyItemIcon;
                    m_AllTreeViewItems.Add(propertyView);
                }
            }

            return rootView;
        }

        public TreeViewItem FindItem(int id)
        {
            return base.FindItem(id, rootItem);
        }

        internal void AttachListeners()
        {
            UnityEngine.GameFoundation.GameFoundation.initialized += Reload;
            InventoryManager.itemAdded += OnItemAddedOrRemoved;
            InventoryManager.itemRemoved += OnItemAddedOrRemoved;
            WalletManager.balanceChanged += OnBalanceChanged;
        }

        internal void DetachListeners()
        {
            UnityEngine.GameFoundation.GameFoundation.initialized -= Reload;
            InventoryManager.itemAdded -= OnItemAddedOrRemoved;
            InventoryManager.itemRemoved -= OnItemAddedOrRemoved;
            WalletManager.balanceChanged -= OnBalanceChanged;
        }

        /// <summary>
        ///     Callback to <see cref="InventoryManager.itemAdded"/>
        ///     and <see cref="InventoryManager.itemRemoved"/>.
        ///     Reload this tree.
        /// </summary>
        void OnItemAddedOrRemoved(InventoryItem _)
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
        }

        /// <summary>
        ///     Callback to <see cref="WalletManager.balanceChanged"/>.
        ///     Reload this tree.
        /// </summary>
        void OnBalanceChanged(BalanceChangedEventArgs _)
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
        }

        /// <summary>
        ///     Get a string from the given <paramref name="itemView"/> to compare to the researched user string.
        /// </summary>
        static string GetSearchableString(TreeViewItem itemView)
        {
            switch (itemView)
            {
                case InventoryItemDefinitionView inventoryItemDefinitionView:
                {
                    return inventoryItemDefinitionView.definition.key;
                }

                case InventoryItemView inventoryItemView:
                {
                    var inventoryItem = inventoryItemView.inventoryItem;
                    return $"{inventoryItem.definition.key} #{inventoryItem.id}";
                }

                case PropertyView propertyView:
                {
                    return propertyView.property.key;
                }

                default:
                    throw new ArgumentException("Cannot get real display name of this InventoryTreeItem, bad depth parameter.");
            }
        }

        //Used by Debug Editor Window
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    contextMenuText = "Items",
                    headerTextAlignment = TextAlignment.Center,
                    width = treeViewWidth - 130,
                    autoResize = true,
                    allowToggleVisibility = true,
                    canSort = false,
                },
                new MultiColumnHeaderState.Column
                {
                    contextMenuText = "Value",
                    headerContent = new GUIContent("Value"),
                    width = 50,
                    headerTextAlignment = TextAlignment.Center,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    contextMenuText = "Trigger action on Item",
                    headerContent = new GUIContent("Actions"),
                    width = 70,
                    headerTextAlignment = TextAlignment.Center,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
            };
            return new MultiColumnHeaderState(columns);
        }
    }
}
