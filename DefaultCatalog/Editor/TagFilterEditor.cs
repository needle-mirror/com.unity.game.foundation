using System.Collections.Generic;
using System.Linq;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Module for UI and logic of tag filter.
    /// </summary>
    class TagFilterEditor
    {
        enum DefaultFilterOptions
        {
            All = 0,
            None = 1,
        }

        const string k_None = "<None>";
        const string k_All = "<All>";
        const int k_ListOffset = 2;
        int m_SelectedFilterTagKey = (int)DefaultFilterOptions.All;
        string[] m_TagNamesForFilter;

        /// <summary>
        /// Gets a filtered list of items based on the currently selected Tag.
        /// </summary>
        /// <param name="fullList">The list of GameItemDefinitions being filtered
        /// to the current Tag.</param>
        /// <param name="tags">The list of possible Tags that can be filtered to.</param>
        /// <returns>Filtered list of GameItemDefinitions.</returns>
        public List<T> GetFilteredItems<T>(List<T> fullList, TagAsset[] tags) where T : CatalogItemAsset
        {
            if (fullList == null)
            {
                return null;
            }

            if (tags is null) return null;

            if (m_SelectedFilterTagKey < 0 || m_SelectedFilterTagKey >= (tags.Length + k_ListOffset))
            {
                m_SelectedFilterTagKey = (int)DefaultFilterOptions.All;
            }

            if (m_SelectedFilterTagKey == (int)DefaultFilterOptions.All)
            {
                return fullList;
            }

            if (m_SelectedFilterTagKey == (int)DefaultFilterOptions.None)
            {
                return fullList.FindAll(item =>
                {
                    var itemTags = item.GetTags();
                    return itemTags == null || !itemTags.Any();
                });
            }

            return fullList.FindAll(item =>
            {
                var itemTags = item.GetTags();
                if (itemTags == null || tags == null)
                {
                    return false;
                }

                return itemTags.Any(tag => tag.key == tags[m_SelectedFilterTagKey - k_ListOffset].key);
            });
        }

        /// <summary>
        /// Draws the UI for the filter selection popup.
        /// </summary>
        /// <param name="tagChanged">out parameter modifier. Returns bool for whether or not the Tag filter has been changed.</returns>
        public void DrawTagFilter(out bool tagChanged)
        {
            CollectionEditorTools.SetGUIEnabledAtRunTime(true);
            int newFilterKey = EditorGUILayout.Popup(m_SelectedFilterTagKey, m_TagNamesForFilter);
            if (newFilterKey != m_SelectedFilterTagKey)
            {
                m_SelectedFilterTagKey = newFilterKey;
                tagChanged = true;
            }
            else
            {
                tagChanged = false;
            }

            CollectionEditorTools.SetGUIEnabledAtRunTime(false);
        }

        /// <summary>
        /// Refreshes the list of possible Tags that can be filtered to based on the given list.
        /// </summary>
        /// <param name="tags">The list of possible Tags that can be filtered to.</param>
        public void RefreshSidebarTagFilterList(TagAsset[] tags)
        {
            int tagFilterCount = k_ListOffset;
            if (tags != null)
            {
                for (int i = 0; i < tags.Count(); ++ i)
                {
                    if (tags[i] != null)
                    {
                        ++ tagFilterCount;
                    }
                }
            }

            // Create Names for Pull-down menus
            m_TagNamesForFilter = new string[tagFilterCount];
            m_TagNamesForFilter[(int)DefaultFilterOptions.All] = k_All;
            m_TagNamesForFilter[(int)DefaultFilterOptions.None] = k_None;

            if (tags != null)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    m_TagNamesForFilter[i + k_ListOffset] = tags[i].key;
                }
            }
        }

        /// <summary>
        /// Returns the current Tag selected in the filter dropdown.
        /// </summary>
        /// <param name="tags">The list of possible Tags that can be filtered to.</param>
        /// <returns>The current TagDefinition selected by the filter.</returns>
        public TagAsset GetCurrentFilteredTag(TagAsset[] tags)
        {
            if (tags == null
                || m_SelectedFilterTagKey == (int)DefaultFilterOptions.All
                || m_SelectedFilterTagKey == (int)DefaultFilterOptions.None)
            {
                return null;
            }

            return tags.ElementAt(m_SelectedFilterTagKey - k_ListOffset);
        }

        /// <summary>
        /// Resets Tag Filters list of potential Tag names and the selected filter index.
        /// </summary>
        public void ResetTagFilter()
        {
            m_SelectedFilterTagKey = (int)DefaultFilterOptions.All;
            m_TagNamesForFilter = null;
        }
    }
}
