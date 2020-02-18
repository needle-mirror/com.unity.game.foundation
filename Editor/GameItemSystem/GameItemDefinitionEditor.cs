using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    internal class GameItemDefinitionEditor : CollectionEditorBase<GameItemDefinition>
    {
        private string m_CurrentItemId;

        public GameItemDefinitionEditor(string name) : base(name)
        {
        }
        
        protected override List<GameItemDefinition> GetFilteredItems()
        {
            return GetItems();
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            if (GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.gameItemCatalog != null)
            {
                GameFoundationDatabaseSettings.database.gameItemCatalog.GetGameItemDefinitions(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            if (GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.gameItemCatalog != null)
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
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("Could not create new game item definition because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationDatabaseSettings.database.gameItemCatalog == null)
            {
                Debug.LogError("Could not create new game item definition because the game item catalog is null.");
                return;
            }

            GameItemDefinition gameItemDefinition = GameItemDefinition.Create(m_NewItemId, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(gameItemDefinition, GameFoundationDatabaseSettings.database.gameItemCatalog);

            EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.gameItemCatalog);

            AddItem(gameItemDefinition);
            SelectItem(gameItemDefinition);
            m_CurrentItemId = m_NewItemId;
            RefreshItems();
            DrawGeneralDetail(gameItemDefinition);
        }

        protected override void AddItem(GameItemDefinition gameItemDefinition)
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"Game Item Definition {gameItemDefinition.displayName} could not be added because the Game Foundation database is null");
            }
            else if (GameFoundationDatabaseSettings.database.gameItemCatalog == null)
            {
                Debug.LogError($"Game Item Definition {gameItemDefinition.displayName} could not be added because the game item catalog is null");
            }
            else
            {
                GameFoundationDatabaseSettings.database.gameItemCatalog.AddGameItemDefinition(gameItemDefinition);
                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.gameItemCatalog);
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
                var displayName = gameItemDefinition.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);

                if (gameItemDefinition.displayName == displayName)
                {
                    return;
                }

                gameItemDefinition.displayName = displayName;
                EditorUtility.SetDirty(gameItemDefinition);
            }
        }

        protected override void DrawSidebarListItem(GameItemDefinition gameItemDefinition)
        {
            BeginSidebarItem(gameItemDefinition, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(gameItemDefinition.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(gameItemDefinition);

            EndSidebarItem();
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
            if (gameItemDefinition == null)
            {
                return;
            }

            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"Game Item Definition {gameItemDefinition.displayName} could not be removed because the Game Foundation database is null");
            }
            else if (GameFoundationDatabaseSettings.database.gameItemCatalog == null)
            {
                Debug.LogError($"Game Item Definition {gameItemDefinition.displayName} could not be removed because the game item catalog is null");
            }
            else
            {
                if (GameFoundationDatabaseSettings.database.gameItemCatalog.RemoveGameItemDefinition(gameItemDefinition))
                {
                    CollectionEditorTools.AssetDatabaseRemoveObject(gameItemDefinition);
                    EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.gameItemCatalog);
                }
                else
                {
                    Debug.LogError($"Game Item Definition {gameItemDefinition.displayName} was not removed from the game item catalog.");
                }
            }
        }
    }
}
