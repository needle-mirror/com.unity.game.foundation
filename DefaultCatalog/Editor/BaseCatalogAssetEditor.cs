using System.Collections.Generic;
using UnityEditor.GameFoundation.DefaultCatalog.Details;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Base editor class for <see cref="BaseCatalogAsset" />.
    /// </summary>
    /// <typeparam name="TCatalogItemAsset">
    ///     The type of <see cref="CatalogItemAsset" /> stored in.
    /// </typeparam>
    abstract class BaseCatalogAssetEditor<TCatalogItemAsset> : CollectionEditorBase<TCatalogItemAsset>
        where TCatalogItemAsset : CatalogItemAsset
    {
        PropertiesEditor<TCatalogItemAsset> m_StaticPropertiesEditor
            = new PropertiesEditor<TCatalogItemAsset>();

        readonly TagPickerEditor m_TagPicker;

        protected string m_CurrentItemKey;

        protected abstract BaseCatalogAsset assetCatalog { get; }

        protected BaseCatalogAssetEditor(string name)
            : base(name)
        {
            m_TagPicker = new TagPickerEditor(GameFoundationDatabaseSettings.database.inventoryCatalog);
        }

        public sealed override void RefreshItems()
        {
            base.RefreshItems();

            m_TagPicker.RefreshTags();

            m_TagFilterEditor.RefreshSidebarTagFilterList(m_TagPicker.tags);

            if (GameFoundationDatabaseSettings.database != null)
                FillCatalogItems();
        }

        protected sealed override List<TCatalogItemAsset> GetFilteredItems()
        {
            if (m_TagPicker == null)
                return null;

            return m_TagFilterEditor.GetFilteredItems(m_Items, m_TagPicker.tags);
        }

        protected sealed override void CreateNewItem()
        {
            var database = GameFoundationDatabaseSettings.database;
            if (database == null)
            {
                Debug.LogError("Could not create a new catalog item because the Game Foundation database is null.");

                return;
            }

            var oldKeys = new HashSet<string>();
            var catalogItems = new List<CatalogItemAsset>();

            database.currencyCatalog.GetItems(catalogItems);
            foreach (var item in catalogItems)
            {
                oldKeys.Add(item.key);
            }

            database.inventoryCatalog.GetItems(catalogItems);
            foreach (var item in catalogItems)
            {
                oldKeys.Add(item.key);
            }

            database.storeCatalog.GetItems(catalogItems);
            foreach (var item in catalogItems)
            {
                oldKeys.Add(item.key);
            }

            database.transactionCatalog.GetItems(catalogItems);
            foreach (var item in catalogItems)
            {
                oldKeys.Add(item.key);
            }

            database.gameParameterCatalog.GetItems(catalogItems);
            foreach (var item in catalogItems)
            {
                oldKeys.Add(item.key);
            }

            m_ReadableNameKeyEditor = new ReadableNameKeyEditor(true, oldKeys);
        }

        protected sealed override void CreateNewItemFinalize()
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("Could not create new item because the Game Foundation database is null.");
                return;
            }

            var catalog = assetCatalog;
            if (catalog == null)
            {
                Debug.LogError("Could not create new item because the catalog is null.");
                return;
            }

            var catalogItemAsset = ScriptableObject.CreateInstance<TCatalogItemAsset>();
            catalogItemAsset.Editor_Initialize(catalog, m_NewItemKey, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(catalogItemAsset, catalog);

            // If filter is currently set to a tag, add that tag to the tag list of the item currently being created
            var currentFilteredTag = m_TagFilterEditor.GetCurrentFilteredTag(m_TagPicker.tags);

            if (currentFilteredTag != null)
            {
                var existingItemTags = new List<TagAsset>();
                catalogItemAsset.GetTags(existingItemTags);

                var isNewTag = true;
                foreach (var existingTag in existingItemTags)
                {
                    if (existingTag.key == currentFilteredTag.key)
                    {
                        isNewTag = false;
                        break;
                    }
                }

                if (isNewTag)
                    catalogItemAsset.Editor_AddTag(currentFilteredTag);
            }

            AddItem(catalogItemAsset);
            SelectItem(catalogItemAsset);

            m_CurrentItemKey = m_NewItemKey;

            RefreshItems();

            DrawGeneralDetail(catalogItemAsset);
        }

        protected sealed override void AddItem(TCatalogItemAsset catalogItem)
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"{typeof(TCatalogItemAsset).Name} {catalogItem.displayName} could not be added because the Game Foundation database is null");

                return;
            }

            var catalog = assetCatalog;
            if (catalog == null)
            {
                Debug.LogError($"{typeof(TCatalogItemAsset).Name} {catalogItem.displayName} could not be added because the catalog is null");

                return;
            }

            AddCatalogItem(catalogItem);
        }

        protected sealed override void DrawSidebarList()
        {
            EditorGUILayout.Space();

            m_TagFilterEditor.DrawTagFilter(out var tagChanged);
            if (tagChanged)
            {
                var selectedTag = m_TagFilterEditor.GetCurrentFilteredTag(m_TagPicker.tags);
                if (selectedItem == null || selectedTag == null || !selectedItem.HasTag(selectedTag))
                {
                    SelectFilteredItem(0);
                }
            }

            base.DrawSidebarList();
        }

        protected override void DrawSidebarListItem(TCatalogItemAsset catalogItem)
        {
            BeginSidebarItem(catalogItem, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(catalogItem.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(catalogItem);

            EndSidebarItem();
        }

        protected sealed override void DrawDetail(TCatalogItemAsset catalogItem)
        {
            DrawGeneralDetail(catalogItem);

            EditorGUILayout.Space();

            m_TagPicker.DrawTagPicker(catalogItem);

            EditorGUILayout.Space();

            DrawTypeSpecificBlocks(catalogItem);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(PropertiesEditor.staticPropertiesLabel, GameFoundationEditorStyles.titleStyle);
            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                m_StaticPropertiesEditor.Draw();
            }

            EditorGUILayout.Space();

            DetailEditorGUI.DrawDetailView(catalogItem);

            // make sure this is the last to draw
            m_TagPicker.DrawTagPickerPopup(catalogItem);
        }

        protected override void SelectItem(TCatalogItemAsset catalogItem)
        {
            base.SelectItem(catalogItem);

            m_TagPicker.ResetTagSearch();

            if (catalogItem != null)
            {
                var oldKeys = new HashSet<string>();
                foreach (var item in m_Items)
                {
                    oldKeys.Add(item.key);
                }

                m_ReadableNameKeyEditor = new ReadableNameKeyEditor(false, oldKeys);
                m_CurrentItemKey = catalogItem.key;
            }

            m_StaticPropertiesEditor.SelectItem(catalogItem);
        }

        protected sealed override void OnRemoveItem(TCatalogItemAsset catalogItemAsset)
        {
            CollectionEditorTools.RemoveObjectFromCatalogAsset(catalogItemAsset);
            Object.DestroyImmediate(catalogItemAsset, true);
        }

        /// <summary>
        ///     Draw additional fields of the given <paramref name="catalogItem" />
        ///     that must appear in the General block.
        /// </summary>
        /// <param name="catalogItem">
        ///     Currently edited item.
        /// </param>
        protected virtual void DrawGeneralFields(TCatalogItemAsset catalogItem) { }

        /// <summary>
        ///     Draw additional blocks that are specific to the type of the current item.
        /// </summary>
        /// <param name="catalogItem">
        ///     The catalog item to draw additional blocks for.
        /// </param>
        protected virtual void DrawTypeSpecificBlocks(TCatalogItemAsset catalogItem) { }

        protected virtual void DrawGeneralDetail(TCatalogItemAsset catalogItem)
        {
            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var displayName = catalogItem.displayName;
                m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_CurrentItemKey, ref displayName);

                if (catalogItem.displayName != displayName)
                {
                    catalogItem.Editor_SetDisplayName(displayName);
                    EditorUtility.SetDirty(catalogItem);
                }

                DrawGeneralFields(catalogItem);
            }
        }

        /// <summary>
        ///     Fill <see cref="CollectionEditorBase{T}.m_Items" /> with the items stored in the edited catalog.
        /// </summary>
        protected abstract void FillCatalogItems();

        /// <summary>
        ///     Add the given <paramref name="catalogItem" /> to the edited catalog.
        /// </summary>
        /// <param name="catalogItem">
        ///     The <see cref="TCatalogItemAsset" /> to add to the edited catalog.
        /// </param>
        protected abstract void AddCatalogItem(TCatalogItemAsset catalogItem);
    }
}
