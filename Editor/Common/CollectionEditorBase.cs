using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.GameFoundation
{
    internal abstract class CollectionEditorBase<T> : ICollectionEditor where T : class
    {
        private const float k_SideBarWidth = 270f;
        private const float k_ContentDetailMaxWidth = 630f;

        public string name { get; }

        protected T m_SelectedItem { get; private set; }
        private T m_PreviouslySelectedItem;
        private List<T> m_Items;

        private Vector2 m_ScrollPosition;
        private Vector2 m_ScrollPositionDetail;

        private Rect m_SidebarItemOffset;

        private T m_ItemToRemove;

        protected bool m_ClickedCreateButton;
        public bool isCreating { get; private set; }

        protected string m_NewItemDisplayName = string.Empty;
        protected string m_NewItemId = string.Empty;

        protected ReadableNameIdEditor m_ReadableNameIdEditor;

        protected readonly CategoryFilterEditor m_CategoryFilterEditor = new CategoryFilterEditor();

        protected CollectionEditorBase(string name)
        {
            this.name = name;
        }

        protected List<T> GetItems()
        {
            return m_Items;
        }

        protected abstract List<T> GetFilteredItems();

        public virtual void Draw()
        {
            ClearAndRemoveItems();

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    DrawSidebar();
                    DrawContent();
                }
            }
        }

        protected abstract void DrawDetail(T item, int index, int count);

        private void DrawCreateForm()
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                DrawCreateInputFields();

                DrawWarningMessage();

                GUILayout.Space(6f);

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Cancel", GUILayout.Width(120f)))
                    {
                        isCreating = false;
                        SelectItem(m_PreviouslySelectedItem);
                        m_PreviouslySelectedItem = null;
                    }

                    GUILayout.Space(6f);

                    if (string.IsNullOrEmpty(m_NewItemDisplayName)
                        || string.IsNullOrEmpty(m_NewItemId) 
                        || m_ReadableNameIdEditor.HasRegisteredId(m_NewItemId)
                        || !CollectionEditorTools.IsValidId(m_NewItemId))
                    {
                        CollectionEditorTools.SetGUIEnabledAtEditorTime(false);
                    }

                    if (GUILayout.Button("Create", GUILayout.Width(120f)))
                    {
                        CreateNewItemFinalize();

                        isCreating = false;
                    }

                    CollectionEditorTools.SetGUIEnabledAtEditorTime(true);
                }
            }
        }

        protected virtual void DrawCreateInputFields()
        {
            m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_NewItemId, ref m_NewItemDisplayName);

            if (m_ClickedCreateButton)
            {
                EditorGUI.FocusTextInControl("displayName");
            }
        }

        protected virtual void DrawWarningMessage()
        {
            EditorGUILayout.HelpBox("Once the Create button is clicked, the Id cannot be changed.", MessageType.Warning);
        }

        public virtual void OnWillEnter()
        {
            RefreshItems();

            isCreating = false;

            SelectItem(null);
        }

        public virtual void OnWillExit()
        {
            isCreating = false;

            SelectItem(null);
        }

        private void DrawSidebar()
        {
            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.sideBarStyle, GUILayout.Width(k_SideBarWidth)))
            {
                DrawSidebarContent();
            }
        }

        private void DrawSidebarContent()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawSidebarList();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+", GameFoundationEditorStyles.createButtonStyle))
            {
                m_ClickedCreateButton = true;

                if (!isCreating || CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    m_PreviouslySelectedItem = m_SelectedItem;
                    isCreating = true;
                    m_NewItemDisplayName = string.Empty;
                    m_NewItemId = string.Empty;
                    SelectItem(null);
                    CreateNewItem();
                }
            }
        }

        protected virtual void DrawSidebarList()
        {
            // TODO: reuse a list so we don't reallocate

            var filteredItems = GetFilteredItems();

            foreach (T item in filteredItems)
            {
                DrawSidebarListItem(item);
            }
        }

        protected abstract void DrawSidebarListItem(T item);

        protected void BeginSidebarItem(T item, Vector2 backgroundSize, Vector2 contentMargin)
        {
            var rect = EditorGUILayout.GetControlRect(true, backgroundSize.y);
            rect.width = backgroundSize.x;

            GUI.backgroundColor =
                item.Equals(m_SelectedItem) ? new Color(0.1f, 0.1f, 0.1f, .2f) : new Color(0, 0, 0, 0.0f);

            CollectionEditorTools.SetGUIEnabledAtRunTime(true);

            if (GUI.Button(rect, string.Empty))
            {
                if (!isCreating || CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    // if you click an item in the list, then cancel any creation in progress
                    isCreating = false;

                    SelectItem(item);
                }
            }

            CollectionEditorTools.SetGUIEnabledAtRunTime(false);

            GUI.backgroundColor = Color.white;

            m_SidebarItemOffset = rect;
            m_SidebarItemOffset.x += contentMargin.x;
            m_SidebarItemOffset.y += contentMargin.y;

            GUI.color = m_SelectedItem == item ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.6f);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(backgroundSize.y));
        }

        protected void EndSidebarItem()
        {
            GUI.color = Color.white;
        
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawSidebarItemLabel(string text, int width, GUIStyle style, int height = -1)
        {
            m_SidebarItemOffset.width = width;
            m_SidebarItemOffset.height = (height == -1) ? m_SidebarItemOffset.height : height;

            if (style == null)
            {
                EditorGUI.LabelField(m_SidebarItemOffset, text);
            }
            else
            {
                EditorGUI.LabelField(m_SidebarItemOffset, text, style);
            }

            m_SidebarItemOffset.x += width;
        }

        private bool DrawSidebarItemButton(string text, GUIStyle style, int width, int height = -1)
        {
            m_SidebarItemOffset.width = width;
            m_SidebarItemOffset.height = (height == -1) ? m_SidebarItemOffset.height : height;

            var clicked = GUI.Button(m_SidebarItemOffset, text, style);

            m_SidebarItemOffset.x += width;

            return clicked;
        }

        protected void DrawSidebarItemRemoveButton(T item)
        {
            if (DrawSidebarItemButton("X", GameFoundationEditorStyles.deleteButtonStyle, 18, 18))
            {
                if (EditorUtility.DisplayDialog("Are you sure?", "Do you want to delete " + name + "?", "Yes", "No"))
                {
                    m_ItemToRemove = item;
                }
            }
        }

        private void DrawContent()
        {
            using (var scrollViewScope =
                new GUILayout.ScrollViewScope(
                    m_ScrollPositionDetail, false, false, GUILayout.MaxWidth(k_ContentDetailMaxWidth)))
            {
                m_ScrollPositionDetail = scrollViewScope.scrollPosition;

                DrawContentDetail();
            }
        }

        public void ValidateSelection()
        {
            // it's possible that the selected item was deleted or a new database was loaded
            if (m_Items != null && !m_Items.Contains(m_SelectedItem))
            {
                SelectItem(null);
            }
        }

        private void DrawContentDetail()
        {
            if (m_SelectedItem != null)
            {
                isCreating = false;

                DrawDetail(m_SelectedItem, m_Items.FindIndex(x => x.Equals(m_SelectedItem)), m_Items.Count);
            }
            else if (isCreating)
            {
                DrawCreateForm();

                if (Event.current.type == EventType.Repaint)
                {
                    m_ClickedCreateButton = false;
                }
            }
            else
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        GUILayout.Label("No object selected.");

                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        protected abstract void CreateNewItem();
        protected abstract void CreateNewItemFinalize();
        protected abstract void AddItem(T item);
        protected abstract void OnRemoveItem(T item);

        /// <summary>
        /// This updates the cached list of items.
        /// The base implementation only constructs and clears the list.
        /// The inherited implementation should populate the list.
        /// </summary>
        public virtual void RefreshItems()
        {
            if (m_Items == null)
            {
                m_Items = new List<T>();
            }

            m_Items.Clear();
        }

        private void ClearAndRemoveItems()
        {
            if (m_Items == null)
            {
                return;
            }

            if (m_ItemToRemove == null)
            {
                return;
            }

            OnRemoveItem(m_ItemToRemove);
            m_ItemToRemove = null;
            SelectItem(null);
            RefreshItems();
        }

        protected virtual void SelectItem(T item)
        {
            m_SelectedItem = item;

            GUI.FocusControl(null);
        }

        protected void SelectFilteredItem(int listIndex)
        {
            var filteredItems = GetFilteredItems();

            if (filteredItems != null && filteredItems.Count > listIndex && listIndex >= 0)
            {
                SelectItem(filteredItems[listIndex]);
            }
            else
            {
                SelectItem(null);
            }
        }
    }
}
