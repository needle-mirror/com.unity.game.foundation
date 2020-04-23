using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    internal class CurrencyAssetEditor : CollectionEditorBase<CurrencyAsset>
    {
        private string m_CurrentItemId;

        private readonly CategoryPickerEditor m_CategoryPicker;

        public CurrencyAssetEditor(string name) : base(name)
        {
            m_CategoryPicker = new CategoryPickerEditor
                (GameFoundationDatabaseSettings.database.currencyCatalog);
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            m_CategoryPicker.RefreshCategories();

            m_CategoryFilterEditor.RefreshSidebarCategoryFilterList(m_CategoryPicker.categories);

            if(GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.currencyCatalog != null)
            {
                GameFoundationDatabaseSettings.database.currencyCatalog.GetItems(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            SelectFilteredItem(0); // Select the first Item
            GameFoundationAnalytics.SendOpenTabEvent(GameFoundationAnalytics.TabName.Currencies);
        }

        protected override List<CurrencyAsset> GetFilteredItems()
        {
            return m_CategoryFilterEditor.GetFilteredItems(GetItems(), m_CategoryPicker.categories);
        }

        protected override void CreateNewItem()
        {
            m_ReadableNameIdEditor = new ReadableNameIdEditor(true, new HashSet<string>(GetItems().Select(i => i.id)));
        }

        protected override void DrawCreateInputFields()
        {
            m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_NewItemId, ref m_NewItemDisplayName);

            if(m_ClickedCreateButton)
            {
                EditorGUI.FocusTextInControl("displayName");
            }
        }

        protected override void CreateNewItemFinalize()
        {
            if(GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("Could not create new currency because the Game Foundation database is null.");
                return;
            }

            if(GameFoundationDatabaseSettings.database.currencyCatalog == null)
            {
                Debug.LogError("Could not create new currency because the currency catalog is null.");
                return;
            }

            var currencyAsset = ScriptableObject.CreateInstance<CurrencyAsset>();
            currencyAsset.m_Catalog = GameFoundationDatabaseSettings.database.currencyCatalog;
            currencyAsset.Editor_SetId(m_NewItemId);
            currencyAsset.Editor_SetDisplayName(m_NewItemDisplayName);
            currencyAsset.name = currencyAsset.Editor_AssetName;

            CollectionEditorTools.AssetDatabaseAddObject(currencyAsset, GameFoundationDatabaseSettings.database.currencyCatalog);

            // If filter is currently set to a category, add that category to the category list of the item currently being created
            var currentFilteredCategory = m_CategoryFilterEditor.GetCurrentFilteredCategory(m_CategoryPicker.categories);

            if(currentFilteredCategory != null)
            {
                var existingItemCategories = new List<CategoryAsset>();
                currencyAsset.GetCategories(existingItemCategories);

                if(existingItemCategories.All(category => category.id != currentFilteredCategory.id))
                {
                    currencyAsset.Editor_AddCategory(currentFilteredCategory);
                }
            }

            EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.currencyCatalog);
            AddItem(currencyAsset);
            SelectItem(currencyAsset);
            m_CurrentItemId = m_NewItemId;
            RefreshItems();
            DrawGeneralDetail(currencyAsset);
        }

        protected override void AddItem(CurrencyAsset currencyAsset)
        {
            if(GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError($"Currency {currencyAsset.displayName} could not be added because the Game Foundation database is null");
            }
            else if(GameFoundationDatabaseSettings.database.currencyCatalog == null)
            {
                Debug.LogError($"Currency {currencyAsset.displayName} could not be added because the currency catalog is null");
            }
            else
            {
                GameFoundationDatabaseSettings.database.currencyCatalog.Editor_AddItem(currencyAsset);
                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.currencyCatalog);
            }
        }

        protected override void DrawDetail(CurrencyAsset currency, int index, int count)
        {
            DrawGeneralDetail(currency);

            EditorGUILayout.Space();

            m_CategoryPicker.DrawCategoryPicker(currency);

            EditorGUILayout.Space();

            DetailEditorGUI.DrawDetailView(currency);

            // make sure this is the last to draw
            m_CategoryPicker.DrawCategoryPickerPopup(currency);
        }

        private void DrawGeneralDetail(CurrencyAsset currency)
        {
            if(GameFoundationDatabaseSettings.database == null
                || GameFoundationDatabaseSettings.database.currencyCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using(new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var displayName = currency.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);

                if(currency.displayName != displayName)
                {
                    currency.Editor_SetDisplayName(displayName);
                    EditorUtility.SetDirty(currency);
                }

                DrawCurrencyFields(currency);
            }
        }

        void DrawCurrencyFields(CurrencyAsset currency)
        {
            using(new EditorGUILayout.VerticalScope())
            {
                GUI.SetNextControlName("Initial allocation");
                var initialAllocation = EditorGUILayout.LongField("Intial allocation", currency.m_InitialBalance);
                if(GUI.changed)
                {
                    currency.m_InitialBalance = initialAllocation;
                }

                GUI.SetNextControlName("Maximum allocation");
                var maximumAllocation = EditorGUILayout.LongField("Maximum allocation", currency.m_MaximumBalance);
                if(GUI.changed)
                {
                    currency.m_MaximumBalance = maximumAllocation;
                }

                GUI.SetNextControlName("Type");
                var type = EditorGUILayout.EnumPopup("Type", currency.m_Type);
                if(GUI.changed)
                {
                    currency.m_Type = (CurrencyType)type;
                }
            }
        }

        protected override void DrawSidebarList()
        {
            EditorGUILayout.Space();

            m_CategoryFilterEditor.DrawCategoryFilter(out var categoryChanged);

            if(categoryChanged)
            {
                var selectedCategory = m_CategoryFilterEditor.GetCurrentFilteredCategory(m_CategoryPicker.categories);
                if (m_SelectedItem == null || selectedCategory == null || !m_SelectedItem.HasCategory(selectedCategory))
                {
                    SelectFilteredItem(0);
                }
            }

            base.DrawSidebarList();
        }

        protected override void DrawSidebarListItem(CurrencyAsset currency)
        {
            BeginSidebarItem(currency, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(currency.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(currency);

            EndSidebarItem();
        }

        protected override void SelectItem(CurrencyAsset currency)
        {
            m_CategoryPicker.ResetCategorySearch();

            if(currency != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentItemId = currency.id;
            }

            base.SelectItem(currency);
        }

        protected override void OnRemoveItem(CurrencyAsset currencyAsset)
        {
            CollectionEditorTools.RemoveObjectFromCatalogAsset(currencyAsset);
            Object.DestroyImmediate(currencyAsset, true);
        }
    }
}
