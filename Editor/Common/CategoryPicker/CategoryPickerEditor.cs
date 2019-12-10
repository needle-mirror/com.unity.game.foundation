using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// UI Module for category selection UI.
    /// </summary>
    internal class CategoryPickerEditor
    {
        private class CategoryRow
        {
            public List<CategoryDefinition> categories;

            public CategoryRow()
            {
                categories = new List<CategoryDefinition>();
            }
        }

        private CategoryDefinition[] m_CategoryDefinitions;
        public CategoryDefinition[] categoryDefinitions
        {
            get { return m_CategoryDefinitions; }
        }

        private List<CategoryRow> m_WrappableCategoryRows = new List<CategoryRow>();
        private List<CategoryDefinition> m_CategorySearchResults = new List<CategoryDefinition>();
        private List<CategoryDefinition> m_AssignedCategories;

        private Rect m_CategoryItemsRect = new Rect();
        private string m_CategorySearchString = string.Empty;
        private string m_CategorySearchStringPrevious = string.Empty;
        private SearchField m_CategorySearchField = new SearchField();

        private Rect m_SuggestRect;
        private Vector2 m_CategorySearchSuggestScrollPosition = Vector2.zero;
        private int m_CategorySuggestSelectedIndex = -1;
        private bool m_UsedScrollWheelInSuggestBox = false;

        private CategoryFilterEditor m_CategoryFilterEditor = new CategoryFilterEditor();

        private static GUIContent s_CategoryLabel = new GUIContent("Categories", "Assign existing categories or create new ones. Categories can be used to filter items in the editor, or to get groups of items in code.");

        /// <summary>
        /// Re-cache the collection of categories from the inventory catalog.
        /// </summary>
        public void RefreshCategories()
        {
            if (GameFoundationSettings.database != null
                && GameFoundationSettings.database.inventoryCatalog != null)
            {
                m_CategoryDefinitions = GameFoundationSettings.database.inventoryCatalog.GetCategories();
            }
            else
            {
                m_CategoryDefinitions = new CategoryDefinition[0];
            }
        }

        private void RefreshAssignedCategories(GameItemDefinition gameItemDefinition)
        {
            if (m_AssignedCategories == null)
            {
                m_AssignedCategories = new List<CategoryDefinition>();
            }

            m_AssignedCategories.Clear();

            if (gameItemDefinition == null)
            {
                return;
            }

            gameItemDefinition.GetCategories(m_AssignedCategories);
        }

        /// <summary>
        /// Draws category selection search bar and selected categories.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition of the item that is
        /// currently selected for category selection.</param>
        public void DrawCategoryPicker(GameItemDefinition gameItemDefinition)
        {
            RefreshAssignedCategories(gameItemDefinition);

            DrawCategoriesDetail(gameItemDefinition);
        }

        /// <summary>
        /// Draws category search suggestion view. NOTE: This needs to be the last GUI call
        /// in the given window otherwise other elements will be drawn over it.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition of the item that is
        /// currently selected for category selection.</param>
        public void DrawCategoryPickerPopup(GameItemDefinition gameItemDefinition)
        {
            DrawCategorySearchSuggest(gameItemDefinition);
            HandleCategorySearchInput(gameItemDefinition);
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

        private void DrawCategoriesDetail(GameItemDefinition gameItemDefinition)
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
                                bool addButtonDisabled = !CollectionEditorTools.IsValidId(CollectionEditorTools.CraftUniqueId(m_CategorySearchString,new HashSet<string>(m_CategoryDefinitions.Select(category => category.id))));
                                
                                EditorGUI.BeginDisabledGroup(addButtonDisabled);
                                if (GUILayout.Button(new GUIContent("Add", addButtonDisabled ? "Input cannot contain only numerical values" : string.Empty), GUILayout.Width(CategoryPickerStyles.categoryAddButtonWidth)))
                                {
                                    // same as if user presses Enter or Return
                                    CreateAndAssignCategoryFromSearchField(gameItemDefinition);
                                }
                                EditorGUI.EndDisabledGroup();
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

                            RecalculateCategoryBoxHeight(gameItemDefinition);
                        }

                        // don't modify a collection while iterating through it
                        CategoryDefinition categoryToRemove = null;

                        using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                        {
                            // make enough room
                            GUILayout.Space(m_CategoryItemsRect.height);

                            // inside this vertical area, we cannot use GUILayout anymore because
                            // for/while inside GUILayout horizontal and vertical scopes will
                            // generate errors between Layout and Repaint events (chicken/egg problem)

                            for (int categoryRowIndex = 0; categoryRowIndex < m_WrappableCategoryRows.Count; categoryRowIndex++)
                            {
                                CategoryRow row = m_WrappableCategoryRows[categoryRowIndex];

                                float rowHeight = CategoryPickerStyles.categoryListItemStyle.CalcHeight(new GUIContent("lorem ipsum"), 1000f);

                                Rect rowRect = new Rect(m_CategoryItemsRect);
                                rowRect.height = rowHeight;
                                rowRect.y += categoryRowIndex * rowHeight;
                                rowRect.y += categoryRowIndex * CategoryPickerStyles.categoryItemMargin;

                                float curX = 0f;

                                foreach (CategoryDefinition category in row.categories)
                                {
                                    Vector2 categoryNameContentSize = CategoryPickerStyles.categoryListItemStyle.CalcSize(new GUIContent(category.displayName));

                                    Rect itemRect = new Rect(rowRect);
                                    itemRect.x += curX;
                                    itemRect.width = categoryNameContentSize.x + CategoryPickerStyles.categoryListItemStyle.padding.horizontal;

                                    Rect categoryDeleteButtonRect = new Rect(itemRect);
                                    // adjust the X rect over to the right side
                                    categoryDeleteButtonRect.x = itemRect.x + itemRect.width - CategoryPickerStyles.categoryRemoveButtonSpaceWidth;
                                    categoryDeleteButtonRect.width = CategoryPickerStyles.categoryRemoveButtonSpaceWidth;
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

                        if (categoryToRemove != null)
                        {
                            gameItemDefinition.RemoveCategory(categoryToRemove);
                            RefreshAssignedCategories(gameItemDefinition);
                        }
                    }
                }
            }
        }

        private void DrawCategorySearchSuggest(GameItemDefinition gameItemDefinition)
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
                float rowHeight = CategoryPickerStyles.categorySuggestItemStyle.CalcSize(new GUIContent("lorem ipsum")).y;
                float minVisibleY = m_CategorySearchSuggestScrollPosition.y;
                float maxVisibleY = m_SuggestRect.height + m_CategorySearchSuggestScrollPosition.y;
                float selectedItemTopY = rowHeight * m_CategorySuggestSelectedIndex;
                float selectedItemBottomY = selectedItemTopY + rowHeight;

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
                using (var scrollViewScope = new GUILayout.ScrollViewScope(m_CategorySearchSuggestScrollPosition, false, true))
                {
                    m_CategorySearchSuggestScrollPosition = scrollViewScope.scrollPosition;

                    for (int resultIndex = 0; resultIndex < m_CategorySearchResults.Count; resultIndex++)
                    {
                        CategoryDefinition suggestedCategory = m_CategorySearchResults[resultIndex];

                        // use the normal style, unless this is the highlighted item, in which case use the highlighted style
                        GUIStyle style = resultIndex == m_CategorySuggestSelectedIndex ? CategoryPickerStyles.categorySuggestItemStyleSelected : CategoryPickerStyles.categorySuggestItemStyle;

                        if (GUILayout.Button(suggestedCategory.displayName, style, GUILayout.ExpandWidth(true)))
                        {
                            AssignCategory(gameItemDefinition, suggestedCategory);
                            ResetCategorySearch(takeFocus: true);
                            UpdateCategorySuggestions();
                            RecalculateCategoryBoxHeight(gameItemDefinition);
                        }
                    }
                }
            }
        }

        private void HandleCategorySearchInput(GameItemDefinition gameItemDefinition)
        {
            if (string.IsNullOrEmpty(m_CategorySearchString)) return;

            if (Event.current.type == EventType.KeyUp)
            {
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
                                AssignCategory(gameItemDefinition, m_CategorySearchResults[m_CategorySuggestSelectedIndex]);
                                RecalculateCategoryBoxHeight(gameItemDefinition);
                            }
                            else
                            {
                                // if there are results but none are selected, then do nothing
                            }
                        }
                        else if (Event.current.keyCode != KeyCode.Tab)
                        {
                            // same as if "Add" is clicked
                            // if there are no suggestions but there is search string, then create a new category
                            // but it's probably not expected when tab key is used, so we'll exclude that one

                            CreateAndAssignCategoryFromSearchField(gameItemDefinition);
                        }

                        ResetCategorySearch(takeFocus: true);
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
        }

        private void CreateAndAssignCategoryFromSearchField(GameItemDefinition gameItemDefinition)
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Could not create category because Game Foundation database is null");
                return;
            }

            if (GameFoundationSettings.database.inventoryCatalog == null)
            {
                Debug.LogError("Could not create category because inventory catalog is null");
                return;
            }

            // don't allow creation of duplicate displayNames here
            // you can still do it in the main category editor
            if (m_CategoryDefinitions == null || m_CategoryDefinitions.Any(category => category.displayName == m_CategorySearchString)) return;

            string categoryId = CollectionEditorTools.CraftUniqueId(m_CategorySearchString, new HashSet<string>(m_CategoryDefinitions.Select(category => category.id)));

            if (CollectionEditorTools.IsValidId(m_CategorySearchString))
            {
                categoryId = CollectionEditorTools.DeDuplicateNewId(m_CategorySearchString, new HashSet<string>(m_CategoryDefinitions.Select(category => category.id)));
            }
            
            CategoryDefinition newCategory = new CategoryDefinition(categoryId, m_CategorySearchString);

            GameFoundationSettings.database.inventoryCatalog.AddCategory(newCategory);
            RefreshCategories();
            AssignCategory(gameItemDefinition, newCategory);

            // Refresh settings with new category
            RecalculateCategoryBoxHeight(gameItemDefinition);
            ResetCategorySearch(takeFocus: true);
            UpdateCategorySuggestions();
            m_CategoryFilterEditor.RefreshSidebarCategoryFilterList(m_CategoryDefinitions);
        }

        private void RecalculateCategoryBoxHeight(GameItemDefinition gameItemDefinition)
        {
            float currentRowContentWidth = 0f;

            m_WrappableCategoryRows = new List<CategoryRow>() { new CategoryRow() };

            if (m_AssignedCategories != null)
            {
                foreach (CategoryDefinition category in m_AssignedCategories)
                {
                    Vector2 contentSize = CategoryPickerStyles.categoryListItemStyle.CalcSize(new GUIContent(category.displayName));
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
            if (string.IsNullOrEmpty(m_CategorySearchString) || m_CategoryDefinitions == null)
            {
                m_CategorySearchResults = new List<CategoryDefinition>();
                m_CategorySuggestSelectedIndex = -1;
                return;
            }

            CategoryDefinition[] potentialMatches =
                System.Array.FindAll(
                    m_CategoryDefinitions,
                    cat => cat.displayName.ToLowerInvariant().Contains(m_CategorySearchString.ToLowerInvariant()));

            m_CategorySearchResults = potentialMatches
                .Where(potentialCategory => {
                    if (m_AssignedCategories == null)
                    {
                        return false;
                    }
                    return m_AssignedCategories.All(existingCategory => existingCategory != potentialCategory);
                }).ToList();

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

        private void AssignCategory(GameItemDefinition gameItemDefinition, CategoryDefinition addCategory)
        {
            if (gameItemDefinition != null && addCategory != null)
            {
                gameItemDefinition.AddCategory(addCategory);
                RefreshAssignedCategories(gameItemDefinition);
            }
        }
    }
}
