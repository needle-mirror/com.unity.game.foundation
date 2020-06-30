using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class TagAssetEditor : CollectionEditorBase<TagAsset>
    {
        string m_CurrentTagId;

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.Tags;

        public TagAssetEditor(string name)
            : base(name) { }

        protected override List<TagAsset> GetFilteredItems()
        {
            return m_Items;
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            GameFoundationDatabaseSettings.database.tagCatalog.GetTags(m_Items);
        }

        protected override void SelectItem(TagAsset tag)
        {
            if (tag != null)
            {
                m_ReadableNameKeyEditor = new ReadableNameKeyEditor(false, new HashSet<string>(m_Items.Select(i => i.key)));
                m_CurrentTagId = tag.key;
            }

            base.SelectItem(tag);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameKeyEditor = new ReadableNameKeyEditor(true, new HashSet<string>(m_Items.Select(i => i.key)));
        }

        protected override void AddItem(TagAsset tag)
        {
            var catalog = GameFoundationDatabaseSettings.database.tagCatalog;
            catalog.Editor_AddTag(tag);
            CollectionEditorTools.AssetDatabaseAddObject(tag, catalog);
        }

        protected override void CreateNewItemFinalize()
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("Could not create new tag because the Game Foundation database is null.");
                return;
            }

            if (GameFoundationDatabaseSettings.database.tagCatalog == null)
            {
                Debug.LogError("Could not create new tag because the tag catalog is null.");
                return;
            }

            var tagAsset = TagAsset.Editor_Create(m_NewItemKey);
            if (tagAsset is null)
            {
                return;
            }

            AddItem(tagAsset);
            SelectItem(tagAsset);

            m_CurrentTagId = m_NewItemKey;

            RefreshItems();

            DrawDetail(tagAsset);
        }

        protected override void DrawCreateInputFields()
        {
            string disableDisplayName = null;
            m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_NewItemKey, ref disableDisplayName);

            if (m_ClickedCreateButton)
            {
                EditorGUI.FocusTextInControl("key");
            }

            // display name is unused, but base class requires it not be empty to enable 'create' button so we fake it.
            m_NewItemDisplayName = "ignored non-null string";
        }

        protected override void DrawDetail(TagAsset tag)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                string disableDisplayName = null;
                m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_CurrentTagId, ref disableDisplayName);
            }
        }

        protected override void DrawSidebarListItem(TagAsset tag)
        {
            BeginSidebarItem(tag, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(tag.key, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(tag);

            EndSidebarItem();
        }

        /// <summary>
        ///     Remove <paramref name="tagAsset" /> from catalog.
        /// </summary>
        /// <param name="tagAsset">
        ///     Tag asset to remove.
        /// </param>
        protected override void OnRemoveItem(TagAsset tagAsset)
        {
            if (tagAsset == null)
            {
                return;
            }

            CollectionEditorTools.RemoveObjectFromCatalogAsset(tagAsset);
            Object.DestroyImmediate(tagAsset, true);
            m_TagFilterEditor.ResetTagFilter();
        }
    }
}
