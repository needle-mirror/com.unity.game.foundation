using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    abstract class CollectionEditorBase<T> : ICollectionEditor
        where T : class
    {
        const float k_SideBarWidth = 270f;
        const float k_ContentDetailMaxWidth = 630f;

        public string name { get; }

        protected T selectedItem { get; private set; }

        T m_PreviouslySelectedItem;

        protected readonly List<T> m_Items = new List<T>();

        Vector2 m_ScrollPosition;
        Vector2 m_ScrollPositionDetail;

        Rect m_SidebarItemOffset;

        T m_ItemToRemove;

        protected bool m_ClickedCreateButton;
        public bool isCreating { get; private set; }

        protected string m_NewItemDisplayName = string.Empty;
        protected string m_NewItemKey = string.Empty;

        protected ReadableNameKeyEditor m_ReadableNameKeyEditor;

        protected readonly TagFilterEditor m_TagFilterEditor = new TagFilterEditor();

        protected abstract GameFoundationAnalytics.TabName tabName { get; }

        protected CollectionEditorBase(string name)
        {
            this.name = name;
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

        protected abstract void DrawDetail(T item);

        void DrawCreateForm()
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                DrawCreateInputFields();

                EditorGUILayout.HelpBox(
                    "Once the Create button is clicked, the Key cannot be changed.",
                    MessageType.Warning);

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
                        || string.IsNullOrEmpty(m_NewItemKey)
                        || m_ReadableNameKeyEditor.HasRegisteredKey(m_NewItemKey)
                        || !CollectionEditorTools.IsValidId(m_NewItemKey))
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
            m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_NewItemKey, ref m_NewItemDisplayName);

            if (m_ClickedCreateButton)
            {
                EditorGUI.FocusTextInControl("displayName");
            }
        }

        public void OnWillEnter()
        {
            RefreshItems();

            isCreating = false;

            SelectItem(null);

            // Select the first Item
            SelectFilteredItem(0);

            GameFoundationAnalytics.SendOpenTabEvent(tabName);
        }

        public virtual void OnWillExit()
        {
            isCreating = false;

            SelectItem(null);
        }

        void DrawSidebar()
        {
            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.sideBarStyle, GUILayout.Width(k_SideBarWidth)))
            {
                DrawSidebarContent();
            }
        }

        void DrawSidebarContent()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            DrawSidebarList();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+", GameFoundationEditorStyles.createButtonStyle))
            {
                m_ClickedCreateButton = true;

                if (!isCreating || CollectionEditorTools.ConfirmDiscardingNewItem())
                {
                    m_PreviouslySelectedItem = selectedItem;
                    isCreating = true;
                    m_NewItemDisplayName = string.Empty;
                    m_NewItemKey = string.Empty;
                    SelectItem(null);
                    CreateNewItem();
                }
            }
        }

        protected virtual void DrawSidebarList()
        {
            // TODO: reuse a list so we don't reallocate

            var filteredItems = GetFilteredItems();

            if (filteredItems == null) return;
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
                item.Equals(selectedItem) ? new Color(0.1f, 0.1f, 0.1f, .2f) : new Color(0, 0, 0, 0.0f);

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

            GUI.color = selectedItem == item ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.6f);

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
            m_SidebarItemOffset.height = height == -1 ? m_SidebarItemOffset.height : height;

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

        bool DrawSidebarItemButton(string text, GUIStyle style, int width, int height = -1)
        {
            m_SidebarItemOffset.width = width;
            m_SidebarItemOffset.height = height == -1 ? m_SidebarItemOffset.height : height;

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

        void DrawContent()
        {
            using (var scrollViewScope = new GUILayout.ScrollViewScope
                (m_ScrollPositionDetail, false, false, GUILayout.MaxWidth(k_ContentDetailMaxWidth)))
            {
                m_ScrollPositionDetail = scrollViewScope.scrollPosition;
                DrawContentDetail();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }

        public void ValidateSelection()
        {
            // it's possible that the selected item was deleted or a new database was loaded
            if (m_Items != null && !m_Items.Contains(selectedItem))
            {
                SelectItem(null);
            }
        }

        void DrawContentDetail()
        {
            if (selectedItem != null)
            {
                isCreating = false;

                DrawDetail(selectedItem);
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
        ///     This updates the cached list of items.
        ///     The base implementation only constructs and clears the list.
        ///     The inherited implementation should populate the list.
        /// </summary>
        public virtual void RefreshItems()
        {
            m_Items.Clear();
        }

        void ClearAndRemoveItems()
        {
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
            selectedItem = item;

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
