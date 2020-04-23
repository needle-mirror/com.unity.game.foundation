using System.Collections.Generic;
using UnityEngine.GameFoundation.CatalogManagement;
using System.Linq;
using UnityEngine;
using System;

using UObject = UnityEngine.Object;

namespace UnityEditor.GameFoundation
{
    internal abstract class BaseTransactionAssetEditor<TTransactionEditor, TTransactionAsset>
        : CollectionEditorBase<TTransactionAsset>
        where TTransactionEditor : BaseTransactionAssetEditor<TTransactionEditor, TTransactionAsset>
        where TTransactionAsset : BaseTransactionAsset
    {
        private string m_CurrentItemId;

        private readonly CategoryPickerEditor m_CategoryPicker;

        public BaseTransactionAssetEditor(string name) : base(name)
        {
            m_CategoryPicker = new CategoryPickerEditor
                (GameFoundationDatabaseSettings.database.m_TransactionCatalog);
        }

        public override void RefreshItems()
        {
            base.RefreshItems();

            m_CategoryPicker.RefreshCategories();

            m_CategoryFilterEditor.RefreshSidebarCategoryFilterList(m_CategoryPicker.categories);

            if(GameFoundationDatabaseSettings.database != null
                && GameFoundationDatabaseSettings.database.transactionCatalog != null)
            {
                GetCatalogItems(GetItems());
            }
        }

        public override void OnWillEnter()
        {
            base.OnWillEnter();

            SelectFilteredItem(0); // Select the first Item
            GameFoundationAnalytics.SendOpenTabEvent(GameFoundationAnalytics.TabName.Transactions);
        }

        protected override List<TTransactionAsset> GetFilteredItems()
        {
            if(m_CategoryPicker == null) return null;
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
                Debug.LogError("Could not create new IAP transaction definition because the Game Foundation database is null.");
                return;
            }

            var catalog = GameFoundationDatabaseSettings.database.transactionCatalog;

            if (catalog == null)
            {
                Debug.LogError("Could not create new IAP transaction definition because the transaction catalog is null.");
                return;
            }

            var transaction = ScriptableObject.CreateInstance<TTransactionAsset>();
            transaction.Editor_Initialize
                (catalog, m_NewItemId, m_NewItemDisplayName);

            CollectionEditorTools.AssetDatabaseAddObject(transaction, catalog);

            // If filter is currently set to a category, add that category to the category list of the item currently being created
            var currentFilteredCategory = m_CategoryFilterEditor.GetCurrentFilteredCategory(m_CategoryPicker.categories);

            if(currentFilteredCategory != null)
            {
                var existingItemCategories = new List<CategoryAsset>();
                transaction.GetCategories(existingItemCategories);

                if(existingItemCategories.All(category => category.id != currentFilteredCategory.id))
                {
                    transaction.Editor_AddCategory(currentFilteredCategory);
                }
            }

            EditorUtility.SetDirty(catalog);
            AddItem(transaction);
            SelectItem(transaction);
            m_CurrentItemId = m_NewItemId;
            RefreshItems();
            DrawGeneralDetail(transaction);
        }

        protected override void AddItem(TTransactionAsset transaction)
        {
            AddCatalogItem(transaction);
            EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
        }

        protected override void DrawDetail(TTransactionAsset transaction, int index, int count)
        {
            DrawGeneralDetail(transaction);

            EditorGUILayout.Space();

            m_CategoryPicker.DrawCategoryPicker(transaction);

            EditorGUILayout.Space();

            DrawExchanges(transaction);

            EditorGUILayout.Space();

            DetailEditorGUI.DrawDetailView(transaction);

            // make sure this is the last to draw
            m_CategoryPicker.DrawCategoryPickerPopup(transaction);
        }

