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
            Remove,
        }

        private enum Depth
        {
            Inventory,
            InventoryItem,
            Stat
        }

        private List<TreeViewItem> m_AllTreeViewItems = new List<TreeViewItem>();

        private IList<int> m_ExpandedIdsBeforeSearch;

        private string m_SearchString = String.Empty;
        public string SearchString
        {
          get => m_SearchString;
          set => m_SearchString = value;
        }

        private List<Inventory> m_Inventories = new List<Inventory>();
        public List<Inventory> Inventories
        {
            get => m_Inventories;
        }
        
        private List<InventoryItem> m_InventoryItems = new List<InventoryItem>();

        private readonly List<Texture2D> m_ItemIcons = new List<Texture2D>
        {
            EditorGUIUtility.FindTexture ("Folder Icon"),
            EditorGUIUtility.FindTexture ("Prefab Icon"),
            EditorGUIUtility.FindTexture ("GameManager Icon")
            
        };

        public InventoryTree(TreeViewState state = null, MultiColumnHeader multiColumnHeader = null) : base(state ?? new TreeViewState(), multiColumnHeader)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }

        public void Update()
        {
            //Calls BuildRoot and RowGUI in order.
            Reload();
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
            TreeViewItem inventoryRoot = GenerateInventoryTreeRoot();
            
            //Filter generated root according to search and save previous expanded state.
            if (!string.IsNullOrEmpty(m_SearchString))
            {
                if (m_ExpandedIdsBeforeSearch == null)
                {
                    m_ExpandedIdsBeforeSearch = GetExpanded();
                }
                FilterRootOnSearch(inventoryRoot, m_SearchString, m_AllTreeViewItems);
            }
            else
            {
                if (m_ExpandedIdsBeforeSearch != null)
                {
                    SetExpanded(m_ExpandedIdsBeforeSearch);
                    m_ExpandedIdsBeforeSearch = null;
                }
            }
            return inventoryRoot;
        }

        //Called when drawing the rows of the Tree View
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), (InventoryTreeItem)item, (Columns)args.GetColumn(i), ref args);
            }
        }
        
        //This function processes a single cell based on the column its in and renders it in a different way.
        private void CellGUI(Rect cellRect, InventoryTreeItem item, Columns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case Columns.Items:
                {
                    ItemColumnGUI(cellRect, item);
                    break;
                }
                case Columns.Value:
                {
                    ValueColumnGUI(cellRect, item);
                    break;
                }
                case Columns.Remove:
                {
                    RemoveColumnGUI(cellRect, item);
                    break;
                }
            }
        }

        private void RemoveColumnGUI(Rect cellRect, InventoryTreeItem item)
        {
            Rect centeredButtonPosition = cellRect;
            centeredButtonPosition.x += ((cellRect.xMax - cellRect.x - 30) / 2);

            //Can't remove the main and wallet inventories
            if (InventoryManager.GetInventory(item.itemHash)?.hash == InventoryManager.mainInventoryHash ||
                InventoryManager.GetInventory(item.itemHash)?.hash == InventoryManager.walletInventoryHash)
                return;

            if (GUI.Button(centeredButtonPosition, "X",
                new GUIStyle(GUI.skin.button) {fixedWidth = 30, alignment = TextAnchor.MiddleCenter}))
            {
                switch ((Depth) item.depth)
                {
                    case  Depth.Inventory:
                    {
                        Inventory inventory = GetGameItem(item) as Inventory;
                        InventoryManager.RemoveInventory(inventory);
                        break;
                    }
                    case Depth.InventoryItem:
                    {
                        //Get Inventory from tree item parent to get InventoryItem
                        InventoryManager.GetInventory(((InventoryTreeItem) item.parent).itemHash).RemoveItem(item.itemHash);
                        InventoryItem inventoryItem = GetGameItem(item) as InventoryItem;
                        inventoryItem?.inventory.RemoveItem(inventoryItem.id);
                        break;
                    }
                    case Depth.Stat:
                    {
                        InventoryItem inventoryItem = GetGameItem(item) as InventoryItem;
                        if (item.statType == StatDefinition.StatValueType.Int)
                        {
                            StatManager.RemoveIntValue(inventoryItem, item.itemHash);
                        }

                        if (item.statType == StatDefinition.StatValueType.Float)
                        {
                            StatManager.RemoveFloatValue(inventoryItem, item.itemHash);
                        }

                        break;
                    }
                }

                CorrectFoldouts(item);
                HandleSelectionRemoved(item);
                Update();
            }
        }

        private void ValueColumnGUI(Rect cellRect, InventoryTreeItem item)
        {
            if (item.depth > 0)
            {
                GUIStyle guiStyle = new GUIStyle(IsSelected(item.id) ? GUI.skin.textField : GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                //Bold Item Quantities
                if ((Depth) item.depth == Depth.InventoryItem)
                {
                    guiStyle.fontStyle = FontStyle.Bold;
                }

                //Enable write mode when item is selected.
                if (IsSelected(item.id))
                {
                    string newValue = EditorGUI.TextField(cellRect, item.statValue.ToString(), guiStyle);
                    if (!string.IsNullOrWhiteSpace(newValue))
                    {
                        switch ((Depth) item.depth)
                        {
                            case Depth.InventoryItem:
                            {
                                int result;
                                if (int.TryParse(newValue, out result))
                                {
                                    (GetGameItem(item) as InventoryItem)?.SetQuantity(result);
                                }

                                break;
                            }
                            case Depth.Stat:
                            {
                                InventoryItem inventoryItem = GetGameItem(item) as InventoryItem;
                                if (item.statType == StatDefinition.StatValueType.Int &&
                                    int.TryParse(newValue, out var resultInt))
                                {
                                    StatManager.SetIntValue(inventoryItem, item.itemHash, resultInt);
                                }

                                if (item.statType == StatDefinition.StatValueType.Float &&
                                    float.TryParse(newValue, out var resultFloat))
                                {
                                    StatManager.SetFloatValue(inventoryItem, item.itemHash, resultFloat);
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    EditorGUI.LabelField(cellRect, item.statValue.ToString(), guiStyle);
                }
            }
        }

        private void ItemColumnGUI(Rect cellRect, InventoryTreeItem item)
        {
            //Make Room for Icon between Arrow and Label
            Rect tempRect = cellRect;
            tempRect.x += GetContentIndent(item);
            tempRect.width = 16f;
            //Get clipping width for cell and remaining column width. 
            tempRect.width = cellRect.width - GetContentIndent(item);

            string labelText = item.displayName;

            GUI.Label(tempRect, labelText, DefaultStyles.label);
        }

        private void DrawTreeViewIcons(Rect cellRect, InventoryTreeItem item, Rect tempRect)
        {
            //Draw Texture if in column if it can fit
            if (cellRect.width - GetContentIndent(item) > 16f)
                GUI.DrawTexture(tempRect, item.icon);
            //Find remaining room for Text (width - indent - icon size)
            tempRect.x += 16;
            tempRect.width = cellRect.width - GetContentIndent(item) - 16f;
        }

        private void FilterRootOnSearch(TreeViewItem root, string search, List<TreeViewItem> allItems)
        {
            List<TreeViewItem> foundItems = allItems.Where(x => GetRealDisplayName((InventoryTreeItem)x).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
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

        private IEnumerable<int> AddItemAndItsAncestorsIDs(TreeViewItem item)
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

        private void HandleSelectionRemoved(InventoryTreeItem item)
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
        private void CorrectFoldouts(InventoryTreeItem deletedItem)
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

        private TreeViewItem GenerateInventoryTreeRoot()
        {
            int id = -1;
            m_AllTreeViewItems.Clear();
            TreeViewItem root = new InventoryTreeItem(id++,-1,"Root");
            m_Inventories.Clear();
            InventoryManager.GetInventories(m_Inventories);
            foreach (var inventory in m_Inventories)
            {
                m_InventoryItems.Clear();
                inventory.GetItems(m_InventoryItems);
                
                TreeViewItem inventoryNode;
                root.AddChild(inventoryNode = new InventoryTreeItem(id++,0,$"{inventory.displayName} - {m_InventoryItems.Count} item{(m_InventoryItems.Count > 1 ? "s" : "")} ({inventory.id})") {itemHash = inventory.hash, statValue = -1});
                m_AllTreeViewItems.Add(inventoryNode);
                inventoryNode.icon = m_ItemIcons[(int)Depth.Inventory];
                
                
                foreach (var item in m_InventoryItems)
                {
                    TreeViewItem inventoryItemNode;
                    inventoryNode.AddChild(inventoryItemNode = new InventoryTreeItem(id++, 1, $"{item.displayName} ({item.id})") {itemHash = item.hash, statValue = item.quantity});
                    inventoryItemNode.icon = m_ItemIcons[(int) Depth.InventoryItem];
                    m_AllTreeViewItems.Add(inventoryItemNode);

                    //Contains information of which statDefinitions are on GameItems
                    Dictionary<int, List<int>> intStatHashLookup = (Dictionary<int, List<int>>)GetInstanceField(typeof(StatManager.StatDictionary<int>), null, "m_GameItemStatList");
                    Dictionary<int, List<int>> floatStatHashLookup = (Dictionary<int, List<int>>)GetInstanceField(typeof(StatManager.StatDictionary<float>), null, "m_GameItemStatList");

                    TreeViewItem statsNode;
                    if (intStatHashLookup.ContainsKey(item.gameItemId))
                    {
                        foreach (int statHash in intStatHashLookup[item.gameItemId])
                        {
                            if (StatManager.HasIntValue(item, statHash))
                            {
                                StatDefinition statDefinition = StatManager.catalog.GetStatDefinition(statHash);
                                inventoryItemNode.AddChild(statsNode = new InventoryTreeItem(id++,2,statDefinition.displayName) { itemHash = statDefinition.idHash, statValue = StatManager.GetIntValue(item,statDefinition.id), statType = StatDefinition.StatValueType.Int});
                                statsNode.icon = m_ItemIcons[(int)Depth.Stat];
                                m_AllTreeViewItems.Add(statsNode);
                            }
                        }
                    }
                    if (floatStatHashLookup.ContainsKey(item.gameItemId))
                    {
                        foreach (int statHash in floatStatHashLookup[item.gameItemId])
                        {
                            if (StatManager.HasFloatValue(item, statHash))
                            {
                                StatDefinition statDefinition = StatManager.catalog.GetStatDefinition(statHash);
                                inventoryItemNode.AddChild(statsNode = new InventoryTreeItem(id++,2,statDefinition.displayName) { itemHash = statDefinition.idHash, statValue = StatManager.GetFloatValue(item,statDefinition.id), statType = StatDefinition.StatValueType.Float});
                                statsNode.icon = m_ItemIcons[(int)Depth.Stat];
                                m_AllTreeViewItems.Add(statsNode);
                            }
                        }
                    }
                }
            }

            return root;
        }
        
        public TreeViewItem FindItem(int id)
        {
            return base.FindItem(id,rootItem);
        }

        public string GetRealDisplayName(InventoryTreeItem inventoryTreeItem)
        {
            switch ((Depth)inventoryTreeItem.depth)
            {
                case Depth.Inventory:
                    return InventoryManager.GetInventory(inventoryTreeItem.itemHash).displayName;
                case Depth.InventoryItem:
                    return GameFoundationSettings.database.inventoryCatalog.GetItemDefinition(inventoryTreeItem.itemHash).displayName;
                case Depth.Stat:
                    return GameFoundationSettings.database.statCatalog.GetStatDefinition(inventoryTreeItem.itemHash).displayName;
                default:
                    throw new ArgumentException("Cannot get real display name of this InventoryTreeItem, bad depth parameter.");
            }
        }

        public GameItem GetGameItem(InventoryTreeItem inventoryTreeItem)
        {
            switch ((Depth)inventoryTreeItem.depth)
            {
                case Depth.Inventory:
                    return InventoryManager.GetInventory(inventoryTreeItem.itemHash);
                case Depth.InventoryItem:
                    return InventoryManager.GetInventory(((InventoryTreeItem)inventoryTreeItem.parent).itemHash).GetItem(inventoryTreeItem.itemHash);
                case Depth.Stat:
                    return InventoryManager.GetInventory(((InventoryTreeItem)inventoryTreeItem.parent.parent).itemHash).GetItem(((InventoryTreeItem)inventoryTreeItem.parent).itemHash);
                default:
                    throw new ArgumentException("Cannot get the corresponding GameItem for this InventoryTreeItem, bad depth parameter.");
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
                    width = treeViewWidth - 107,
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
                    contextMenuText = "Remove Item",
                    headerContent = new GUIContent("Remove"),
                    width = 55,
                    headerTextAlignment = TextAlignment.Center,
                    autoResize = false,
                    allowToggleVisibility = true,
                    canSort = false
                },
            };
            return new MultiColumnHeaderState(columns);
        }
        
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
    }
}
