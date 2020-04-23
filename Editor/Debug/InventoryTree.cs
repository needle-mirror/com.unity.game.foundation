using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class InventoryTree : TreeView
    {
        private enum Columns
        {
            Items,
            Value,
            Action,
        }


        static readonly Texture2D kIcon_InventoryItem = EditorGUIUtility.FindTexture("Prefab Icon");

        static readonly Texture2D kIcon_StatItem = EditorGUIUtility.FindTexture("GameManager Icon");


        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }


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


        public InventoryTree(TreeViewState state = null, MultiColumnHeader multiColumnHeader = null) : base(state ?? new TreeViewState(), multiColumnHeader)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }

        
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            //Reset AddItem PopUp Window Index when something else is selected
            DebugEditorWindow.ClearIndexes();
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
            else
            {
                if (m_ExpandedIdsBeforeSearch != null)
                {
                    SetExpanded(m_ExpandedIdsBeforeSearch);
                    m_ExpandedIdsBeforeSearch = null;
                }
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

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
            }
        }


        //This function processes a single cell based on the column its in and renders it in a different way.
        void CellGUI(Rect cellRect, TreeViewItem viewItem, Columns column, ref RowGUIArgs args)
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
            Rect centeredButtonPosition = cellRect;
            centeredButtonPosition.x += ((cellRect.xMax - cellRect.x - 30) / 2);

            if (viewItem is InventoryItemView itemView)
            {
                var clicked = GUI.Button(
                    centeredButtonPosition,
                    viewItem is InventoryItemView ? "Delete" : "Reset",
                    new GUIStyle(GUI.skin.button)
                    {
                        fixedWidth = 50,
                        alignment = TextAnchor.MiddleCenter
                    });

                if (clicked)
                {
                    HandleSelectionRemoved(viewItem);
                    //Get Inventory from tree item parent to get InventoryItem
                    var inventoryItem = itemView.inventoryItem;
                    InventoryManager.RemoveItem(inventoryItem);
                    CorrectFoldouts(viewItem);
                    Update();
                }
            }

            else if (viewItem is StatView statView)
            {
                var clicked = GUI.Button(
                    centeredButtonPosition,
                    viewItem is InventoryItemView ? "Delete" : "Reset",
                    new GUIStyle(GUI.skin.button)
                    {
                        fixedWidth = 50,
                        alignment = TextAnchor.MiddleCenter
                    });

                if (clicked)
                {
                    var inventoryItem = statView.inventoryItem;
                    var statDefinition = statView.statDefinition;
                    inventoryItem.ResetStat(statDefinition);
                    CorrectFoldouts(viewItem);
                    Update();
                }
            }

        }

        void ValueColumnGUI(Rect cellRect, TreeViewItem viewItem)
        {
            GUIStyle guiStyle = new GUIStyle(IsSelected(viewItem.id) ? GUI.skin.textField : GUI.skin.label)
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
                case StatView statView:
                    try
                    {
                        var inventoryItem = statView.inventoryItem;
                        var statDefinition = statView.statDefinition;

                        var statValue = inventoryItem.GetStat(statDefinition);

                        if (statDefinition.statValueType == StatValueType.Int)
                        {
                            var newValue = EditorGUI.IntField(cellRect, statValue, guiStyle);
                            if (newValue != statValue)
                            {
                                inventoryItem.SetStat(statDefinition, newValue);
                            }
                        }

                        else if (statDefinition.statValueType == StatValueType.Float)
                        {
                            var newValue = EditorGUI.FloatField(cellRect, statValue, guiStyle);
                            if (newValue != statValue)
                            {
                                inventoryItem.SetStat(statDefinition, newValue);
                            }
                        }
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

            string labelText = viewItem.displayName;

            GUI.Label(tempRect, labelText, DefaultStyles.label);
        }

        void DrawTreeViewIcons(Rect cellRect, TreeViewItem item, Rect tempRect)
        {
            //Draw Texture if in column if it can fit
            if (cellRect.width - GetContentIndent(item) > 16f)
                GUI.DrawTexture(tempRect, item.icon);
            //Find remaining room for Text (width - indent - icon size)
            tempRect.x += 16;
            tempRect.width = cellRect.width - GetContentIndent(item) - 16f;
        }

        void FilterRootOnSearch(TreeViewItem root, string search, List<TreeViewItem> allItems)
        {
            List<TreeViewItem> foundItems = allItems.Where(x => GetRealDisplayName(x).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            var newExpandedState = new HashSet<int>();
            foreach (var foundItem in foundItems)
            {
                newExpandedState.UnionWith(AddItemAndItsAncestorsIDs(foundItem));
            }
            state.expandedIDs = newExpandedState.ToList();
            state.expandedIDs.Sort();

            RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(root,foundItems);
        }
        
        void RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(TreeViewItem item, List<TreeViewItem> foundItems)
        {
            if (!item.hasChildren)
                return;

            for (int i=item.children.Count-1; i>=0; i--)
            {
                var child = item.children[i];
                if (child.hasChildren)
                {
                    if (!IsExpanded(child.id))
                        item.children.RemoveAt(i);
                    else
                        RemoveCollapsedChildrenAndLeafsNotMatchingSearchResultRecursive(child, foundItems);  // remove collapsed items
                }
                else
                {
                    if (!foundItems.Contains(child))
                        item.children.RemoveAt(i); // remove leaf items not matching search result
                }
            }
        }

        IEnumerable<int> AddItemAndItsAncestorsIDs(TreeViewItem item)
        {
            var results = new List<int>();
            results.Add(item.id);
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
            List<int> selectedIds = new List<int>(GetSelection());
            selectedIds.Remove(item.id);

            foreach (int id in selectedIds)
            {
                if (FindItem(id) == null)
                {
                    selectedIds.Remove(id);
                }
            }
            SetSelection(selectedIds);
        }

        //Correct expanded foldouts state when removing a row.
        void CorrectFoldouts(TreeViewItem deletedItem)
        {
            int GetLastId(TreeViewItem item)
            {
                return (item.hasChildren ? GetLastId(item.children[item.children.Count - 1]) : item.id);
            }
            
            int lastIdInChildren = GetLastId(deletedItem);
            int lastRemovedIndexInExpandedIds = deletedItem.id;
            for (int i = deletedItem.id; i <= lastIdInChildren; i++)
            {
                if (state.expandedIDs.Remove(i))
                {
                    lastRemovedIndexInExpandedIds = i;
                }
            }

            int removeOffset = lastIdInChildren - deletedItem.id + 1;
            for (int i = 0; i < state.expandedIDs.Count; i++)
            {
                if(state.expandedIDs[i] > lastRemovedIndexInExpandedIds)
                    state.expandedIDs[i] -= removeOffset;
            }
        }

        TreeViewItem GenerateInventoryTreeRoot()
        {
            int id = 0;
            m_AllTreeViewItems.Clear();
            var rootView = new TreeViewItem(id++, -1, "Root");

            var statCatalog = UnityEngine.GameFoundation.GameFoundation.catalogs.statCatalog;
            var currencyCatalog = UnityEngine.GameFoundation.GameFoundation.catalogs.currencyCatalog;

            var currenciesView = new TreeViewItem(id++, 0, "Wallet");
            rootView.AddChild(currenciesView);

            foreach (var currency in currencyCatalog.GetItems())
            {
                var currencyView = new CurrencyView
                    (id++, 1, $"{currency.displayName}", currency);

                currenciesView.AddChild(currencyView);
            }

            var itemsView = new TreeViewItem(id++, 0, "Items");
            rootView.AddChild(itemsView);

            InventoryManager.GetItems(m_InventoryItems);

            foreach (var item in m_InventoryItems)
            {
                var itemView = new InventoryItemView
                    (id++, 1, $"{item.definition.displayName} ({item.definition.id}) #{item.id}", item);

                rootView.AddChild(itemView);

                itemView.icon = kIcon_InventoryItem;
                m_AllTreeViewItems.Add(itemView);

                var statDetail = item.definition.GetDetail<StatDetail>();
                if (statDetail is null) continue;

                foreach (var statId in statDetail.m_DefaultValues.Keys)
                {
                    var statDefinition = statCatalog.FindStatDefinition(statId);

                    var statView = new StatView
                        (id++, 2, statDefinition.displayName, item, statDefinition);

                    itemView.AddChild(statView);
                    statView.icon = kIcon_StatItem;
                    m_AllTreeViewItems.Add(statView);
                }
            }

            return rootView;
        }

        bool m_Bound;
        public void Update()
        {
            if (!m_Bound)
            {
                if (UnityEngine.GameFoundation.GameFoundation.IsInitialized)
                {
                    InventoryManager.itemAdded += o => Reload();
                    InventoryManager.itemRemoved += o => Reload();
                    WalletManager.balanceChanged += (b, o, v) => Reload();

                    m_Bound = true;

                    //Calls BuildRoot and RowGUI in order.
                    Reload();
                }
            }
        }

        public TreeViewItem FindItem(int id)
        {
            return base.FindItem(id, rootItem);
        }

        public string GetRealDisplayName(TreeViewItem itemView)
        {
            switch (itemView)
            {
                case InventoryItemView inventoryItemView:
                    var inventoryItem = inventoryItemView.inventoryItem;
                    return inventoryItem.definition.displayName;

                case StatView statView:
                    var statDefinition = statView.statDefinition;
                    return statDefinition.displayName;

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
