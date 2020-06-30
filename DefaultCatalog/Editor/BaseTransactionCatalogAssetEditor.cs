using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using UObject = UnityEngine.Object;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    abstract class BaseTransactionCatalogAssetEditor<TTransactionEditor, TTransactionAsset>
        : BaseCatalogAssetEditor<TTransactionAsset>
        where TTransactionEditor : BaseTransactionCatalogAssetEditor<TTransactionEditor, TTransactionAsset>
        where TTransactionAsset : BaseTransactionAsset
    {
        protected override BaseCatalogAsset assetCatalog
            => GameFoundationDatabaseSettings.database.transactionCatalog;

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.Transactions;

        protected BaseTransactionCatalogAssetEditor(string name)
            : base(name) { }

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

        static void DrawCurrencyExchange(TransactionExchangeDefinitionObject exchangeDefinition)
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

        static void DrawItemExchanges(TransactionExchangeDefinitionObject exchangeDefinition)
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
    }
}