        private void DrawGeneralDetail(TTransactionAsset transaction)
        {
            if(GameFoundationDatabaseSettings.database == null
                || GameFoundationDatabaseSettings.database.transactionCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using(new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var displayName = transaction.displayName;
                m_ReadableNameIdEditor.DrawReadableNameIdFields(ref m_CurrentItemId, ref displayName);

                if(transaction.displayName != displayName)
                {
                    transaction.Editor_SetDisplayName(displayName);
                    EditorUtility.SetDirty(transaction);
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

        protected override void DrawSidebarListItem(TTransactionAsset transaction)
        {
            BeginSidebarItem(transaction, new Vector2(210f, 30f), new Vector2(5f, 7f));

            DrawSidebarItemLabel(transaction.displayName, 210, GameFoundationEditorStyles.boldTextStyle);

            DrawSidebarItemRemoveButton(transaction);

            EndSidebarItem();
        }

        protected override void SelectItem(TTransactionAsset transaction)
        {
            m_CategoryPicker.ResetCategorySearch();

            if(transaction != null)
            {
                m_ReadableNameIdEditor = new ReadableNameIdEditor(false, new HashSet<string>(GetItems().Select(i => i.id)));
                m_CurrentItemId = transaction.id;
            }

            base.SelectItem(transaction);
        }

        protected override void OnRemoveItem(TTransactionAsset transactionAsset)
        {
            CollectionEditorTools.RemoveObjectFromCatalogAsset(transactionAsset);
            UObject.DestroyImmediate(transactionAsset, true);
        }

        protected abstract void DrawExchanges(TTransactionAsset transaction);

        protected void DrawRewards(TTransactionAsset transaction)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUI.SetNextControlName("Rewards");
                DrawExchangeDefinition(transaction.rewards, "Rewards");
            }
        }

        protected void DrawExchangeDefinition(TransactionExchangeDefinitionObject exchangeDefinitionObject, string title)
        {
            EditorGUILayout.LabelField(title, GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                DrawCurrencyExchange(exchangeDefinitionObject);
                DrawItemExchanges(exchangeDefinitionObject);
            }
        }

        void DrawCurrencyExchange(TransactionExchangeDefinitionObject exchangeDefinition)
        {
            EditorGUILayout.LabelField("Currencies");

            var availableCurrencies = GameFoundationDatabaseSettings.database.currencyCatalog.GetItems();
            var availableCurrencyNames = availableCurrencies.Select(currency => currency.displayName).ToArray();

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                if (exchangeDefinition.m_Currencies != null)
                {
                    CurrencyExchangeObject toRemove = null;
                    foreach (var currency in exchangeDefinition.m_Currencies)
                    {
                        var index = Array.IndexOf(availableCurrencies, currency.currency);
                        using (new GUILayout.HorizontalScope())
                        {
                            var newIndex = EditorGUILayout.Popup(index, availableCurrencyNames);
                            if (newIndex != index)
                            {
                                currency.m_Currency = availableCurrencies[newIndex];
                                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                            }
                            var amount = EditorGUILayout.LongField(currency.m_Amount);
                            if (amount != currency.amount)
                            {
                                currency.m_Amount = amount;
                                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                            }

                            var click = GUILayout.Button((string)null, GameFoundationEditorStyles.deleteButtonStyle);
                            if (click)
                            {
                                toRemove = currency;
                            }
                        }
                    }

                    if (toRemove != null)
                    {
                        exchangeDefinition.m_Currencies.Remove(toRemove);
                        EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                    }
                }

                {
                    var click = GUILayout.Button("+");
                    if (click)
                    {
                        var currencyExchange = new CurrencyExchangeObject();
                        if (exchangeDefinition.m_Currencies == null)
                        {
                            exchangeDefinition.m_Currencies = new List<CurrencyExchangeObject>();
                        }
                        exchangeDefinition.m_Currencies.Add(currencyExchange);
                        EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                    }
                }
            }
        }

        void DrawItemExchanges(TransactionExchangeDefinitionObject exchangeDefinition)
        {
            EditorGUILayout.LabelField("Items");

            var availableItems = GameFoundationDatabaseSettings.database.inventoryCatalog.GetItems();
            var availableItemNames = availableItems.Select(item => item.displayName).ToArray();

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                if (exchangeDefinition.m_Items != null)
                {
                    ItemExchangeDefinitionObject toRemove = null;
                    foreach (var item in exchangeDefinition.m_Items)
                    {
                        var index = Array.IndexOf(availableItems, item.item);
                        using (new GUILayout.HorizontalScope())
                        {
                            var newIndex = EditorGUILayout.Popup(index, availableItemNames);
                            if (newIndex != index)
                            {
                                item.m_Item = availableItems[newIndex];
                                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                            }
                            var amount = EditorGUILayout.LongField(item.m_Amount);
                            if (amount != item.amount)
                            {
                                item.m_Amount = amount;
                                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                            }

                            var click = GUILayout.Button((string)null, GameFoundationEditorStyles.deleteButtonStyle);
                            if (click)
                            {
                                toRemove = item;
                            }
                        }
                    }

                    if (toRemove != null)
                    {
                        exchangeDefinition.m_Items.Remove(toRemove);
                        EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                    }
                }

                {
                    var click = GUILayout.Button("+");
                    if (click)
                    {
                        var itemExchangeObject = new ItemExchangeDefinitionObject();
                        if (exchangeDefinition.m_Items == null)
                        {
                            exchangeDefinition.m_Items = new List<ItemExchangeDefinitionObject>();
                        }
                        exchangeDefinition.m_Items.Add(itemExchangeObject);
                        EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                    }
                }
            }
        }

        protected abstract TTransactionAsset[] GetCatalogItems();

        protected abstract void GetCatalogItems(ICollection<TTransactionAsset> target);

        protected abstract void AddCatalogItem(TTransactionAsset transaction);
    }
}
