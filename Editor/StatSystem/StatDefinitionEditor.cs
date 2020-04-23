using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Editor tab for the Stat window that allows creation of Stat definitions.
    /// </summary>
    internal class StatDefinitionEditor : CollectionEditorBase<StatDefinitionAsset>
    {
        private string m_CurrentItemId;
        private int m_SelectedTypePopupIdx;
        private readonly string[] m_TypeNamesForPopup = System.Enum.GetNames( typeof( StatValueType ) );
        private StatValueType m_NewItemValueType;
        private static readonly GUIContent s_ValueTypeLabel = new GUIContent("Value Type", "Values for this stat will be stored as this numerical data type.");


        /// <summary>
        /// Constructor for the StatDefinitionEditor class.
        /// </summary>
        public StatDefinitionEditor(string name) : base(name)
        {
        }
        
        protected override List<StatDefinitionAsset> GetFilteredItems()
        {
            return GetItems();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            if (GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.statCatalog != null)
            {
                GameFoundationDatabaseSettings.database.statCatalog.GetStatDefinitions(GetItems());
            }
        }

        /// <summary>
        /// Override base class method for what happens when the tab is opened.
        /// </summary>
        public override void OnWillEnter()
        {
            base.OnWillEnter();

            if (GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.statCatalog != null)
            {
                SelectFilteredItem(0); // Select the first Item
            }
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
            m_SelectedTypePopupIdx = 0;
            m_NewItemValueType = StatValueType.Int;
        }

        protected override void DrawCreateInputFields()
        {
            base.DrawCreateInputFields();
            DrawValueTypePopup();
        }

        protected override void DrawWarningMessage()
        {
            EditorGUILayout.HelpBox("Once the Create button is clicked, the Id and Value Type cannot be changed.", MessageType.Warning);
        }

        private void DrawValueTypePopup()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(s_ValueTypeLabel, GUILayout.Width(145));
                var newFilterIdx = EditorGUILayout.Popup(m_SelectedTypePopupIdx, m_TypeNamesForPopup);

                if (newFilterIdx == m_SelectedTypePopupIdx)
                {
                    return;
                }

                m_SelectedTypePopupIdx = newFilterIdx;
                System.Enum.TryParse(m_TypeNamesForPopup[m_SelectedTypePopupIdx], out m_NewItemValueType);
            }
        }

        protected override void CreateNewItemFinalize()
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("Could not create new stat definition because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationDatabaseSettings.database.statCatalog == null)
            {
                Debug.LogError("Could not create new stat definition because the stat catalog is null.");
                return;
            }

            var statDefinition = StatDefinitionAsset.Editor_Create
                (m_NewItemId, m_NewItemDisplayName, m_NewItemValueType);

            statDefinition.catalog = GameFoundationDatabaseSettings.database.statCatalog;

            CollectionEditorTools.AssetDatabaseAddObject(
                statDefinition,
                GameFoundationDatabaseSettings.database.statCatalog);

            AddItem(statDefinition);
            SelectItem(statDefinition);
            RefreshItems();
            var statDefinitions = GetItems();
            DrawDetail(statDefinition, statDefinitions.FindIndex(x => x.Equals(m_SelectedItem)), statDefinitions.Count);
        }

        protected override void AddItem(StatDefinitionAsset statDefinition)
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"Stat Definition {statDefinition.displayName} could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationDatabaseSettings.database.statCatalog == null)
            {
                Debug.LogError($"Stat Definition {statDefinition.displayName} could not be added because the stat catalog is null");
            }
            else
            {
                GameFoundationDatabaseSettings.database.statCatalog.Editor_AddStatDefinition(statDefinition);
                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.statCatalog);
            }
        }

        protected override void DrawDetail(StatDefinitionAsset item, int index, int count)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var displayName = item.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);

                if (item.displayName != displayName)
                {
                    item.Editor_SetDiplayName(displayName);
                    EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.statCatalog);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(s_ValueTypeLabel, GUILayout.Width(145));
                    EditorGUILayout.SelectableLabel(item.statValueType.ToString(), GUILayout.Height(15), GUILayout.ExpandWidth(true));
                }
            }
        }

        protected override void DrawSidebarListItem(StatDefinitionAsset statDefinition)
        {
            BeginSidebarItem(statDefinition, new Vector2(220f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(statDefinition.displayName, 220, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(statDefinition);

            EndSidebarItem();
        }

        protected override void SelectItem(StatDefinitionAsset statDefinition)
        {
            if (statDefinition != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentItemId = statDefinition.id;
                m_SelectedTypePopupIdx = (int)statDefinition.statValueType;
            }

            base.SelectItem(statDefinition);
        }

        protected override void OnRemoveItem(StatDefinitionAsset statDefinitionAsset)
        {
            AssetDatabase.RemoveObjectFromAsset(statDefinitionAsset);
            EditorUtility.SetDirty(statDefinitionAsset.catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Object.DestroyImmediate(statDefinitionAsset, true);
        }
    }
}
