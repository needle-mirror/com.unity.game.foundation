using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// UI Module for category selection UI.
    /// </summary>
    internal class CategoryPickerEditor
    {
        private class CategoryRow
        {
            public readonly List<CategoryAsset> categories;

            public CategoryRow()
            {
                categories = new List<CategoryAsset>();
            }
        }

        BaseCatalogAsset m_Catalog;

        private CategoryAsset[] m_Categories;
        public CategoryAsset[] categories => m_Categories;

        private List<CategoryRow> m_WrappableCategoryRows = new List<CategoryRow>();
        private List<CategoryAsset> m_CategorySearchResults = new List<CategoryAsset>();
        private List<CategoryAsset> m_AssignedCategories;

        private Rect m_CategoryItemsRect;
        private string m_CategorySearchString = string.Empty;
        private string m_CategorySearchStringPrevious = string.Empty;
        private readonly SearchField m_CategorySearchField = new SearchField();

        private Rect m_SuggestRect;
        private Vector2 m_CategorySearchSuggestScrollPosition = Vector2.zero;
        private int m_CategorySuggestSelectedIndex = -1;
        private bool m_UsedScrollWheelInSuggestBox;

        private readonly CategoryFilterEditor m_CategoryFilterEditor = new CategoryFilterEditor();

        private static readonly GUIContent s_CategoryLabel = new GUIContent(
            "Categories",
            "Assign existing categories or create new ones. Use categories to filter/group items in the editor and code.");

        public CategoryPickerEditor(BaseCatalogAsset catalog)
        {
            m_Catalog = catalog;
        }

        /// <summary>
        /// Re-cache the collection of categories from the inventory catalog.
        /// </summary>
        public void RefreshCategories()
        {
            if(!CheckCatalog()) return;
            var categories = GetCatalogCategories();
            if (categories == null) categories = new CategoryAsset[0];

            m_Categories = categories;
        }

        protected CategoryAsset[] GetCatalogCategories()
            => m_Catalog.GetCategories();

        private void RefreshAssignedCategories(CatalogItemAsset catalogItem)
        {
            if (m_AssignedCategories == null)
            {
                m_AssignedCategories = new List<CategoryAsset>();
            }

            m_AssignedCategories.Clear();

            if (catalogItem == null)
            {
                return;
            }

            catalogItem.GetCategories(m_AssignedCategories);
        }

        /// <summary>
        /// Draws category selection search bar and selected categories.
        /// </summary>
        /// <param name="catalogItem">The GameItemDefinition of the item that is
        /// currently selected for category selection.</param>
        public void DrawCategoryPicker(CatalogItemAsset catalogItem)
        {
            RefreshAssignedCategories(catalogItem);

            DrawCategoriesDetail(catalogItem);
        }

        /// <summary>
        /// Draws category search suggestion view. NOTE: This needs to be the last GUI call
        /// in the given window otherwise other elements will be drawn over it.
        /// </summary>
        /// <param name="catalogItem">The GameItemDefinition of the item that is
        /// currently selected for category selection.</param>
        public void DrawCategoryPickerPopup(CatalogItemAsset catalogItem)
        {
            DrawCategorySearchSuggest(catalogItem);
            HandleCategorySearchInput(catalogItem);
        }

        /// <summary>
        /// Resets category search string.
        /// </summary>
        public void ResetCategorySearch(bool takeFocus = false)
        {
            m_CategorySuggestSelectedIndex = -1;
            m_CategorySearchString = string.Empty;

            if (takeFocus)
            {
                // do both of these - the first one just makes sure the next control doesn't get focused, the second one makes sure the text is being edited
                EditorGUI.FocusTextInControl("search field");
                m_CategorySearchField.SetFocus();
            }

            m_UsedScrollWheelInSuggestBox = false;
        }

        private void DrawCategoriesDetail(CatalogItemAsset catalogItem)
        {
            EditorGUILayout.LabelField(s_CategoryLabel, GameFoundationEditorStyles.titleStyle);

            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            m_CategorySearchString = m_CategorySearchField.OnGUI(m_CategorySearchString);

                            if (m_CategorySearchStringPrevious != m_CategorySearchString)
                            {
                                UpdateCategorySuggestions();
                            }
                            m_CategorySearchStringPrevious = m_CategorySearchString;

                            // only show the Add button if:
                            //  • we are searching
                            //  • there are no suggestions found

                            if (!string.IsNullOrEmpty(m_CategorySearchString) && m_CategorySearchResults.Count <= 0)
                            {
                                // Disable Category Add Button if the category they are trying to add is not a valid id
                                var addButtonDisabled = !CollectionEditorTools.IsValidId(
                                    CollectionEditorTools.CraftUniqueId(
                                        m_CategorySearchString,
                                        new HashSet<string>(m_Categories.Select(category => category.id))));

                                using (new EditorGUI.DisabledGroupScope(addButtonDisabled))
                                {
                                    var tooltip = addButtonDisabled
                                        ? "No existing categories matched. If creating a new category, the input must contain at least one letter."
                                        : string.Empty;

                                    if (GUILayout.Button(new GUIContent("Add", tooltip), GUILayout.Width(CategoryPickerStyles.categoryAddButtonWidth)))
                                    {
                                        // same as if user presses Enter or Return
                                        CreateAndAssignCategoryFromSearchField(catalogItem);
                                    }
                                }
                            }
                        }

                        EditorGUILayout.Space();

                        // dimensions should be calculated during Repaint because during Layout they aren't calculated yet
                        if (Event.current.type == EventType.Repaint)
                        {
                            m_SuggestRect = GUILayoutUtility.GetLastRect();
                            m_SuggestRect.x += 24;
                            m_SuggestRect.width -= 40;
                            m_SuggestRect.height = 220;

                            m_CategoryItemsRect = GUILayoutUtility.GetLastRect();
                            m_CategoryItemsRect.x += 12f;
                            m_CategoryItemsRect.y += 18f;

                            RecalculateCategoryBoxHeight();
                        }

                        // don't modify a collection while iterating through it
                        CategoryAsset categoryToRemove = null;

                        using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                        {
                            // make enough room
                            GUILayout.Space(m_CategoryItemsRect.height);

                            // inside this vertical area, we cannot use GUILayout anymore because
                            // for/while inside GUILayout horizontal and vertical scopes will
                            // generate errors between Layout and Repaint events (chicken/egg problem)

                            for (var categoryRowIndex = 0; categoryRowIndex < m_WrappableCategoryRows.Count; categoryRowIndex++)
                            {
                                var row = m_WrappableCategoryRows[categoryRowIndex];

                                var rowHeight = CategoryPickerStyles.categoryListItemStyle.CalcHeight(new GUIContent("lorem ipsum"), 1000f);

                                var rowRect = new Rect(m_CategoryItemsRect) {height = rowHeight};
                                rowRect.y += categoryRowIndex * rowHeight;
                                rowRect.y += categoryRowIndex * CategoryPickerStyles.categoryItemMargin;

                                var curX = 0f;

                                foreach (var category in row.categories)
                                {
                                    var categoryNameContentSize = CategoryPickerStyles.categoryListItemStyle.CalcSize(new GUIContent(category.displayName));

                                    var itemRect = new Rect(rowRect);
                                    itemRect.x += curX;
                                    itemRect.width = categoryNameContentSize.x + CategoryPickerStyles.categoryListItemStyle.padding.horizontal;

                                    var categoryDeleteButtonRect = new Rect(itemRect)
                                    {
                                        x = itemRect.x + itemRect.width - CategoryPickerStyles.categoryRemoveButtonSpaceWidth,
                                        width = CategoryPickerStyles.categoryRemoveButtonSpaceWidth
                                    };

                                    // adjust the X rect over to the right side

                                    // nudge it a bit to look better
                                    categoryDeleteButtonRect.x -= 2;
                                    categoryDeleteButtonRect.y += 4;

                                    GUI.Box(itemRect, category.displayName, CategoryPickerStyles.categoryListItemStyle);

                                    if (GUI.Button(categoryDeleteButtonRect, "<b>X</b>", GameFoundationEditorStyles.deleteButtonStyle))
                                    {
                                        categoryToRemove = category;
                                    }

                                    curX += itemRect.width + CategoryPickerStyles.categoryItemMargin;
                                }
                            }
                        }

                        if (categoryToRemove == null)
                        {
                            return;
                        }

                        catalogItem.Editor_RemoveCategory(categoryToRemove);
                        RefreshAssignedCategories(catalogItem);
                    }
                }
            }
        }

        private void DrawCategorySearchSuggest(CatalogItemAsset catalogItem)
        {
            // only show the search suggest window and handle input for it if...
            // - the search field is currently in focus
            // - there is text in the search field
            // - there are suggestions to show
            if (string.IsNullOrEmpty(m_CategorySearchString)) return;
            if (m_CategorySearchResults.Count <= 0) return;

            // adjust scroll position if the highlighted item is not visible
            // but if the scroll wheel is used, then obey the scroll wheel instead

            if (Event.current.type == EventType.ScrollWheel) m_UsedScrollWheelInSuggestBox = true;

            if (!m_UsedScrollWheelInSuggestBox)
            {
                var rowHeight = CategoryPickerStyles.categorySuggestItemStyle.CalcSize(new GUIContent("lorem ipsum")).y;
                var minVisibleY = m_CategorySearchSuggestScrollPosition.y;
                var maxVisibleY = m_SuggestRect.height + m_CategorySearchSuggestScrollPosition.y;
                var selectedItemTopY = rowHeight * m_CategorySuggestSelectedIndex;
                var selectedItemBottomY = selectedItemTopY + rowHeight;

                if (minVisibleY > selectedItemTopY)
                {
                    m_CategorySearchSuggestScrollPosition.Set(0, selectedItemTopY);
                }

                if (maxVisibleY < selectedItemBottomY)
                {
                    m_CategorySearchSuggestScrollPosition.Set(0, selectedItemBottomY - m_SuggestRect.height);
                }
            }

            // RENDER

            using (new GUILayout.AreaScope(m_SuggestRect, "", CategoryPickerStyles.searchSuggestAreaStyle))
            {
                using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(m_CategorySearchSuggestScrollPosition, false, true))
                {
                    m_CategorySearchSuggestScrollPosition = scrollViewScope.scrollPosition;

                    for (var resultIndex = 0; resultIndex < m_CategorySearchResults.Count; resultIndex++)
                    {
                        var suggestedCategory = m_CategorySearchResults[resultIndex];

                        // use the normal style, unless this is the highlighted item, in which case use the highlighted style
                        var style = resultIndex == m_CategorySuggestSelectedIndex
                            ? CategoryPickerStyles.categorySuggestItemStyleSelected
                            : CategoryPickerStyles.categorySuggestItemStyle;

                        if (GUILayout.Button(suggestedCategory.displayName, style, GUILayout.ExpandWidth(true)))
                        {
                            AssignCategory(catalogItem, suggestedCategory);
                            ResetCategorySearch(true);
                            UpdateCategorySuggestions();
                            RecalculateCategoryBoxHeight();
                        }
                    }
                }
            }
        }

        private void HandleCategorySearchInput(CatalogItemAsset catalogItem)
        {
            if (string.IsNullOrEmpty(m_CategorySearchString)) return;

            if (Event.current.type != EventType.KeyUp)
            {
                return;
            }

            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                    if (m_CategorySearchResults.Count > 0)
                    {
                        Event.current.Use();

                        m_CategorySuggestSelectedIndex -= 1;
                        m_UsedScrollWheelInSuggestBox = false;
                    }
                    break;

                case KeyCode.DownArrow:
                    if (m_CategorySearchResults.Count > 0)
                    {
                        Event.current.Use();
                        m_CategorySuggestSelectedIndex += 1;
                        m_UsedScrollWheelInSuggestBox = false;
                    }
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                case KeyCode.Tab:
                    Event.current.Use();

                    if (m_CategorySearchResults.Count > 0)
                    {
                        if (m_CategorySuggestSelectedIndex >= 0)
                        {
                            // if there are results and one is selected, then assign it
                            AssignCategory(catalogItem, m_CategorySearchResults[m_CategorySuggestSelectedIndex]);
                            RecalculateCategoryBoxHeight();
                        }
                        // if there are results but none are selected, then do nothing
                    }
                    else if (Event.current.keyCode != KeyCode.Tab)
                    {
                        // same as if "Add" is clicked
                        // if there are no suggestions but there is search string, then create a new category
                        // but it's probably not expected when tab key is used, so we'll exclude that one

                        CreateAndAssignCategoryFromSearchField(catalogItem);
                    }

                    ResetCategorySearch(true);
                    UpdateCategorySuggestions();
                    break;

                case KeyCode.Escape:
                    Event.current.Use();
                    ResetCategorySearch();
                    UpdateCategorySuggestions();
                    break;

                default:
                    break;
            }

            CorrectCategorySearchSuggestSelectedIndex();
        }

        protected bool CheckCatalog() => m_Catalog != null;

        protected void AddItem(CategoryAsset category)
        {
            m_Catalog.Editor_AddCategory(category);
            CollectionEditorTools.AssetDatabaseAddObject(category, m_Catalog);
        }

        private void CreateAndAssignCategoryFromSearchField(CatalogItemAsset catalogItem)
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("Could not create category because Game Foundation database is null");
                return;
            }

            if(!CheckCatalog()) return;

            // don't allow creation of duplicate displayNames here
            // you can still do it in the main category editor
            if (m_Categories == null || m_Categories.Any(category => category.displayName == m_CategorySearchString)) return;

            string categoryId = CollectionEditorTools.CraftUniqueId(m_CategorySearchString, new HashSet<string>(m_Categories.Select(category => category.id)));

            if (CollectionEditorTools.IsValidId(m_CategorySearchString))
            {
                categoryId = CollectionEditorTools.DeDuplicateNewId(m_CategorySearchString, new HashSet<string>(m_Categories.Select(category => category.id)));
            }
            
            var newCategory = CategoryAsset.Editor_Create(categoryId, m_CategorySearchString);
            if (newCategory != null)
            {
                AddItem(newCategory);

                RefreshCategories();
                AssignCategory(catalogItem, newCategory);
            }

            // Refresh settings with new category
            RecalculateCategoryBoxHeight();
            ResetCategorySearch(true);
            UpdateCategorySuggestions();
            m_CategoryFilterEditor.RefreshSidebarCategoryFilterList(m_Categories);
        }

        private void RecalculateCategoryBoxHeight()
        {
            var currentRowContentWidth = 0f;

            m_WrappableCategoryRows = new List<CategoryRow> { new CategoryRow() };

            if (m_AssignedCategories != null)
            {
                foreach (var category in m_AssignedCategories)
                {
                    var contentSize = CategoryPickerStyles.categoryListItemStyle.CalcSize(new GUIContent(category.displayName));
                    contentSize.x += CategoryPickerStyles.categoryListItemStyle.padding.horizontal + CategoryPickerStyles.categoryRemoveButtonSpaceWidth;

                    if (currentRowContentWidth + contentSize.x > m_CategoryItemsRect.width)
                    {
                        m_WrappableCategoryRows.Add(new CategoryRow());
                        currentRowContentWidth = 0f;
                    }

                    m_WrappableCategoryRows.Last().categories.Add(category);
                    currentRowContentWidth += contentSize.x;
                }
            }

            m_CategoryItemsRect.height = m_WrappableCategoryRows.Count * CategoryPickerStyles.categoryListItemStyle.CalcSize(new GUIContent("lorem ipsum")).y;
            m_CategoryItemsRect.height += (m_WrappableCategoryRows.Count - 1) * CategoryPickerStyles.categoryItemMargin;
        }

        private void UpdateCategorySuggestions()
        {
            if (string.IsNullOrEmpty(m_CategorySearchString) || m_Categories == null)
            {
                m_CategorySearchResults = new List<CategoryAsset>();
                m_CategorySuggestSelectedIndex = -1;
                return;
            }

            var potentialMatches =
                System.Array.FindAll(
                    m_Categories,
                    cat => cat.displayName.ToLowerInvariant().Contains(m_CategorySearchString.ToLowerInvariant()));

            m_CategorySearchResults = potentialMatches
                .Where(potentialCategory =>
                {
                    return m_AssignedCategories != null && m_AssignedCategories.All(existingCategory => existingCategory != potentialCategory);
                })
                .ToList();

            CorrectCategorySearchSuggestSelectedIndex();
        }

        private void CorrectCategorySearchSuggestSelectedIndex()
        {
            if (m_CategorySearchResults.Count <= 0)
            {
                m_CategorySuggestSelectedIndex = -1;
            }
            else if (m_CategorySuggestSelectedIndex < 0)
            {
                m_CategorySuggestSelectedIndex = m_CategorySearchResults.Count - 1;
            }
            else if (m_CategorySuggestSelectedIndex >= m_CategorySearchResults.Count)
            {
                m_CategorySuggestSelectedIndex = 0;
            }
        }

        private void AssignCategory(CatalogItemAsset catalogItem, CategoryAsset category)
        {
            if (catalogItem == null || category == null)
            {
                return;
            }

            catalogItem.Editor_AddCategory(category);

            RefreshAssignedCategories(catalogItem);
        }
    }
}
