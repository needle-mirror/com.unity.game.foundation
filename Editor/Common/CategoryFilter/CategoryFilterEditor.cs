using System.Collections.Generic;
using System.Linq;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Module for UI and logic of category filter.
    /// </summary>
    internal class CategoryFilterEditor
    {
        enum DefaultFilterOptions
        {
            All = 0,
            None = 1,
        }
        private const string k_None = "<None>";
        private const string k_All = "<All>";
        private const int k_ListOffset = 2;
        private int m_SelectedFilterCategoryIdx = (int)DefaultFilterOptions.All;
        private string[] m_CategoryNamesForFilter = null;

        /// <summary>
        /// Gets a filtered list of items based on the currently selected category.
        /// </summary>
        /// <param name="fullList">The list of GameItemDefinitions being filtered
        /// to the current category.</param>
        /// <param name="catalogCategories">The list of possible categories that can be filtered to.</param>
        /// <returns>Filtered list of GameItemDefinitions.</returns>
        public List<T> GetFilteredItems<T>(List<T> fullList, CategoryDefinition[] catalogCategories) where T : GameItemDefinition
        {
            if (fullList == null)
            {
                return null;
            }

            if (m_SelectedFilterCategoryIdx < 0 || m_SelectedFilterCategoryIdx >= (catalogCategories.Length + k_ListOffset))
            {
                m_SelectedFilterCategoryIdx = (int)DefaultFilterOptions.All;
            }

            if (m_SelectedFilterCategoryIdx == (int)DefaultFilterOptions.All)
            {
                return fullList;
            }
            else if (m_SelectedFilterCategoryIdx == (int)DefaultFilterOptions.None)
            {
                return fullList.FindAll(gameItemDefinition =>
                {
                    var categories = gameItemDefinition.GetCategories();
                    return categories == null || !categories.Any();
                });
            }

            return fullList.FindAll(gameItemDefinition =>
                {
                    var categories = gameItemDefinition.GetCategories();
                    if (categories == null || catalogCategories == null)
                    {
                        return false;
                    }

                    return categories.Any(categoryDefinition => categoryDefinition.hash == catalogCategories[m_SelectedFilterCategoryIdx - k_ListOffset].hash);
                });
        }

        /// <summary>
        /// Draws the UI for the filter selection popup.
        /// </summary>
        /// <param name="categoryChanged">out parameter modifier. Returns bool for whether or not the category filter has been changed.</returns>
        public void DrawCategoryFilter(out bool categoryChanged)
        {
            CollectionEditorTools.SetGUIEnabledAtRunTime(true);
            int newFilterIdx = EditorGUILayout.Popup(m_SelectedFilterCategoryIdx, m_CategoryNamesForFilter);
            if (newFilterIdx != m_SelectedFilterCategoryIdx)
            {
                m_SelectedFilterCategoryIdx = newFilterIdx;
                categoryChanged = true;
            }
            else
            {
                categoryChanged = false;
            }
            CollectionEditorTools.SetGUIEnabledAtRunTime(false);
        }

        /// <summary>
        /// Refreshes the list of possible categories that can be filtered to based on the given list.
        /// </summary>
        /// <param name="catalogCategories">The list of possible categories that can be filtered to.</param>
        public void RefreshSidebarCategoryFilterList(CategoryDefinition[] catalogCategories)
        {
            int categoryFilterCount = k_ListOffset;
            if (catalogCategories != null)
            {
                categoryFilterCount += catalogCategories.Count();
            }
            // Create Names for Pull-down menus
            m_CategoryNamesForFilter = new string[categoryFilterCount];
            m_CategoryNamesForFilter[(int)DefaultFilterOptions.All] = k_All;
            m_CategoryNamesForFilter[(int)DefaultFilterOptions.None] = k_None;

            if (catalogCategories != null)
            {
                for (int i = 0; i < catalogCategories.Length; i++)
                {
                    m_CategoryNamesForFilter[i + k_ListOffset] = catalogCategories[i].displayName;
                }
            }
        }

        /// <summary>
        /// Returns the current category selected in the filter dropdown.
        /// </summary>
        /// <param name="catalogCategories">The list of possible categories that can be filtered to.</param>
        /// <returns>The current CategoryDefinition selected by the filter.</returns>
        public CategoryDefinition GetCurrentFilteredCategory(CategoryDefinition[] catalogCategories)
        {
            if (catalogCategories == null
                || m_SelectedFilterCategoryIdx == (int)DefaultFilterOptions.All
                || m_SelectedFilterCategoryIdx == (int)DefaultFilterOptions.None)
            {
                return null;
            }

            return catalogCategories.ElementAt(m_SelectedFilterCategoryIdx - k_ListOffset);
        }

        /// <summary>
        /// Resets Category Filters list of potential category names and the selected filter index.
        /// </summary>
        public void ResetCategoryFilter()
        {
            m_SelectedFilterCategoryIdx = (int)DefaultFilterOptions.All;
            m_CategoryNamesForFilter = null;
        }
    }
}
