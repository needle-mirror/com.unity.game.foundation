using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Editor tab for the Stat window that allows creation of Stat definitions.
    /// </summary>
    internal class StatDefinitionEditor : CollectionEditorBase<StatDefinition>
    {
        private string m_CurrentItemId = null;
        private int m_SelectedTypePopupIdx = 0;
        private string[] m_TypeNamesForPopup = System.Enum.GetNames( typeof( StatDefinition.StatValueType ) );
        private StatDefinition.StatValueType m_NewItemValueType;
        private static GUIContent s_ValueTypeLabel = new GUIContent("Value Type", "The numerical data type that values for this stat will be stored as.");

        /// <summary>
        /// Constructor for the StatDefinitionEditor class.
        /// </summary>
        public StatDefinitionEditor(string name, StatEditorWindow window) : base(name, window)
        {
        }
        
        protected override List<StatDefinition> GetFilteredItems()
        {
            return GetItems();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            if (GameFoundationSettings.database != null
                && GameFoundationSettings.database.statCatalog != null)
            {
                GameFoundationSettings.database.statCatalog.GetStatDefinitions(GetItems());
            }
        }

        /// <summary>
        /// Override base class method for what happens when the tab is opened.
        /// </summary>
        public override void OnWillEnter()
        {
            base.OnWillEnter();

            if (GameFoundationSettings.database != null
                && GameFoundationSettings.database.statCatalog != null)
            {
                SelectFilteredItem(0); // Select the first Item
            }
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
            m_SelectedTypePopupIdx = 0;
            m_NewItemValueType = StatDefinition.StatValueType.Int;
        }

        protected override void DrawCreateInputFields()
        {
            base.DrawCreateInputFields();
            DrawValueTypePopup();
        }

        protected override void DrawWarningMessage()
        {
            EditorGUILayout.HelpBox("Once the Create button is clicked Id and Value Type cannot be changed.", MessageType.Warning);
        }

        protected void DrawValueTypePopup()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(s_ValueTypeLabel, GUILayout.Width(145));
                int newFilterIdx = EditorGUILayout.Popup(m_SelectedTypePopupIdx, m_TypeNamesForPopup);
                if (newFilterIdx != m_SelectedTypePopupIdx)
                {
                    m_SelectedTypePopupIdx = newFilterIdx;
                    System.Enum.TryParse(m_TypeNamesForPopup[m_SelectedTypePopupIdx], out m_NewItemValueType);
                }
            }
        }

        protected override void CreateNewItemFinalize()
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Could not create new stat definition because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationSettings.database.statCatalog == null)
            {
                Debug.LogError("Could not create new stat definition because the stat catalog is null.");
                return;
            }

            StatDefinition itemDefinition = new StatDefinition(m_NewItemId, m_NewItemDisplayName, m_NewItemValueType);

            AddItem(itemDefinition);
            SelectItem(itemDefinition);
            RefreshItems();
            List<StatDefinition> statDefinitions = GetItems();
            DrawDetail(itemDefinition, statDefinitions.FindIndex(x => x.Equals(m_SelectedItem)), statDefinitions.Count);
        }

        protected override void AddItem(StatDefinition statDefinition)
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Stat Definition " + statDefinition.displayName + " could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationSettings.database.statCatalog == null)
            {
                Debug.LogError("Stat Definition " + statDefinition.displayName + " could not be added because the stat catalog is null");
            }
            else
            {
                GameFoundationSettings.database.statCatalog.AddStatDefinition(statDefinition);
                EditorUtility.SetDirty(GameFoundationSettings.database.statCatalog);
            }
        }

        protected override void DrawDetail(StatDefinition item, int index, int count)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = item.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);
                if (item.displayName != displayName)
                {
                    item.displayName = displayName;
                    EditorUtility.SetDirty(GameFoundationSettings.database.statCatalog);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(s_ValueTypeLabel, GUILayout.Width(145));
                    EditorGUILayout.SelectableLabel(item.statValueType.ToString(), GUILayout.Height(15), GUILayout.ExpandWidth(true));
                }
            }
        }

        protected override void DrawSidebarListItem(StatDefinition item, int index)
        {
            BeginSidebarItem(item, index, new Vector2(220f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(item.displayName, 220, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(item);

            EndSidebarItem(item, index);
        }

        protected override void SelectItem(StatDefinition item)
        {
            if (item != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentItemId = item.id;
                m_SelectedTypePopupIdx = (int)item.statValueType;
            }

            base.SelectItem(item);
        }

        protected override void OnRemoveItem(StatDefinition statDefinition)
        {
            if (statDefinition != null)
            {
                if (GameFoundationSettings.database == null)
                {
                    Debug.LogError("Stat Definition " + statDefinition.displayName + " could not be removed because the Game Foundation database is null");
                }
                else if (GameFoundationSettings.database.statCatalog == null)
                {
                    Debug.LogError("Stat Definition " + statDefinition.displayName + " could not be removed because the stat catalog is null");
                }
                else
                {
                    if (GameFoundationSettings.database.statCatalog.RemoveStatDefinition(statDefinition))
                    {
                        EditorUtility.SetDirty(GameFoundationSettings.database.statCatalog);
                    }
                    else
                    {
                        Debug.LogError("Stat Definition " + statDefinition.displayName + " was not removed from the stat catalog.");
                    }
                }
            }
        }
    }
}
