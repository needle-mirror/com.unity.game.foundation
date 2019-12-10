using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal class GameItemDefinitionEditor : CollectionEditorBase<GameItemDefinition>
    {
        private string m_CurrentItemId = null;

        public GameItemDefinitionEditor(string name, GameItemEditorWindow window) : base(name, window)
        {
        }
        
        protected override List<GameItemDefinition> GetFilteredItems()
        {
            return GetItems();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            if (GameFoundationSettings.database != null
                && GameFoundationSettings.database.gameItemCatalog != null)
            {
                GameFoundationSettings.database.gameItemCatalog.GetGameItemDefinitions(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            if (GameFoundationSettings.database != null
                && GameFoundationSettings.database.gameItemCatalog != null)
            {
                SelectFilteredItem(0); // Select the first Item
            }
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
        }

        protected override void CreateNewItemFinalize()
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Could not create new game item definition because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationSettings.database.gameItemCatalog == null)
            {
                Debug.LogError("Could not create new game item definition because the game item catalog is null.");
                return;
            }

            GameItemDefinition gameItemDefinition = GameItemDefinition.Create(m_NewItemId, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(gameItemDefinition, GameFoundationSettings.database.gameItemCatalog);

            EditorUtility.SetDirty(GameFoundationSettings.database.gameItemCatalog);

            AddItem(gameItemDefinition);
            SelectItem(gameItemDefinition);
            m_CurrentItemId = m_NewItemId;
            RefreshItems();
            DrawGeneralDetail(gameItemDefinition);
        }

        protected override void AddItem(GameItemDefinition gameItemDefinition)
        {
            if (GameFoundationSettings.database == null)
            {
                Debug.LogError("Game Item Definition " + gameItemDefinition.displayName + " could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationSettings.database.gameItemCatalog == null)
            {
                Debug.LogError("Game Item Definition " + gameItemDefinition.displayName + " could not be added because the game item catalog is null");
            }
            else
            {
                GameFoundationSettings.database.gameItemCatalog.AddGameItemDefinition(gameItemDefinition);
                EditorUtility.SetDirty(GameFoundationSettings.database.gameItemCatalog);
            }
        }

        protected override void DrawDetail(GameItemDefinition gameItemDefinition, int index, int count)
        {
            DrawGeneralDetail(gameItemDefinition);

            EditorGUILayout.Space();

            DetailEditorGUI.DrawDetailView(gameItemDefinition);
        }

        private void DrawGeneralDetail(GameItemDefinition gameItemDefinition)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string displayName = gameItemDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);
                if (gameItemDefinition.displayName != displayName)
                {
                    gameItemDefinition.displayName = displayName;
                    EditorUtility.SetDirty(gameItemDefinition);
                }
            }
        }

        protected override void DrawSidebarListItem(GameItemDefinition gameItemDefinition, int index)
        {
            BeginSidebarItem(gameItemDefinition, index, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(gameItemDefinition.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(gameItemDefinition);

            EndSidebarItem(gameItemDefinition, index);
        }

        protected override void SelectItem(GameItemDefinition item)
        {
            if (item != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentItemId = item.id;
            }

            base.SelectItem(item);
        }

        protected override void OnRemoveItem(GameItemDefinition gameItemDefinition)
        {
            if (gameItemDefinition != null)
            {
                if (GameFoundationSettings.database == null)
                {
                    Debug.LogError("Game Item Definition " + gameItemDefinition.displayName + " could not be removed because the Game Foundation database is null");
                }
                else if (GameFoundationSettings.database.gameItemCatalog == null)
                {
                    Debug.LogError("Game Item Definition " + gameItemDefinition.displayName + " could not be removed because the game item catalog is null");
                }
                else
                {
                    if (GameFoundationSettings.database.gameItemCatalog.RemoveGameItemDefinition(gameItemDefinition))
                    {
                        CollectionEditorTools.AssetDatabaseRemoveObject(gameItemDefinition);
                        EditorUtility.SetDirty(GameFoundationSettings.database.gameItemCatalog);
                    }
                    else
                    {
                        Debug.LogError("Game Item Definition " + gameItemDefinition.displayName + " was not removed from the game item catalog.");
                    }
                }
            }
        }
    }
}
